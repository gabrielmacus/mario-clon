using MarioClone.Features.Economy.Models;
using MarioClone.Features.Economy.UseCases.CollectCoin;
using MarioClone.Features.Economy.UseCases.GetWallet;

namespace MarioClone.Features.Economy;

public static class EconomyModuleExtensions
{
    public static CollectCoinHandler CollectCoin { get; private set; } = null!;
    public static GetWalletHandler GetWallet { get; private set; } = null!;

    public static void AddEconomyModule()
    {
        var repository = new WalletRepository();
        CollectCoin = new CollectCoinHandler(repository);
        GetWallet = new GetWalletHandler(repository);
    }
}
