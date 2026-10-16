using MarioClone.Features.Economy.Models;
using MarioClone.Features.Economy.UseCases.CollectCoin;

namespace MarioClone.Features.Economy;

public static class EconomyModuleExtensions
{
    public static CollectCoinHandler CollectCoin { get; private set; } = null!;

    public static void AddEconomyModule()
    {
        var state = new EconomyState();
        CollectCoin = new CollectCoinHandler(state);
    }
}
