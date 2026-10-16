using Godot;
using MarioClone.Features.Economy.UseCases.CollectCoin;
using MarioClone.Features.Player.UseCases.AddLife;
using MarioClone.Shared.Messaging;

namespace MarioClone.Features.UI.Presentation;

public partial class HudController : CanvasLayer
{
    private Label _coinsLabel = null!;
    private Label _livesLabel = null!;

    public override void _Ready()
    {
        _coinsLabel = GetNode<Label>("MarginContainer/VBoxContainer/CoinsLabel");
        _livesLabel = GetNode<Label>("MarginContainer/VBoxContainer/LivesLabel");

        EventBus.Subscribe<WalletUpdatedEvent>(OnWalletUpdated);
        EventBus.Subscribe<PlayerLivesChangedEvent>(OnLivesChanged);
    }

    public override void _ExitTree()
    {
        EventBus.Unsubscribe<WalletUpdatedEvent>(OnWalletUpdated);
        EventBus.Unsubscribe<PlayerLivesChangedEvent>(OnLivesChanged);
    }

    private void OnWalletUpdated(WalletUpdatedEvent e)
    {
        _coinsLabel.Text = $"MONEDAS: {e.TotalCoins:D2}  SCORE: {e.Score:D6}";
    }

    private void OnLivesChanged(PlayerLivesChangedEvent e)
    {
        _livesLabel.Text = $"VIDAS: {e.TotalLives}";
    }
}
