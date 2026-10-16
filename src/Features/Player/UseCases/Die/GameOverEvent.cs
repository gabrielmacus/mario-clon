using MarioClone.Shared.Messaging;

namespace MarioClone.Features.Player.UseCases.Die;

public record GameOverEvent(int PlayerId) : IEvent;
