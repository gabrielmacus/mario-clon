using MarioClone.Shared.Messaging;

namespace MarioClone.Features.Enemies.UseCases.TouchEnemy;

public record PlayerHitByEnemyEvent(int PlayerId) : IEvent;
