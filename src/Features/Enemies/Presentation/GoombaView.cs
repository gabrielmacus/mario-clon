using Godot;
using MarioClone.Features.Enemies.Models;

namespace MarioClone.Features.Enemies.Presentation;

// Solo dibuja: traduce el estado que decidió el Handler a una animación
public partial class GoombaView : AnimatedSprite2D
{
    public void Render(EnemyStatus status)
    {
        Play(status == EnemyStatus.Squashed ? "squash" : "walk");
    }
}
