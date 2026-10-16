using MarioClone.Features.Enemies.Models;

namespace MarioClone.Features.Enemies.UseCases.SpawnEnemy;

// Alta en el estado: se llama desde _Ready del nodo
public class SpawnEnemyHandler(EnemyRepository repository)
{
    public void Handle(ulong enemyId) => repository.Add(enemyId);
}
