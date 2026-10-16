using Godot;
using MarioClone.Features.Player.Models;
using MarioClone.Features.Player.UseCases.Locomotion;

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
        if (result.State == MovementState.Dead) Position += Velocity * (float)delta;
        else MoveAndSlide();

        // 4. La View dibuja el estado que decidió el Handler
        _view.Render(result.State, result.Facing);
    }
}
