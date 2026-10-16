using MarioClone.Features.Enemies.UseCases.TouchEnemy;
using MarioClone.Features.Player.Models;
using MarioClone.Features.Player.Events;
using MarioClone.Shared.Messaging;

namespace MarioClone.Features.Player.UseCases.TakeDamage;

public class TakeDamageHandler(PlayerRepository playerRepository) : IEventHandler<PlayerHitByEnemyEvent>
{
    // Reacciona al golpe de un enemigo: resta una vida
    public void Handle(PlayerHitByEnemyEvent @event)
    {
        var player = playerRepository.GetPlayer(@event.PlayerId);
        player.Lives -= 1;

        EventBus.Publish(new PlayerLivesChangedEvent(@event.PlayerId, player.Lives));
    }
}
