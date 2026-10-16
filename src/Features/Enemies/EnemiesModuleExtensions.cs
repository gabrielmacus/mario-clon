using MarioClone.Features.Enemies.Models;
using MarioClone.Features.Enemies.UseCases.DefeatByStar;
using MarioClone.Features.Enemies.UseCases.DespawnEnemy;
using MarioClone.Features.Enemies.UseCases.SpawnEnemy;
using MarioClone.Features.Enemies.UseCases.TouchEnemy;
using MarioClone.Features.Enemies.UseCases.Walk;
using MarioClone.Shared.Messaging;

namespace MarioClone.Features.Enemies;

public static class EnemiesModuleExtensions
{
    public static SpawnEnemyHandler SpawnEnemy { get; private set; } = null!;
    public static DespawnEnemyHandler DespawnEnemy { get; private set; } = null!;
    public static TouchEnemyHandler TouchEnemy { get; private set; } = null!;
    public static WalkHandler Walk { get; private set; } = null!;
    public static DefeatByStarHandler DefeatByStar { get; private set; } = null!;

    public static void AddEnemiesModule()
    {
        var repository = new EnemyRepository();
        SpawnEnemy = new SpawnEnemyHandler(repository);
        DespawnEnemy = new DespawnEnemyHandler(repository);
        TouchEnemy = new TouchEnemyHandler(repository);
        Walk = new WalkHandler(repository);
        DefeatByStar = new DefeatByStarHandler(repository);

        EventBus.Register(DefeatByStar);
    }
}
