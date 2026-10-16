namespace MarioClone.Features.Player.Models;

public class PlayerData
{
    // Datos de la partida: sobreviven a perder una vida
    public int Lives { get; set; } = 3;

    // Datos de esta vida: se reinician en cada intento del nivel
    public PowerLevel CurrentPowerLevel { get; set; } = PowerLevel.Small;
    public bool IsBig => CurrentPowerLevel != PowerLevel.Small;
    public float InvulnerableTimeLeft { get; set; } = 0f;   // segundos sin poder ser dañado (tras un golpe)
    public bool IsInvulnerable => InvulnerableTimeLeft > 0;
    public float StarTimeLeft { get; set; } = 0f;           // segundos de estrella: invencible, mata enemigos al tocarlos
    public bool IsInvincible => StarTimeLeft > 0;
    public bool BounceRequested { get; set; } = false;
    public bool IsDead { get; set; } = false;
    public float DeathElapsed { get; set; } = 0f;          // segundos desde que murió (para la animación de muerte)
    public bool HasFinished { get; set; } = false;         // llegó a la bandera: ya no responde a los controles

    public void StartNewLife()
    {
        CurrentPowerLevel = PowerLevel.Small;   // como en el original: cada vida arranca chico
        InvulnerableTimeLeft = 0f;
        StarTimeLeft = 0f;
        BounceRequested = false;
        IsDead = false;
        DeathElapsed = 0f;
        HasFinished = false;
    }
}
