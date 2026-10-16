using MarioClone.Shared.Messaging;

namespace MarioClone.Features.Player.Events;

public record PlayerLivesChangedEvent(int PlayerId, int TotalLives) : IEvent;
