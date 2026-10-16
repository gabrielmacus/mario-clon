using System.Collections.Generic;

namespace MarioClone.Features.Enemies.Models;

public class EnemyRepository
{
    private readonly Dictionary<ulong, EnemyData> _enemies = new();

    public void Add(ulong enemyId) => _enemies[enemyId] = new EnemyData();

    public EnemyData Get(ulong enemyId) => _enemies[enemyId];

    public void Remove(ulong enemyId) => _enemies.Remove(enemyId);
}
