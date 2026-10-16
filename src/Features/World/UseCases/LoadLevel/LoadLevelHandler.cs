using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MarioClone.Features.World.Models;

namespace MarioClone.Features.World.UseCases.LoadLevel;

// Traduce el texto de un nivel (un carácter por celda) a un plan con bloques y entidades,
// y registra los bloques golpeables (ladrillos y ?) en el repositorio.
// La leyenda completa está en el README (sección 11b).
public class LoadLevelHandler(BlockRepository blockRepository)
{
    private static readonly Dictionary<char, (TileKind Kind, BlockContent Content)> TileChars = new()
    {
        ['#'] = (TileKind.Ground, BlockContent.None),
        ['B'] = (TileKind.Brick, BlockContent.None),
        ['E'] = (TileKind.Brick, BlockContent.Star),        // ladrillo con estrella
        ['?'] = (TileKind.Question, BlockContent.Coin),
        ['M'] = (TileKind.Question, BlockContent.Mushroom), // ? con hongo
        ['L'] = (TileKind.Question, BlockContent.OneUp),    // ? con 1UP
        ['S'] = (TileKind.Stair, BlockContent.None),
        ['['] = (TileKind.PipeTopLeft, BlockContent.None),
        [']'] = (TileKind.PipeTopRight, BlockContent.None),
        ['('] = (TileKind.PipeBodyLeft, BlockContent.None),
        [')'] = (TileKind.PipeBodyRight, BlockContent.None),
    };

    private static readonly Dictionary<char, EntityKind> EntityChars = new()
    {
        ['g'] = EntityKind.Goomba,
        ['o'] = EntityKind.Coin,
        ['c'] = EntityKind.Cloud,
        ['h'] = EntityKind.Hill,
        ['b'] = EntityKind.Bush,
        ['F'] = EntityKind.Flag,
        ['K'] = EntityKind.Castle,
    };

    public LevelPlan Handle(string levelText)
    {
        var rows = levelText.Replace("\r", "").Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var tiles = new List<TilePlacement>();
        var entities = new List<EntityPlacement>();
        var playerStart = Vector2I.Zero;

        for (int y = 0; y < rows.Length; y++)
        {
            for (int x = 0; x < rows[y].Length; x++)
            {
                char c = rows[y][x];
                var cell = new Vector2I(x, y);

                if (TileChars.TryGetValue(c, out var tile))
                {
                    tiles.Add(new TilePlacement(cell, tile.Kind, tile.Content));
                    if (tile.Kind is TileKind.Brick or TileKind.Question) blockRepository.Add(cell, new BlockData(tile.Content));
                }
                else if (EntityChars.TryGetValue(c, out var entity)) entities.Add(new EntityPlacement(cell, entity));
                else if (c == 'P') playerStart = cell;
            }
        }

        return new LevelPlan(rows.Max(r => r.Length), rows.Length, playerStart, tiles, entities);
    }
}
