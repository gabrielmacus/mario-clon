using System.Collections.Generic;

namespace MarioClone.Features.Economy.Models;

public class WalletRepository
{
	private readonly Dictionary<int, WalletData> _wallets = new();

	public WalletData GetWallet(int playerId = 1)
	{
		if (!_wallets.TryGetValue(playerId, out var wallet))
		{
			wallet = new WalletData();
			_wallets[playerId] = wallet;
		}
		return wallet;
	}

	public void Reset()
	{
		_wallets.Clear();
	}
}
