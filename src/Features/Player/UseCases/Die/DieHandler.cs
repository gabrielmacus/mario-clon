using MarioClone.Features.Player.Models;
using MarioClone.Features.Player.UseCases.TakeDamage;
using MarioClone.Shared.Messaging;

namespace MarioClone.Features.Player.UseCases.Die;

// Reacciona a la muerte del jugador (hoy por golpes; a futuro: pozos, tiempo, etc.)
// El movimiento de muerte (quieto, salto y caída) lo hace Locomotion leyendo IsDead
public class DieHandler(PlayerRepository playerRepository) : IEventHandler<PlayerDiedEvent>
{
    public void Handle(PlayerDiedEvent @event)
    {
        // 1. Marcar al jugador como muerto
        playerRepository.GetPlayer(@event.PlayerId).IsDead = true;

        // 2. Sin vidas extra todavía: siempre es fin del juego
        EventBus.Publish(new GameOverEvent(@event.PlayerId));
    }
}
