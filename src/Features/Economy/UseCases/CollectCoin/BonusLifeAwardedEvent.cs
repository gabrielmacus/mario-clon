using MarioClone.Shared.Messaging;

namespace MarioClone.Features.Economy.UseCases.CollectCoin;

public record BonusLifeAwardedEvent(int PlayerId = 1) : IEvent;
