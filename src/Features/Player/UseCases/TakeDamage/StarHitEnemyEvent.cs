using MarioClone.Shared.Messaging;

namespace MarioClone.Features.Player.UseCases.TakeDamage;

// El jugador con estrella tocó un enemigo: el que pierde es el enemigo
public record StarHitEnemyEvent(int PlayerId, ulong EnemyId) : IEvent;
