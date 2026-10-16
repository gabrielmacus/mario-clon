using System;
using System.Collections.Generic;
using Godot;
using MarioClone.Features.World.UseCases.GenerateSegment;

namespace MarioClone.Features.World.Presentation;

public partial class WorldSpawner : Node2D
{
    [Export] public PackedScene GroundScene = null!;
    [Export] public PackedScene PlatformScene = null!;
    [Export] public PackedScene CoinScene = null!;
    [Export] public PackedScene EnemyScene = null!;
    [Export] public Node2D Target = null!;          // el jugador
    [Export] public float SpawnAhead = 640f;        // cuánto generar por delante
    [Export] public float DespawnBehind = 640f;     // cuánto conservar por detrás

    private readonly GenerateSegmentHandler _generate = new(new Random());
    private readonly Queue<(float EndX, Node2D Root)> _segments = new();
    private float _nextX = 0f;

    public override void _Ready() => Refresh();

    public override void _Process(double delta) => Refresh();

    private void Refresh()
    {
        // 1. Generar tramos nuevos por delante del jugador
        while (_nextX < Target.Position.X + SpawnAhead)
        {
            Spawn(_generate.Handle(_nextX));
        }

        // 2. Liberar los tramos que quedaron atrás
        while (_segments.Count > 0 && _segments.Peek().EndX < Target.Position.X - DespawnBehind)
        {
            _segments.Dequeue().Root.QueueFree();
        }
    }

    private void Spawn(SegmentPlan plan)
    {
        var root = new Node2D();
        AddChild(root);

        Place(root, GroundScene, plan.GroundPosition);
        foreach (var position in plan.Platforms) Place(root, PlatformScene, position);
        foreach (var position in plan.Coins) Place(root, CoinScene, position);
        foreach (var position in plan.Enemies) Place(root, EnemyScene, position);

        _segments.Enqueue((plan.EndX, root));
        _nextX = plan.EndX;
    }

    private static void Place(Node2D parent, PackedScene scene, Vector2 position)
    {
        var node = scene.Instantiate<Node2D>();
        node.Position = position;
        parent.AddChild(node);
    }
}
