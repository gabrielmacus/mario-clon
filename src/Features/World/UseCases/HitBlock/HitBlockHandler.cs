using MarioClone.Features.World.Models;
using MarioClone.Shared.Messaging;

namespace MarioClone.Features.World.UseCases.HitBlock;

// El jugador golpeó un bloque con la cabeza
public class HitBlockHandler(BlockRepository blockRepository)
{
    public void Handle(HitBlockInput input)
    {
        var cell = Grid.ToCell(input.HeadPosition);

        // 1. Sin bloque golpeable o ya usado: no pasa nada
        var block = blockRepository.Get(cell);
        if (block == null || block.IsUsed) return;

        // 2. Ladrillo común: el jugador grande lo rompe; el chico solo lo hace saltar
        if (block.Content == BlockContent.None)
        {
            if (input.IsBig)
            {
                blockRepository.Remove(cell);
                EventBus.Publish(new BlockBrokenEvent(cell));
            }
            else
            {
                EventBus.Publish(new BlockBumpedEvent(cell));
            }
            return;
        }

        // 3. Con contenido: lo suelta y queda usado
        block.IsUsed = true;
        EventBus.Publish(new BlockEmptiedEvent(cell, block.Content, input.PlayerId));
    }
}
