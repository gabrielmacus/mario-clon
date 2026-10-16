namespace MarioClone.Features.Enemies.Models;

public class EnemyData
{
    public int Health { get; set; } = 1;

    public bool IsDefeated => Health <= 0;
}
