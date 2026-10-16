using MarioClone.Features.World.Models;
using MarioClone.Features.World.UseCases.CompleteLevel;
using MarioClone.Features.World.UseCases.HitBlock;
using MarioClone.Features.World.UseCases.LoadLevel;

namespace MarioClone.Features.World;

public static class WorldModuleExtensions
{
    public static LoadLevelHandler LoadLevel { get; private set; } = null!;
    public static HitBlockHandler HitBlock { get; private set; } = null!;
    public static CompleteLevelHandler CompleteLevel { get; private set; } = null!;

    public static void AddWorldModule()
    {
        // Los bloques se recrean en cada carga del nivel (al reintentar, vuelven a estar llenos)
        var blocks = new BlockRepository();
        LoadLevel = new LoadLevelHandler(blocks);
        HitBlock = new HitBlockHandler(blocks);
        CompleteLevel = new CompleteLevelHandler();
    }
}
