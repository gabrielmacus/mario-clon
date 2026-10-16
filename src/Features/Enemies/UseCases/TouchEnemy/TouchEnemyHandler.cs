using MarioClone.Features.Enemies.Models;
using MarioClone.Shared.Messaging;

namespace MarioClone.Features.Enemies.UseCases.TouchEnemy;

public class TouchEnemyHandler(EnemyRepository repository)
{
    // Devuelve true si el enemigo quedó derrotado
    public bool Handle(TouchInput input)
    {
        // 1. De costado o desde abajo: el golpe lo recibe el jugador
        if (input.PlayerVelocityY <= 0 || input.PlayerY >= input.EnemyY)
        {
            EventBus.Publish(new PlayerHitByEnemyEvent(input.PlayerId));
            return false;
        }

        // 2. Pisotón: restar vida al enemigo
        var enemy = repository.Get(input.EnemyId);
        enemy.Health -= 1;
        bool defeated = enemy.Health <= 0;

        // 3. Avisar (el jugador rebota; a futuro: puntaje, sonido)
        EventBus.Publish(new EnemyStompedEvent(input.PlayerId, input.EnemyId, defeated));
        return defeated;
    }
}
