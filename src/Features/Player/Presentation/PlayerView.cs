using System;
using Godot;
using MarioClone.Features.Player.Models;
using MarioClone.Features.Player.UseCases.PowerUp;
using MarioClone.Features.Player.UseCases.TakeDamage;
using MarioClone.Shared.Messaging;

namespace MarioClone.Features.Player.Presentation;

// Solo dibuja: los estados (correr, saltar, tamaño, estrella) los lee en cada frame;
// los efectos de un instante (parpadeo al recibir daño, crecer) los escucha por evento
public partial class PlayerView : AnimatedSprite2D
{
    // Colores de la estrella (van cambiando mientras dura)
    private static readonly Color[] StarColors =
    {
        new(1f, 1f, 1f), new(1f, 0.55f, 0.35f), new(0.55f, 1f, 0.55f), new(1f, 1f, 0.45f),
    };

    private int _playerId;
    private Tween? _blink;
    private bool _growing;

    public override void _Ready()
    {
        _playerId = GetParent<PlayerController>().PlayerId;
        AnimationFinished += OnAnimationFinished;
        EventBus.Subscribe<PlayerDamagedEvent>(OnPlayerDamaged);
        EventBus.Subscribe<PlayerGrewEvent>(OnPlayerGrew);
    }

    public override void _ExitTree()
    {
        EventBus.Unsubscribe<PlayerDamagedEvent>(OnPlayerDamaged);
        EventBus.Unsubscribe<PlayerGrewEvent>(OnPlayerGrew);
    }

    public void Render(MovementState state, int facing, bool isBig, bool isInvincible)
    {
        // 1. Animación según estado y tamaño (mientras crece, se deja terminar "grow")
        if (!_growing)
        {
            string name = state switch
            {
                MovementState.Running => "run",
                MovementState.Jumping => "jump",
                MovementState.Falling => "fall",
                MovementState.Dead => "dead",
                _ => "idle",
            };
            Play(isBig || state == MovementState.Dead ? name : "small_" + name);
        }

        // 2. Hacia dónde mira
        FlipH = facing < 0;

        // 3. Estrella: colores que cambian cada 60 ms
        SelfModulate = isInvincible ? StarColors[(int)(Time.GetTicksMsec() / 60) % StarColors.Length] : Colors.White;
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

    private void OnPlayerGrew(PlayerGrewEvent e)
    {
        if (e.PlayerId != _playerId) return;

        // Crecer: alterna chico y grande un instante (animación sin loop)
        _growing = true;
        Play("grow");
    }

    // La única animación sin loop es "grow": al terminar, vuelve a mandar el estado
    private void OnAnimationFinished() => _growing = false;
}
