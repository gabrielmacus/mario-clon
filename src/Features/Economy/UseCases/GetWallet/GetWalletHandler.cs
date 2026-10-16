using MarioClone.Features.Economy.Models;

namespace MarioClone.Features.Economy.UseCases.GetWallet;

// Consulta: solo lee, no modifica nada ni publica eventos
public class GetWalletHandler(WalletRepository walletRepository)
{
    public WalletResult Handle(int playerId = 1)
    {
        var wallet = walletRepository.GetWallet(playerId);
        return new WalletResult(wallet.Coins, wallet.Score);
    }
}
