using MarioClone.Features.Collectibles.UseCases.CollectItem;
using MarioClone.Features.Collectibles.UseCases.MoveItem;

namespace MarioClone.Features.Collectibles;

public static class CollectiblesModuleExtensions
{
    public static MoveItemHandler MoveItem { get; private set; } = null!;
    public static CollectItemHandler CollectItem { get; private set; } = null!;

    public static void AddCollectiblesModule()
    {
        MoveItem = new MoveItemHandler();
        CollectItem = new CollectItemHandler();
    }
}
