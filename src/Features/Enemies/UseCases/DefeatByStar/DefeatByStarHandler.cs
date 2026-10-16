using MarioClone.Features.Enemies.Models;
using MarioClone.Features.Player.UseCases.TakeDamage;
using MarioClone.Shared.Messaging;

namespace MarioClone.Features.Enemies.UseCases.DefeatByStar;

// El jugador con estrella tocó al enemigo: queda derrotado (Walk lo frena y la View muestra el aplastado)
public class DefeatByStarHandler(EnemyRepository repository) : IEventHandler<StarHitEnemyEvent>
{
    public void Handle(StarHitEnemyEvent @event)
    {
        repository.Get(@event.EnemyId).Health = 0;
    }
}
