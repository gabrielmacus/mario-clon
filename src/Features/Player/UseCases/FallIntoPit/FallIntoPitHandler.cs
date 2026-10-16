using MarioClone.Features.Player.Events;
using MarioClone.Shared.Messaging;

namespace MarioClone.Features.Player.UseCases.FallIntoPit;

// El jugador cayó debajo del nivel: muere (lo resuelve el caso de uso Die)
public class FallIntoPitHandler
{
    public void Handle(int playerId = 1)
    {
        EventBus.Publish(new PlayerDiedEvent(playerId));
    }
}
