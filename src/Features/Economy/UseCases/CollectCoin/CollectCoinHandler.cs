using MarioClone.Features.Economy.Models;
using MarioClone.Features.World.Models;
using MarioClone.Features.World.UseCases.HitBlock;
using MarioClone.Shared.Messaging;

namespace MarioClone.Features.Economy.UseCases.CollectCoin;

public class CollectCoinHandler(WalletRepository walletRepository) : IEventHandler<BlockEmptiedEvent>
{
    // Moneda que sale de un bloque ?
    public void Handle(BlockEmptiedEvent @event)
    {
        if (@event.Content == BlockContent.Coin) Handle(@event.PlayerId);
    }

    // Moneda del nivel (la llama CoinNode) o de un bloque
    public void Handle(int playerId = 1, int value = 1)
    {
        var wallet = walletRepository.GetWallet(playerId);
        wallet.Coins += value;
        wallet.Score += value * 100;

        // 1. Notificar saldo actualizado a los sistemas interesados (UI, Sonido)
        EventBus.Publish(new WalletUpdatedEvent(playerId, wallet.Coins, wallet.Score));

        // 2. Regla de negocio: 100 monedas otorgan 1 vida
        if (wallet.Coins >= 100)
        {
            wallet.Coins -= 100;
            EventBus.Publish(new BonusLifeAwardedEvent(playerId));
            EventBus.Publish(new WalletUpdatedEvent(playerId, wallet.Coins, wallet.Score));
        }
    }
}
