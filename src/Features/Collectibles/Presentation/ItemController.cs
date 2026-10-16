using Godot;
using MarioClone.Features.Collectibles.Models;
using MarioClone.Features.Collectibles.UseCases.MoveItem;
using MarioClone.Features.Player.Presentation;

namespace MarioClone.Features.Collectibles.Presentation;

// Humble Object: sale del bloque, se mueve según MoveItem y avisa cuando el jugador lo agarra
public partial class ItemController : CharacterBody2D
{
    [Export] public ItemKind Kind = ItemKind.Mushroom;

    private float _gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
    private int _direction = 1;

    public override void _Ready()
    {
        GetNode<AnimatedSprite2D>("Sprite").Play(Kind switch
        {
            ItemKind.OneUp => "oneup",
            ItemKind.Star => "star",
            _ => "mushroom",
        });
        GetNode<Area2D>("Hitbox").BodyEntered += OnHitboxBodyEntered;

        // Aparición: sube 16 px desde adentro del bloque y recién ahí empieza a moverse
        SetPhysicsProcess(false);
        var tween = CreateTween();
        tween.TweenProperty(this, "position:y", Position.Y - 16, 0.5);
        tween.TweenCallback(Callable.From(() => SetPhysicsProcess(true)));
    }

    public override void _PhysicsProcess(double delta)
    {
        var result = CollectiblesModuleExtensions.MoveItem.Handle(
            new MoveItemInput(Kind, Velocity, _direction, IsOnFloor(), IsOnWall(), (float)delta, _gravity));
        Velocity = result.Velocity;
        _direction = result.Direction;
        MoveAndSlide();
    }

    private void OnHitboxBodyEntered(Node2D body)
    {
        if (body is not PlayerController player) return;

        CollectiblesModuleExtensions.CollectItem.Handle(Kind, player.PlayerId);
        QueueFree();
    }
}
