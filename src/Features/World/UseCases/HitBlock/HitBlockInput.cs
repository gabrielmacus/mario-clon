using Godot;

namespace MarioClone.Features.World.UseCases.HitBlock;

// HeadPosition: punto justo arriba de la cabeza del jugador, en coordenadas del mundo
public record HitBlockInput(int PlayerId, Vector2 HeadPosition);
