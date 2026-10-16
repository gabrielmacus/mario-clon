namespace MarioClone.Features.Economy.Models;

public class PlayerWallet
{
	public int Coins { get; set; } = 0;
	public int Score { get; set; } = 0;

	public void Reset()
	{
		Coins = 0;
		Score = 0;
	}
}
