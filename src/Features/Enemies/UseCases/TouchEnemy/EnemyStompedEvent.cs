using MarioClone.Shared.Messaging;

namespace MarioClone.Features.Enemies.UseCases.TouchEnemy;

public record EnemyStompedEvent(int PlayerId, ulong EnemyId, bool Defeated) : IEvent;
