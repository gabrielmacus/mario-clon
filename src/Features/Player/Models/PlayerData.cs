namespace MarioClone.Features.Player.Models;

public class PlayerData
{
    // Datos de la partida: sobreviven a perder una vida
    public int Lives { get; set; } = 3;
    public PowerLevel CurrentPowerLevel { get; set; } = PowerLevel.Small;

    // Datos de esta vida: se reinician en cada intento del nivel
    public float InvulnerableTimeLeft { get; set; } = 0f;   // segundos que le quedan sin poder ser dañado
    public bool IsInvulnerable => InvulnerableTimeLeft > 0;
    public bool BounceRequested { get; set; } = false;
    public bool IsDead { get; set; } = false;
    public float DeathElapsed { get; set; } = 0f;          // segundos desde que murió (para la animación de muerte)
    public bool HasFinished { get; set; } = false;         // llegó a la bandera: ya no responde a los controles

    public void StartNewLife()
    {
        InvulnerableTimeLeft = 0f;
        BounceRequested = false;
        IsDead = false;
        DeathElapsed = 0f;
        HasFinished = false;
    }
}
