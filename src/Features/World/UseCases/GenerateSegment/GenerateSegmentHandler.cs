using System;
using System.Collections.Generic;
using Godot;

namespace MarioClone.Features.World.UseCases.GenerateSegment;

public class GenerateSegmentHandler(Random random)
{
    public float Width { get; set; } = 640f;
    public float GroundY { get; set; } = 340f;
    public float LowPlatformY { get; set; } = 270f;   // alcanzable desde el piso
    public float HighPlatformY { get; set; } = 205f;  // alcanzable solo desde una plataforma baja cercana
    public float CoinHeight { get; set; } = 30f;

    public SegmentPlan Handle(float startX)
    {
        var platforms = new List<Vector2>();
        var coins = new List<Vector2>();
        var enemies = new List<Vector2>();

        // 1. Entre 1 y 3 plataformas repartidas a lo ancho del tramo
        int count = random.Next(1, 4);
        float slot = Width / count;

        for (int i = 0; i < count; i++)
        {
            float x = startX + slot * (i + 0.5f);

            // 2. Solo con 3 plataformas quedan lo bastante juntas para subir a una alta
            bool high = count == 3 && i > 0 && random.Next(2) == 0;
            float y = high ? HighPlatformY : LowPlatformY;

            platforms.Add(new Vector2(x, y));
            coins.Add(new Vector2(x, y - CoinHeight));
        }

        // 3. Una moneda suelta a la altura del jugador sobre el piso
        coins.Add(new Vector2(startX + random.Next(40, (int)Width - 40), GroundY - 36));

        // 4. A veces un enemigo sobre el piso, en la mitad de adelante del tramo
        if (random.Next(2) == 0)
        {
            enemies.Add(new Vector2(startX + random.Next((int)Width / 2, (int)Width - 40), GroundY - 28));
        }

        return new SegmentPlan(startX + Width, new Vector2(startX + Width / 2, GroundY), platforms, coins, enemies);
    }
}
