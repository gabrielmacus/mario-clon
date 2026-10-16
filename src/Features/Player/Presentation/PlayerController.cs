using Godot;
using MarioClone.Features.Player.Models;
using MarioClone.Features.Player.UseCases.Locomotion;
using MarioClone.Features.World;
using MarioClone.Features.World.UseCases.HitBlock;

namespace MarioClone.Features.Player.Presentation;

public partial class PlayerController : CharacterBody2D
{
    [Export] public int PlayerId = 1;

    // Humble Object: delega la decisión al Handler (Caso de Uso)
    private float _gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
    private int _facing = 1;
    private PlayerView _view = null!;

    public override void _Ready()
    {
        _view = GetNode<PlayerView>("View");
    }

    public override void _PhysicsProcess(double delta)
    {
        // 0. Descontar el tiempo de invulnerabilidad
        PlayerModuleExtensions.UpdateInvulnerability.Handle(PlayerId, (float)delta);

        // 1. Recolectar estímulos del hardware en un DTO inmutable
        var input = new LocomotionInput(
            CurrentVelocity: Velocity,
            Direction: Input.GetAxis("move_left", "move_right"),
            JumpRequested: Input.IsActionJustPressed("jump"),
            IsOnFloor: IsOnFloor(),
            DeltaTime: (float)delta,
            Gravity: _gravity,
            CurrentFacing: _facing
        );

        // 2. El Handler resuelve la matemática y devuelve la velocidad
        var result = PlayerModuleExtensions.Locomotion.Handle(input, PlayerId);
        Velocity = result.Velocity;
        _facing = result.Facing;

        // 3. Aplicar el movimiento: vivo, con colisiones (MoveAndSlide); muerto, atraviesa todo y cae
        if (result.State == MovementState.Dead)
        {
            Position += Velocity * (float)delta;
        }
        else
        {
            MoveAndSlide();
            DetectHeadHit();
        }

        // 4. La View dibuja el estado que decidió el Handler
        _view.Render(result.State, result.Facing);
    }

    // Si chocó algo con la cabeza, avisa al caso de uso HitBlock (él decide si había un bloque golpeable)
    private void DetectHeadHit()
    {
        for (int i = 0; i < GetSlideCollisionCount(); i++)
        {
            var collision = GetSlideCollision(i);
            if (collision.GetNormal().Y > 0.5f)
            {
                var head = new Vector2(GlobalPosition.X, collision.GetPosition().Y - 1);
                WorldModuleExtensions.HitBlock.Handle(new HitBlockInput(PlayerId, head));
                return;
            }
        }
    }
}
