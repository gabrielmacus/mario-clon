using MarioClone.Features.Enemies.Models;

namespace MarioClone.Features.Enemies.UseCases.DespawnEnemy;

// Baja en el estado: se llama desde _ExitTree del nodo (muera como muera)
public class DespawnEnemyHandler(EnemyRepository repository)
{
    public void Handle(ulong enemyId) => repository.Remove(enemyId);
}
