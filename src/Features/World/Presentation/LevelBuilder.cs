using System;
using Godot;
using MarioClone.Features.Collectibles.Models;
using MarioClone.Features.Collectibles.Presentation;
using MarioClone.Features.Player;
using MarioClone.Features.Player.Presentation;
using MarioClone.Features.World.Models;
using MarioClone.Features.World.UseCases.HitBlock;
using MarioClone.Features.World.UseCases.LoadLevel;
using MarioClone.Shared.Messaging;
using FileAccess = Godot.FileAccess;

namespace MarioClone.Features.World.Presentation;

// Humble Object: lee el archivo del nivel, le pide el plan al caso de uso y lo arma en Godot.
// También muestra lo que pasa con los bloques (rebote, bloque usado, moneda, ítem) cuando llegan los eventos
public partial class LevelBuilder : Node2D
{
    private const int TileSize = Grid.TileSize;
    private const int FlagPoleOffset = 26;   // columna del palo dentro de assets/flag.png

    [Export] public string LevelPath = "res://levels/1-1.txt";
    [Export] public Texture2D TilesTexture = null!;
    [Export] public Texture2D CloudTexture = null!;
    [Export] public Texture2D HillTexture = null!;
    [Export] public Texture2D BushTexture = null!;
    [Export] public Texture2D FlagTexture = null!;
    [Export] public Texture2D CastleTexture = null!;
    [Export] public PackedScene GoombaScene = null!;
    [Export] public PackedScene CoinScene = null!;
    [Export] public PackedScene ItemScene = null!;
    [Export] public Texture2D CoinTexture = null!;
    [Export] public Node2D PlayerBody = null!;
    [Export] public Camera2D Camera = null!;

    private TileMapLayer _layer = null!;
    private int _sourceId;

    public override void _Ready()
    {
        // 1. El caso de uso traduce el texto del nivel a un plan
        var plan = WorldModuleExtensions.LoadLevel.Handle(FileAccess.GetFileAsString(LevelPath));

        // 2. Bloques, entidades y zona de caída
        BuildTiles(plan);
        foreach (var entity in plan.Entities) Place(entity);
        AddKillZone(plan.Height * TileSize + 64);

        // 3. Jugador en su punto de partida y cámara hasta el final del nivel
        PlayerBody.Position = new Vector2(plan.PlayerStart.X * TileSize + TileSize / 2, (plan.PlayerStart.Y + 1) * TileSize);
        Camera.LimitRight = plan.Width * TileSize;

        // 4. Lo que pasa con los bloques llega por evento (efectos de un instante)
        EventBus.Subscribe<BlockBumpedEvent>(OnBlockBumped);
        EventBus.Subscribe<BlockEmptiedEvent>(OnBlockEmptied);
    }

    public override void _ExitTree()
    {
        EventBus.Unsubscribe<BlockBumpedEvent>(OnBlockBumped);
        EventBus.Unsubscribe<BlockEmptiedEvent>(OnBlockEmptied);
    }

    // Arma el TileSet desde assets/tiles.png (una columna por TileKind, todos sólidos) y pinta el nivel
    private void BuildTiles(LevelPlan plan)
    {
        var tileSet = new TileSet { TileSize = new Vector2I(TileSize, TileSize) };
        tileSet.AddPhysicsLayer();
        tileSet.SetPhysicsLayerCollisionLayer(0, 1);   // capa 1 = mundo

        var source = new TileSetAtlasSource { Texture = TilesTexture, TextureRegionSize = new Vector2I(TileSize, TileSize) };
        _sourceId = tileSet.AddSource(source);

        float half = TileSize / 2f;
        var square = new[] { new Vector2(-half, -half), new Vector2(half, -half), new Vector2(half, half), new Vector2(-half, half) };

        foreach (var kind in Enum.GetValues<TileKind>())
        {
            var atlasCoords = new Vector2I((int)kind, 0);
            source.CreateTile(atlasCoords);
            var data = source.GetTileData(atlasCoords, 0);
            data.AddCollisionPolygon(0);
            data.SetCollisionPolygonPoints(0, 0, square);
        }

        _layer = new TileMapLayer { TileSet = tileSet };
        AddChild(_layer);

        foreach (var tile in plan.Tiles)
        {
            _layer.SetCell(tile.Cell, _sourceId, new Vector2I((int)tile.Kind, 0));
        }
    }

    private void Place(EntityPlacement entity)
    {
        var topLeft = new Vector2(entity.Cell.X * TileSize, entity.Cell.Y * TileSize);
        var center = topLeft + new Vector2(TileSize / 2, TileSize / 2);
        float bottom = (entity.Cell.Y + 1) * TileSize;   // la decoración se apoya en el borde inferior de su celda

        switch (entity.Kind)
        {
            case EntityKind.Goomba: Spawn(GoombaScene, center); break;
            case EntityKind.Coin: Spawn(CoinScene, center); break;
            case EntityKind.Cloud: AddDecor(CloudTexture, topLeft); break;
            case EntityKind.Hill: AddDecor(HillTexture, new Vector2(topLeft.X, bottom - HillTexture.GetHeight())); break;
            case EntityKind.Bush: AddDecor(BushTexture, new Vector2(topLeft.X, bottom - BushTexture.GetHeight())); break;
            case EntityKind.Castle: AddDecor(CastleTexture, new Vector2(topLeft.X, bottom - CastleTexture.GetHeight())); break;
            case EntityKind.Flag: AddFlag(center.X, bottom); break;
        }
    }

