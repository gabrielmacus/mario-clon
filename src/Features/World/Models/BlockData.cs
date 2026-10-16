namespace MarioClone.Features.World.Models;

// Un bloque golpeable (ladrillo o ?)
public class BlockData(BlockContent content)
{
    public BlockContent Content { get; } = content;
    public bool IsUsed { get; set; } = false;
}
