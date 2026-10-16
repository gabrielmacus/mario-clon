using MarioClone.Shared.Messaging;

namespace MarioClone.Features.Player.UseCases.TakeDamage;

public record PlayerDiedEvent(int PlayerId) : IEvent;
