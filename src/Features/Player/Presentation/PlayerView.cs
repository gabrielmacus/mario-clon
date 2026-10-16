using System;
using Godot;
using MarioClone.Features.Player.Models;
using MarioClone.Features.Player.UseCases.TakeDamage;
using MarioClone.Shared.Messaging;

namespace MarioClone.Features.Player.Presentation;

// Solo dibuja: los estados (correr, saltar...) los lee en cada frame;
// los efectos de un instante (parpadeo al recibir daño) los escucha por evento
public partial class PlayerView : AnimatedSprite2D
{
    private int _playerId;
    private Tween? _blink;

    public override void _Ready()
    {
        _playerId = GetParent<PlayerController>().PlayerId;
        EventBus.Subscribe<PlayerDamagedEvent>(OnPlayerDamaged);
    }

    public override void _ExitTree()
    {
        EventBus.Unsubscribe<PlayerDamagedEvent>(OnPlayerDamaged);
    }

    public void Render(MovementState state, int facing)
    {
        Play(state switch
        {
            MovementState.Running => "run",
            MovementState.Jumping => "jump",
            MovementState.Falling => "fall",
            MovementState.Dead => "dead",
            _ => "idle",
        });

        FlipH = facing < 0;
    }

    private void OnPlayerDamaged(PlayerDamagedEvent e)
    {
        if (e.PlayerId != _playerId) return;

        // Parpadeo mientras dura la invulnerabilidad (cada ciclo dura 0,16 s)
        _blink?.Kill();
        _blink = CreateTween().SetLoops((int)Math.Ceiling(e.InvulnerableSeconds / 0.16f));
        _blink.TweenProperty(this, "modulate:a", 0.2f, 0.08f);
        _blink.TweenProperty(this, "modulate:a", 1f, 0.08f);
    }
}
