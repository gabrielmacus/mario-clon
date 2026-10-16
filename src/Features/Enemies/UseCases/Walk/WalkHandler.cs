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

        // 2. Chocó una pared (caño, escalera u otro enemigo): da la vuelta
        int direction = input.IsOnWall ? -input.Direction : input.Direction;

        // 3. Camina, salvo que esté derrotado
        velocity.X = defeated ? 0 : direction * Speed;

        // 4. Estado para la View
        return new WalkResult(velocity, direction, defeated ? EnemyStatus.Squashed : EnemyStatus.Walking);
    }
}