    private void Spawn(PackedScene scene, Vector2 position)
    {
        var node = scene.Instantiate<Node2D>();
        node.Position = position;
        AddChild(node);
    }

    // Decoración: solo se dibuja, detrás de todo y sin colisión
    private void AddDecor(Texture2D texture, Vector2 position)
    {
        AddChild(new Sprite2D { Texture = texture, Centered = false, Position = position, ZIndex = -1 });
    }

    // Mástil: el sprite se apoya sobre la celda y un detector cubre todo el palo
    private void AddFlag(float poleX, float bottom)
    {
        float height = FlagTexture.GetHeight();
        AddDecor(FlagTexture, new Vector2(poleX - FlagPoleOffset, bottom - height));

        var area = new Area2D { CollisionLayer = 0, CollisionMask = 2, Position = new Vector2(poleX, bottom - height / 2) };
        area.AddChild(new CollisionShape2D { Shape = new RectangleShape2D { Size = new Vector2(4, height) } });
        area.BodyEntered += body =>
        {
            if (body is PlayerController player) WorldModuleExtensions.CompleteLevel.Handle(player.PlayerId);
        };
        AddChild(area);
    }

    // Línea invisible debajo del nivel: el jugador que la cruza cayó a un pozo
    private void AddKillZone(float y)
    {
        var area = new Area2D { CollisionLayer = 0, CollisionMask = 2 | 4 | 8, Position = new Vector2(0, y) };
        area.AddChild(new CollisionShape2D { Shape = new WorldBoundaryShape2D() });
        area.BodyEntered += body =>
        {
            if (body is PlayerController player) PlayerModuleExtensions.FallIntoPit.Handle(player.PlayerId);
            else body.QueueFree();   // un enemigo o ítem que cayó: se borra (la baja en el repositorio llega en _ExitTree)
        };
        AddChild(area);
    }

    private void OnBlockBumped(BlockBumpedEvent e) => Bump(e.Cell, TileKind.Brick);

    private void OnBlockEmptied(BlockEmptiedEvent e)
    {
        Bump(e.Cell, TileKind.Used);

        var cellCenter = new Vector2(e.Cell.X * TileSize + TileSize / 2, e.Cell.Y * TileSize + TileSize / 2);
        switch (e.Content)
        {
            case BlockContent.Coin: PopCoin(cellCenter); break;
            case BlockContent.Mushroom: SpawnItem(ItemKind.Mushroom, cellCenter); break;
            case BlockContent.OneUp: SpawnItem(ItemKind.OneUp, cellCenter); break;
            case BlockContent.Star: SpawnItem(ItemKind.Star, cellCenter); break;
        }
    }

    // Rebote: el tile se reemplaza un instante por un sprite que sube y baja, y vuelve como finalKind
    private void Bump(Vector2I cell, TileKind finalKind)
    {
        var atlasCoords = _layer.GetCellAtlasCoords(cell);
        _layer.EraseCell(cell);

        var start = new Vector2(cell.X * TileSize + TileSize / 2, cell.Y * TileSize + TileSize / 2);
        var sprite = new Sprite2D
        {
            Texture = new AtlasTexture { Atlas = TilesTexture, Region = new Rect2(atlasCoords.X * TileSize, 0, TileSize, TileSize) },
            Position = start,
        };
        AddChild(sprite);

        var tween = CreateTween();
        tween.TweenProperty(sprite, "position:y", start.Y - 6, 0.08);
        tween.TweenProperty(sprite, "position:y", start.Y, 0.08);
        tween.TweenCallback(Callable.From(() =>
        {
            _layer.SetCell(cell, _sourceId, new Vector2I((int)finalKind, 0));
            sprite.QueueFree();
        }));
    }

    // La moneda del bloque: sube, baja un poco y desaparece (el conteo lo hace Economy por evento)
    private void PopCoin(Vector2 cellCenter)
    {
        var coin = new Sprite2D
        {
            Texture = new AtlasTexture { Atlas = CoinTexture, Region = new Rect2(0, 0, 8, 16) },
            Position = cellCenter - new Vector2(0, TileSize),
        };
        AddChild(coin);

        var tween = CreateTween();
        tween.TweenProperty(coin, "position:y", coin.Position.Y - 40, 0.25);
        tween.TweenProperty(coin, "position:y", coin.Position.Y - 16, 0.15);
        tween.TweenCallback(Callable.From(coin.QueueFree));
    }

    // El ítem nace adentro del bloque; su controller lo hace subir y moverse
    private void SpawnItem(ItemKind kind, Vector2 cellCenter)
    {
        var item = ItemScene.Instantiate<ItemController>();
        item.Kind = kind;
        item.Position = cellCenter;
        AddChild(item);
    }
}
