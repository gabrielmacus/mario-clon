using MarioClone.Shared.Messaging;

namespace MarioClone.Features.World.UseCases.CompleteLevel;

public record LevelCompletedEvent(int PlayerId) : IEvent;
