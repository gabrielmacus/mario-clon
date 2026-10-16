using Godot;
using MarioClone.Features.Player.Models;
using MarioClone.Shared.Messaging;

namespace MarioClone.Features.Player.UseCases.Locomotion;

public class UpdateLocomotionHandler(PlayerRepository playerRepository)
{
    public float Speed { get; set; } = 220.0f;
    public float JumpVelocity { get; set; } = -390.0f;
    public float BounceVelocity { get; set; } = -250.0f;
    public float Acceleration { get; set; } = 1200.0f;
    public float Friction { get; set; } = 1400.0f;
    public float DeathFreezeSeconds { get; set; } = 0.5f;
    public float DeathJumpVelocity { get; set; } = -350.0f;

    public LocomotionResult Handle(LocomotionInput input, int playerId = 1)
    {
        var player = playerRepository.GetPlayer(playerId);
        var velocity = input.CurrentVelocity;

        // 0. Muerto: no responde a los controles. Quieto un instante, salta y cae (sin colisiones)
        if (player.IsDead)
        {
            float before = player.DeathElapsed;
            player.DeathElapsed += input.DeltaTime;

            if (player.DeathElapsed < DeathFreezeSeconds) velocity = Vector2.Zero;
            else if (before < DeathFreezeSeconds) velocity = new Vector2(0, DeathJumpVelocity);
            else velocity.Y += input.Gravity * input.DeltaTime;

            return new LocomotionResult(velocity, MovementState.Dead, input.CurrentFacing);
        }

        // 0b. Llegó a la bandera: sin controles (solo gravedad y frenado)
        if (player.HasFinished) input = input with { Direction = 0, JumpRequested = false };

        // 1. Gravedad
        if (!input.IsOnFloor)
        {
            velocity.Y += input.Gravity * input.DeltaTime;
        }

        // 2. Salto (Valida piso y emite evento para Audio / Animación)
        if (input.JumpRequested && input.IsOnFloor)
        {
            velocity.Y = JumpVelocity;
            EventBus.Publish(new PlayerJumpedEvent(playerId));
        }

        // 3. Rebote al pisar un enemigo (se consume una sola vez)
        if (player.BounceRequested)
        {
            velocity.Y = BounceVelocity;
            player.BounceRequested = false;
        }

        // 4. Movimiento Horizontal (Inercia y aceleración)
        if (input.Direction != 0)
        {
            velocity.X = Mathf.MoveToward(velocity.X, input.Direction * Speed, Acceleration * input.DeltaTime);
        }
        else
        {
            velocity.X = Mathf.MoveToward(velocity.X, 0, Friction * input.DeltaTime);
        }

        // 5. Estado (para animaciones y otras reglas)
        MovementState state;
        if (velocity.Y < 0) state = MovementState.Jumping;
        else if (!input.IsOnFloor) state = MovementState.Falling;
        else if (velocity.X != 0) state = MovementState.Running;
        else state = MovementState.Idle;

        // 6. Hacia dónde mira: si no hay dirección, conserva la anterior
        int facing = input.Direction < 0 ? -1 : input.Direction > 0 ? 1 : input.CurrentFacing;

        return new LocomotionResult(velocity, state, facing);
    }
}
