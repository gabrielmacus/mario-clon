using MarioClone.Shared.Messaging;

namespace MarioClone.Features.Player.UseCases.AddLife;

public record PlayerLivesChangedEvent(int PlayerId, int TotalLives) : IEvent;
