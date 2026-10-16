using System.Collections.Generic;
using Godot;

namespace MarioClone.Features.World.Models;

// Bloques golpeables del nivel, indexados por celda
public class BlockRepository
{
    private readonly Dictionary<Vector2I, BlockData> _blocks = new();

    public void Add(Vector2I cell, BlockData block) => _blocks[cell] = block;

    public BlockData? Get(Vector2I cell) => _blocks.GetValueOrDefault(cell);

    public void Remove(Vector2I cell) => _blocks.Remove(cell);
}
