using MarioClone.Features.Collectibles.Models;
using MarioClone.Shared.Messaging;

namespace MarioClone.Features.Collectibles.UseCases.CollectItem;

// El jugador agarró un ítem. Qué efecto tiene lo decide quien escuche el evento
// (1UP: AddLife en Player; hongo y estrella: Parte 3)
public class CollectItemHandler
{
    public void Handle(ItemKind kind, int playerId = 1)
    {
        EventBus.Publish(new ItemCollectedEvent(kind, playerId));
    }
}
