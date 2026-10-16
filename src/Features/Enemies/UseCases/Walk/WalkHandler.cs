using Godot;
using MarioClone.Features.Enemies.Models;

namespace MarioClone.Features.Enemies.UseCases.Walk;

public class WalkHandler(EnemyRepository repository)
{
    public float Speed { get; set; } = 60f;

    public WalkResult Handle(WalkInput input)
    {
        var velocity = input.CurrentVelocity;
        bool defeated = repository.Get(input.EnemyId).IsDefeated;

        // 1. Gravedad
        if (!input.IsOnFloor)
        {
            velocity.Y += input.Gravity * input.DeltaTime;
        }

        // 2. Camina hacia la izquierda (hacia el jugador), salvo que esté derrotado
        velocity.X = defeated ? 0 : -Speed;

        // 3. Estado para la View
        return new WalkResult(velocity, defeated ? EnemyStatus.Squashed : EnemyStatus.Walking);
    }
}
