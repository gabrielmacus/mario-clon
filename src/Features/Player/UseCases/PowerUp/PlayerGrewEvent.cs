using MarioClone.Shared.Messaging;

namespace MarioClone.Features.Player.UseCases.PowerUp;

public record PlayerGrewEvent(int PlayerId) : IEvent;
