using Godot;

namespace MarioClone.Features.Enemies.UseCases.Walk;

public class WalkHandler
{
    public float Speed { get; set; } = 60f;

    public Vector2 Handle(WalkInput input)
    {
        var velocity = input.CurrentVelocity;

        // 1. Gravedad
        if (!input.IsOnFloor)
        {
            velocity.Y += input.Gravity * input.DeltaTime;
        }

        // 2. Camina siempre hacia la izquierda (hacia el jugador)
        velocity.X = -Speed;

        return velocity;
    }
}
