using Godot;
using MarioClone.Features.Collectibles.Models;

namespace MarioClone.Features.Collectibles.UseCases.MoveItem;

// Hongo y 1UP caminan; la estrella avanza rebotando. Todos dan la vuelta al chocar una pared
public class MoveItemHandler
{
    public float WalkSpeed { get; set; } = 60f;
    public float StarSpeed { get; set; } = 80f;
    public float StarBounceVelocity { get; set; } = -280f;

    public MoveItemResult Handle(MoveItemInput input)
    {
        var velocity = input.CurrentVelocity;

        // 1. Chocó una pared: da la vuelta
        int direction = input.IsOnWall ? -input.Direction : input.Direction;

        // 2. Gravedad
        if (!input.IsOnFloor) velocity.Y += input.Gravity * input.DeltaTime;

        // 3. Avance horizontal (y rebote de la estrella al tocar el piso)
        if (input.Kind == ItemKind.Star)
        {
            velocity.X = direction * StarSpeed;
            if (input.IsOnFloor) velocity.Y = StarBounceVelocity;
        }
        else
        {
            velocity.X = direction * WalkSpeed;
        }

        return new MoveItemResult(velocity, direction);
    }
}
