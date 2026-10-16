using MarioClone.Features.Enemies.UseCases.TouchEnemy;
using MarioClone.Features.Player.Events;
using MarioClone.Features.Player.Models;
using MarioClone.Shared.Messaging;

namespace MarioClone.Features.Player.UseCases.TakeDamage;

public class TakeDamageHandler(PlayerRepository playerRepository) : IEventHandler<PlayerHitByEnemyEvent>
{
    public float InvulnerabilityDuration { get; set; } = 1.5f;

    // Reacciona al golpe de un enemigo
    public void Handle(PlayerHitByEnemyEvent @event)
    {
        var player = playerRepository.GetPlayer(@event.PlayerId);

        // 1. Muerto, recién golpeado o ya en la bandera: no se lo puede dañar
        if (player.IsDead || player.IsInvulnerable || player.HasFinished) return;

        // 2. Con la última vida, el golpe lo mata (la vida la descuenta el caso de uso Die)
        if (player.Lives <= 1)
        {
            EventBus.Publish(new PlayerDiedEvent(@event.PlayerId));
            return;
        }

        // 3. Restar vida y arrancar la invulnerabilidad
        player.Lives -= 1;
        player.InvulnerableTimeLeft = InvulnerabilityDuration;

        // 4. Avisar (la View parpadea durante la invulnerabilidad; el HUD actualiza las vidas)
        EventBus.Publish(new PlayerDamagedEvent(@event.PlayerId, InvulnerabilityDuration));
        EventBus.Publish(new PlayerLivesChangedEvent(@event.PlayerId, player.Lives));
    }
}
