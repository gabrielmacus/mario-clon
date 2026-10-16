using MarioClone.Features.Collectibles.Models;
using MarioClone.Features.Collectibles.UseCases.CollectItem;
using MarioClone.Features.Player.Models;
using MarioClone.Shared.Messaging;

namespace MarioClone.Features.Player.UseCases.PowerUp;

// Efecto del hongo y la estrella (el 1UP lo resuelve AddLife)
public class PowerUpHandler(PlayerRepository playerRepository) : IEventHandler<ItemCollectedEvent>
{
    public float StarDuration { get; set; } = 10f;

    public void Handle(ItemCollectedEvent @event)
    {
        var player = playerRepository.GetPlayer(@event.PlayerId);

        switch (@event.Kind)
        {
            // Hongo: si es chico, crece (si ya es grande, no pasa nada)
            case ItemKind.Mushroom when !player.IsBig:
                player.CurrentPowerLevel = PowerLevel.Super;
                EventBus.Publish(new PlayerGrewEvent(@event.PlayerId));
                break;

            // Estrella: invencible por un rato (la View lo muestra leyendo el estado)
            case ItemKind.Star:
                player.StarTimeLeft = StarDuration;
                break;
        }
    }
}
