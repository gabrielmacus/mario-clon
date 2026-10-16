using MarioClone.Shared.Messaging;

namespace MarioClone.Features.World.UseCases.CompleteLevel;

// El jugador llegó a la bandera
public class CompleteLevelHandler
{
    public void Handle(int playerId = 1)
    {
        EventBus.Publish(new LevelCompletedEvent(playerId));
    }
}
