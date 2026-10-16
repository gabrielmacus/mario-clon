using MarioClone.Shared.Messaging;

namespace MarioClone.Features.Player.UseCases.Die;

public record LevelRestartEvent(int PlayerId) : IEvent;
