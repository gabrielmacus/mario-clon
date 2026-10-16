using System.Collections.Generic;

namespace MarioClone.Features.Economy.Models;

public class EconomyState
{
	private readonly Dictionary<int, PlayerWallet> _wallets = new();

	public PlayerWallet GetWallet(int playerId = 1)
	{
		if (!_wallets.TryGetValue(playerId, out var wallet))
		{
			wallet = new PlayerWallet();
			_wallets[playerId] = wallet;
		}
		return wallet;
	}

	public void Reset()
	{
		_wallets.Clear();
	}
}
