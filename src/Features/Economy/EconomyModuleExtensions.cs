using MarioClone.Features.Economy.Models;
using MarioClone.Features.Economy.UseCases.CollectCoin;
using MarioClone.Features.Economy.UseCases.GetWallet;
using MarioClone.Shared.Messaging;

namespace MarioClone.Features.Economy;

public static class EconomyModuleExtensions
{
    // El repositorio dura toda la partida: las monedas y el puntaje se conservan al perder una vida
    private static WalletRepository? _repository;

    public static CollectCoinHandler CollectCoin { get; private set; } = null!;
    public static GetWalletHandler GetWallet { get; private set; } = null!;

    public static void AddEconomyModule()
    {
        _repository ??= new WalletRepository();
        CollectCoin = new CollectCoinHandler(_repository);
        GetWallet = new GetWalletHandler(_repository);

        EventBus.Register(CollectCoin);
    }

    // Partida nueva (game over o nivel completado): billetera en cero
    public static void ResetSession() => _repository = null;
}
