namespace MarioClone.Features.Enemies.UseCases.TouchEnemy;

public record TouchInput(
    ulong EnemyId,
    int PlayerId,
    float PlayerVelocityY,
    float PlayerY,
    float EnemyY
);
