using Godot;
using MarioClone.Features.Economy;
using MarioClone.Features.Economy.UseCases.CollectCoin;
using MarioClone.Features.Player;
using MarioClone.Features.Player.Events;
using MarioClone.Features.Player.UseCases.Die;
using MarioClone.Shared.Messaging;

namespace MarioClone.Features.UI.Presentation;

public partial class HudController : CanvasLayer
{
    [Export] public int PlayerId = 1;

    private Label _coinsLabel = null!;
    private Label _livesLabel = null!;
    private Label _gameOverLabel = null!;

    public override void _Ready()
    {
        _coinsLabel = GetNode<Label>("MarginContainer/VBoxContainer/CoinsLabel");
        _livesLabel = GetNode<Label>("MarginContainer/VBoxContainer/LivesLabel");
        _gameOverLabel = GetNode<Label>("GameOverLabel");

        // 1. Valores iniciales: se consultan una vez
        var wallet = EconomyModuleExtensions.GetWallet.Handle(PlayerId);
        ShowWallet(wallet.Coins, wallet.Score);
        ShowLives(PlayerModuleExtensions.GetLives.Handle(PlayerId));

        // 2. Cambios posteriores: llegan por evento
        EventBus.Subscribe<WalletUpdatedEvent>(OnWalletUpdated);
        EventBus.Subscribe<PlayerLivesChangedEvent>(OnLivesChanged);
        EventBus.Subscribe<GameOverEvent>(OnGameOver);
    }

    public override void _ExitTree()
    {
        EventBus.Unsubscribe<WalletUpdatedEvent>(OnWalletUpdated);
        EventBus.Unsubscribe<PlayerLivesChangedEvent>(OnLivesChanged);
        EventBus.Unsubscribe<GameOverEvent>(OnGameOver);
    }

    private void OnWalletUpdated(WalletUpdatedEvent e) => ShowWallet(e.TotalCoins, e.Score);

    private void OnLivesChanged(PlayerLivesChangedEvent e) => ShowLives(e.TotalLives);

    // El cartel aparece después de que Mario cae (el juego está pausado: el timer corre igual)
    private void OnGameOver(GameOverEvent e) =>
        GetTree().CreateTimer(2.0, processAlways: true).Timeout += () => _gameOverLabel.Visible = true;

    private void ShowWallet(int coins, int score) => _coinsLabel.Text = $"MONEDAS: {coins:D2}  SCORE: {score:D6}";

    private void ShowLives(int lives) => _livesLabel.Text = $"VIDAS: {lives}";
}
