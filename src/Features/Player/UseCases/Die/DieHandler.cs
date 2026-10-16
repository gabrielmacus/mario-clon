using MarioClone.Features.Player.Events;
using MarioClone.Features.Player.Models;
using MarioClone.Shared.Messaging;

namespace MarioClone.Features.Player.UseCases.Die;

// Reacciona a la muerte del jugador (golpe con la última vida o caída a un pozo).
// El movimiento de muerte (quieto, salto y caída) lo hace Locomotion leyendo IsDead
public class DieHandler(PlayerRepository playerRepository) : IEventHandler<PlayerDiedEvent>
{
    public void Handle(PlayerDiedEvent @event)
    {
        var player = playerRepository.GetPlayer(@event.PlayerId);

        // 1. Ya muerto (por ejemplo, cae al vacío durante la animación): nada que hacer
        if (player.IsDead) return;

        // 2. Morir cuesta una vida
        player.IsDead = true;
        player.Lives -= 1;
        EventBus.Publish(new PlayerLivesChangedEvent(@event.PlayerId, player.Lives));

        // 3. Como en el original: con vidas, se reintenta el nivel; sin vidas, fin del juego
        if (player.Lives > 0) EventBus.Publish(new LevelRestartEvent(@event.PlayerId));
        else EventBus.Publish(new GameOverEvent(@event.PlayerId));
    }
}
