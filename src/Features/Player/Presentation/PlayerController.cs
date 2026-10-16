using Godot;
using MarioClone.Features.Enemies.UseCases.TouchEnemy;
using MarioClone.Features.Player.UseCases.Locomotion;
using MarioClone.Shared.Messaging;

namespace MarioClone.Features.Player.Presentation;

public partial class PlayerController : CharacterBody2D
{
    [Export] public int PlayerId = 1;

    // Humble Object: delega la decisión al Handler (Caso de Uso)
    private readonly UpdateLocomotionHandler _locomotion = new();
    private float _gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
    private bool _bounceRequested;

    public override void _Ready()
    {
        EventBus.Subscribe<EnemyStompedEvent>(OnEnemyStomped);
    }

    public override void _ExitTree()
    {
        EventBus.Unsubscribe<EnemyStompedEvent>(OnEnemyStomped);
    }

    private void OnEnemyStomped(EnemyStompedEvent e)
    {
        if (e.PlayerId == PlayerId) _bounceRequested = true;
    }

    public override void _PhysicsProcess(double delta)
    {
        // 1. Recolectar estímulos del hardware en un DTO inmutable
        var input = new LocomotionInput(
            CurrentVelocity: Velocity,
            Direction: Input.GetAxis("move_left", "move_right"),
            JumpRequested: Input.IsActionJustPressed("jump"),
            IsOnFloor: IsOnFloor(),
            DeltaTime: (float)delta,
            Gravity: _gravity,
            BounceRequested: _bounceRequested
        );

        // 2. El Handler resuelve la matemática y devuelve la velocidad
        Velocity = _locomotion.Handle(input, PlayerId);
        _bounceRequested = false;

        // 3. El driver físico de Godot aplica el deslizamiento contra paredes y suelo
        MoveAndSlide();
    }
}
