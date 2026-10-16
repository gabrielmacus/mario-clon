using Godot;

namespace MarioClone.Features.World.UseCases.HitBlock;

// HeadPosition: punto justo arriba de la cabeza del jugador, en coordenadas del mundo
// IsBig: lo decide Player (Locomotion); de él depende si un ladrillo se rompe
public record HitBlockInput(int PlayerId, Vector2 HeadPosition, bool IsBig);
