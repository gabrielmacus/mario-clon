using MarioClone.Features.Enemies.UseCases.TouchEnemy;
using MarioClone.Features.Player.Models;
using MarioClone.Features.Player.Events;
using MarioClone.Shared.Messaging;

namespace MarioClone.Features.Player.UseCases.TakeDamage;

public class TakeDamageHandler(PlayerRepository playerRepository) : IEventHandler<PlayerHitByEnemyEvent>
{
    public float InvulnerabilityDuration { get; set; } = 1.5f;

    // Reacciona al golpe de un enemigo: resta una vida
    public void Handle(PlayerHitByEnemyEvent @event)
    {
        var player = playerRepository.GetPlayer(@event.PlayerId);

        // 1. Muerto o recién golpeado: no se lo puede volver a dañar
        if (player.IsDead || player.IsInvulnerable) return;

        // 2. Restar vida y arrancar la invulnerabilidad
        player.Lives -= 1;
        player.InvulnerableTimeLeft = InvulnerabilityDuration;

        // 3. Avisar (la View parpadea durante la invulnerabilidad; el HUD actualiza las vidas)
        EventBus.Publish(new PlayerDamagedEvent(@event.PlayerId, InvulnerabilityDuration));
        EventBus.Publish(new PlayerLivesChangedEvent(@event.PlayerId, player.Lives));

        // 4. Sin vidas: el jugador muere (lo resuelve el caso de uso Die)
        if (player.Lives <= 0)
        {
            EventBus.Publish(new PlayerDiedEvent(@event.PlayerId));
        }
    }
}
