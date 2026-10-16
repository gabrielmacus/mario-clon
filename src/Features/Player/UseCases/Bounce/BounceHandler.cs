using MarioClone.Features.Enemies.UseCases.TouchEnemy;
using MarioClone.Features.Player.Models;
using MarioClone.Shared.Messaging;

namespace MarioClone.Features.Player.UseCases.Bounce;

// Reacciona al pisotón: deja anotado el rebote, que Locomotion aplica en el próximo frame
public class BounceHandler(PlayerRepository playerRepository) : IEventHandler<EnemyStompedEvent>
{
    public void Handle(EnemyStompedEvent @event)
    {
        playerRepository.GetPlayer(@event.PlayerId).BounceRequested = true;
    }
}
