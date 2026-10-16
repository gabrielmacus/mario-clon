using Godot;
using MarioClone.Features.Enemies.UseCases.TouchEnemy;
using MarioClone.Features.Enemies.UseCases.Walk;
using MarioClone.Features.Player.Presentation;

namespace MarioClone.Features.Enemies.Presentation;

public partial class GoombaController : CharacterBody2D
{
    private readonly WalkHandler _walk = new();
    private float _gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

    public override void _Ready()
    {
        EnemiesModuleExtensions.SpawnEnemy.Handle(GetInstanceId());
        GetNode<Area2D>("Hitbox").BodyEntered += OnHitboxBodyEntered;
    }

    public override void _ExitTree()
    {
        EnemiesModuleExtensions.DespawnEnemy.Handle(GetInstanceId());
    }

    public override void _PhysicsProcess(double delta)
    {
        Velocity = _walk.Handle(new WalkInput(Velocity, IsOnFloor(), (float)delta, _gravity));
        MoveAndSlide();
    }

    private void OnHitboxBodyEntered(Node2D body)
    {
        if (body is not PlayerController player) return;

        var input = new TouchInput(
            EnemyId: GetInstanceId(),
            PlayerId: player.PlayerId,
            PlayerVelocityY: player.Velocity.Y,
            PlayerY: player.GlobalPosition.Y,
            EnemyY: GlobalPosition.Y
        );

        if (EnemiesModuleExtensions.TouchEnemy.Handle(input))
        {
            QueueFree();
        }
    }
}
