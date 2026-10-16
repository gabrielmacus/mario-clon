using MarioClone.Shared.Messaging;

namespace MarioClone.Features.Player.UseCases.TakeDamage;

public record PlayerDamagedEvent(int PlayerId, float InvulnerableSeconds) : IEvent;
