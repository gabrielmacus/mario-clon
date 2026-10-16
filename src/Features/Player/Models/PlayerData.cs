namespace MarioClone.Features.Player.Models;

public class PlayerData
{
    public int Lives { get; set; } = 3;
    public PowerLevel CurrentPowerLevel { get; set; } = PowerLevel.Small;
    public float InvulnerableTimeLeft { get; set; } = 0f;   // segundos que le quedan sin poder ser dañado
    public bool IsInvulnerable => InvulnerableTimeLeft > 0;
    public bool BounceRequested { get; set; } = false;
    public bool IsDead { get; set; } = false;
    public float DeathElapsed { get; set; } = 0f;          // segundos desde que murió (para la animación de muerte)

    public void Reset()
    {
        Lives = 3;
        CurrentPowerLevel = PowerLevel.Small;
        InvulnerableTimeLeft = 0f;
        BounceRequested = false;
        IsDead = false;
        DeathElapsed = 0f;
    }
}
