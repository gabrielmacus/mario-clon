using MarioClone.Features.Enemies.UseCases.TouchEnemy;
using MarioClone.Features.Player.Events;
using MarioClone.Features.Player.Models;
using MarioClone.Shared.Messaging;

namespace MarioClone.Features.Player.UseCases.TakeDamage;

// Reacciona al golpe de un enemigo, como en el original: grande se achica, chico muere
public class TakeDamageHandler(PlayerRepository playerRepository) : IEventHandler<PlayerHitByEnemyEvent>
{
    public float InvulnerabilityDuration { get; set; } = 1.5f;

    public void Handle(PlayerHitByEnemyEvent @event)
    {
        var player = playerRepository.GetPlayer(@event.PlayerId);

        // 1. Muerto, recién golpeado o ya en la bandera: no se lo puede dañar
        if (player.IsDead || player.IsInvulnerable || player.HasFinished) return;

        // 2. Con estrella: el que pierde es el enemigo
        if (player.IsInvincible)
        {
            EventBus.Publish(new StarHitEnemyEvent(@event.PlayerId, @event.EnemyId));
            return;
        }

        // 3. Chico: muere (la vida la descuenta el caso de uso Die)
        if (!player.IsBig)
        {
            EventBus.Publish(new PlayerDiedEvent(@event.PlayerId));
            return;
        }

        // 4. Grande: se achica y queda invulnerable un rato (la View parpadea)
        player.CurrentPowerLevel = PowerLevel.Small;
        player.InvulnerableTimeLeft = InvulnerabilityDuration;
        EventBus.Publish(new PlayerDamagedEvent(@event.PlayerId, InvulnerabilityDuration));
    }
}
