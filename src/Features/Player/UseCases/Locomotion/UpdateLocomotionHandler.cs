using Godot;
using MarioClone.Shared.Messaging;

namespace MarioClone.Features.Player.UseCases.Locomotion;

public class UpdateLocomotionHandler
{
    public float Speed { get; set; } = 220.0f;
    public float JumpVelocity { get; set; } = -390.0f;
    public float Acceleration { get; set; } = 1200.0f;
    public float Friction { get; set; } = 1400.0f;

    public Vector2 Handle(LocomotionInput input, int playerId = 1)
    {
        var velocity = input.CurrentVelocity;

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

        // 3. Movimiento Horizontal (Inercia y aceleración)
        if (input.Direction != 0)
        {
            velocity.X = Mathf.MoveToward(velocity.X, input.Direction * Speed, Acceleration * input.DeltaTime);
        }
        else
        {
            velocity.X = Mathf.MoveToward(velocity.X, 0, Friction * input.DeltaTime);
        }

        return velocity;
    }
}
