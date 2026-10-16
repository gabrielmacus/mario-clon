using MarioClone.Features.Collectibles.Models;
using MarioClone.Shared.Messaging;

namespace MarioClone.Features.Collectibles.UseCases.CollectItem;

public record ItemCollectedEvent(ItemKind Kind, int PlayerId) : IEvent;
