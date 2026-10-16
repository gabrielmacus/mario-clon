namespace MarioClone.Features.Player.Models;

public class PlayerData
{
    public int Lives { get; set; } = 3;
    public PowerLevel CurrentPowerLevel { get; set; } = PowerLevel.Small;
    public bool IsInvulnerable { get; set; } = false;

    public void Reset()
    {
        Lives = 3;
        CurrentPowerLevel = PowerLevel.Small;
        IsInvulnerable = false;
    }
}
