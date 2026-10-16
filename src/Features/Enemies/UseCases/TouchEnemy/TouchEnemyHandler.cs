using MarioClone.Features.Enemies.Models;
using MarioClone.Shared.Messaging;

namespace MarioClone.Features.Enemies.UseCases.TouchEnemy;

public class TouchEnemyHandler(EnemyRepository repository)
{
    public void Handle(TouchInput input)
    {
        var enemy = repository.Get(input.EnemyId);

        // 1. Un enemigo derrotado ya no lastima ni se puede volver a pisar
        if (enemy.IsDefeated) return;

        // 2. De costado o desde abajo: el golpe lo recibe el jugador
        if (input.PlayerVelocityY <= 0 || input.PlayerY >= input.EnemyY)
        {
            EventBus.Publish(new PlayerHitByEnemyEvent(input.PlayerId));
            return;
        }

        // 3. Pisotón: restar vida al enemigo
        enemy.Health -= 1;

        // 4. Avisar (el jugador rebota, la View muestra el aplastado; a futuro: puntaje, sonido)
        EventBus.Publish(new EnemyStompedEvent(input.PlayerId, input.EnemyId, enemy.IsDefeated));
    }
}
