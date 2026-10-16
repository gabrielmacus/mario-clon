using Godot;
using MarioClone.Features.Enemies.UseCases.TouchEnemy;
using MarioClone.Features.Enemies.UseCases.Walk;
using MarioClone.Features.Player.Presentation;

namespace MarioClone.Features.Enemies.Presentation;

public partial class GoombaController : CharacterBody2D
{
    private float _gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
    private GoombaView _view = null!;
    private int _direction = -1;   // arranca hacia el jugador

    public override void _Ready()
    {
        EnemiesModuleExtensions.SpawnEnemy.Handle(GetInstanceId());
        GetNode<Area2D>("Hitbox").BodyEntered += OnHitboxBodyEntered;

        _view = GetNode<GoombaView>("Sprite");

        // Señal de Godot: "squash" es la única animación sin loop. Al terminar, el nodo se borra
        // (la baja en el repositorio llega en _ExitTree)
        _view.AnimationFinished += QueueFree;
    }

    public override void _ExitTree()
    {
        EnemiesModuleExtensions.DespawnEnemy.Handle(GetInstanceId());
    }

    public override void _PhysicsProcess(double delta)
    {
        var result = EnemiesModuleExtensions.Walk.Handle(
            new WalkInput(GetInstanceId(), Velocity, _direction, IsOnFloor(), IsOnWall(), (float)delta, _gravity));
        Velocity = result.Velocity;
        _direction = result.Direction;
        MoveAndSlide();

        // La View dibuja el estado que decidió el Handler
        _view.Render(result.Status);
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

        EnemiesModuleExtensions.TouchEnemy.Handle(input);
    }
}
