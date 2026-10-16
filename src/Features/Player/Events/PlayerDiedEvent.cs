using MarioClone.Shared.Messaging;

namespace MarioClone.Features.Player.Events;

public record PlayerDiedEvent(int PlayerId) : IEvent;
