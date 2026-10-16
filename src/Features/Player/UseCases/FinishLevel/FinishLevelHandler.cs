using MarioClone.Features.Player.Models;
using MarioClone.Features.World.UseCases.CompleteLevel;
using MarioClone.Shared.Messaging;

namespace MarioClone.Features.Player.UseCases.FinishLevel;

// Al llegar a la bandera, el jugador deja de responder a los controles y no puede ser dañado
public class FinishLevelHandler(PlayerRepository playerRepository) : IEventHandler<LevelCompletedEvent>
{
    public void Handle(LevelCompletedEvent @event)
    {
        playerRepository.GetPlayer(@event.PlayerId).HasFinished = true;
    }
}
