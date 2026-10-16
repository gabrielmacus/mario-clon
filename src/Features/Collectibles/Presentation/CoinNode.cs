using Godot;
using MarioClone.Features.Economy;
using MarioClone.Features.Player.Presentation;

namespace MarioClone.Features.Collectibles.Presentation;

public partial class CoinNode : Area2D
{
    [Export] public int Value = 1;

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is PlayerController)
        {
            // Ejecución directa del Handler / Caso de Uso
            EconomyModuleExtensions.CollectCoin.Handle(playerId: 1, value: Value);
            QueueFree();
        }
    }
}
