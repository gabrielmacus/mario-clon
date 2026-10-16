# 🎮 MarioClone — Arquitectura de juego para Godot 4 + C#

Juego de plataformas con **scroll lateral infinito** (estilo "dino de Google"), hecho para aprender Godot con C#.
La arquitectura es **independiente del género**: sirve igual para un top-down, un roguelike o un simulador.

> 🤖 **Si sos un agente de IA:** leé también [`AGENTS.md`](AGENTS.md). Ahí están las reglas obligatorias y la forma de trabajar.

---

## Índice

1. [Cómo correrlo](#1-cómo-correrlo)
2. [La idea en una frase](#2-la-idea-en-una-frase)
3. [Glosario: equivalencias con sistemas de gestión](#3-glosario-equivalencias-con-sistemas-de-gestión)
4. [Las tres capas](#4-las-tres-capas)
5. [Dos formas de comunicarse](#5-dos-formas-de-comunicarse)
6. [Ejemplo completo: juntar una moneda](#6-ejemplo-completo-juntar-una-moneda)
7. [Estructura de carpetas](#7-estructura-de-carpetas)
8. [Las piezas, una por una](#8-las-piezas-una-por-una)
9. [Convenciones](#9-convenciones)
10. [Capas de colisión](#10-capas-de-colisión)
11. [Recetas: cómo agregar cosas](#11-recetas-cómo-agregar-cosas)
12. [Decisiones y límites conocidos](#12-decisiones-y-límites-conocidos)

---

## 1. Cómo correrlo

**Requisitos:** Godot **4.7 (.NET)** y el SDK de **.NET 8**.

1. Abrir `project.godot` con Godot.
2. Presionar **F5**.

| Acción | Teclas |
|---|---|
| Moverse | ← → o A / D |
| Saltar | Espacio, W o ↑ |

**Qué hay hoy en el juego:**
- El mundo se genera solo en tramos de 640 px a medida que avanzás, y los tramos que quedan atrás se borran.
- Cada tramo trae piso, 1 a 3 plataformas, monedas y, a veces, un Goomba.
- 100 monedas dan 1 vida.
- Al Goomba lo matás pisándolo (y rebotás). Si te toca de costado, perdés una vida.
- Todavía **no hay game over**: las vidas pueden quedar en negativo.

---

## 2. La idea en una frase

> La lógica del juego se organiza **como un sistema de gestión orientado a eventos**: las entidades viven en repositorios, cada caso de uso es un `Handler` y los hechos importantes se avisan por un `EventBus`. Godot solo aporta los "periféricos": la entrada del teclado, la física y el dibujo en pantalla.

---

## 3. Glosario: equivalencias con sistemas de gestión

| En este proyecto | En un sistema de gestión | Ejemplo |
|---|---|---|
| `[Entity]Data` | Entidad / una fila | `PlayerData`, `WalletData` |
| `[Entity]Repository` | Repositorio / `DbSet` | `PlayerRepository` |
| `[Acción]Handler` | Caso de uso / servicio de aplicación | `CollectCoinHandler` |
| `[Algo]Input` | DTO de request / comando | `LocomotionInput` |
| `[Hecho]Event` | Evento de dominio | `WalletUpdatedEvent` |
| `EventBus` | Bus de mensajes en memoria | — |
| `[Feature]ModuleExtensions` | Registro en DI (`services.AddXxx()`) | `AddPlayerModule()` |
| `GameBootstrap` | `Program.cs` / composition root | — |
| Nodo en `Presentation/` | Controller de una API / adaptador | `PlayerController` |
| `MoveAndSlide()` | `SaveChanges()` | — |
| Escena `.tscn` | Vista (HTML / XAML) | `Player.tscn` |
| Tick de simulación | Cron job (pero con tiempo de juego) | — |

---

## 4. Las tres capas

```
┌────────────────────────────────────────────────────────────────────────┐
│  CAPA 1: PRESENTACIÓN (nodos de Godot como "Humble Objects")           │
│  - Capturan estímulos: teclado, colisiones, tiempo (delta)             │
│  - No calculan ni deciden nada                                         │
│  - Empaquetan los datos en un DTO y llaman al Handler                  │
│  - Aplican el resultado: MoveAndSlide(), QueueFree(), etc.             │
└───────────────────────────────────┬────────────────────────────────────┘
                                    │  1. Llamada directa (F12 navega)
                                    ▼
┌────────────────────────────────────────────────────────────────────────┐
│  CAPA 2: CASOS DE USO (Handlers)                                       │
│  - Aplican las reglas: gravedad, salto, 100 monedas = 1 vida           │
│  - Leen y modifican su repositorio (PlayerRepository, etc.)            │
│  - Publican eventos con lo que pasó: WalletUpdatedEvent, etc.          │
└───────────────────────────────────┬────────────────────────────────────┘
                                    │  2. Eventos por el EventBus (1 a N)
                                    ▼
┌────────────────────────────────────────────────────────────────────────┐
│  CAPA 3: REACCIONES (desacopladas)                                     │
│  - HUD: escucha WalletUpdatedEvent y actualiza el texto                │
│  - Player: AddLifeHandler escucha BonusLifeAwardedEvent                │
│  - A futuro: audio, partículas, logros                                 │
└────────────────────────────────────────────────────────────────────────┘
```

---

## 5. Dos formas de comunicarse

| | Llamada directa (1 a 1) | EventBus (1 a N) |
|---|---|---|
| **Cuándo** | Un nodo necesita que se ejecute una regla | Pasó algo y a otros les puede interesar |
| **Cómo** | `EconomyModuleExtensions.CollectCoin.Handle(...)` | `EventBus.Publish(new WalletUpdatedEvent(...))` |
| **Ventaja** | F12 te lleva directo al código | Quien publica no conoce a quien escucha |
| **Ejemplo** | La moneda le pide a Economy que la cobre | El HUD se entera de que cambió el saldo |

⚠️ El `EventBus` es **solo para hechos puntuales** (saltó, juntó, murió). Nunca para algo que pasa en cada frame, como "la posición cambió".

---

## 6. Ejemplo completo: juntar una moneda

1. El jugador toca una moneda. Godot dispara `BodyEntered` en `CoinNode`.
2. `CoinNode` llama directo al caso de uso: `EconomyModuleExtensions.CollectCoin.Handle(playerId: 1, value: 1)` y se borra (`QueueFree()`).
3. `CollectCoinHandler` busca la billetera en `WalletRepository`, suma la moneda y el puntaje, y publica `WalletUpdatedEvent`.
4. `HudController`, que está suscripto a ese evento, actualiza el texto en pantalla.
5. Si la billetera llega a 100 monedas, el Handler descuenta 100 y publica `BonusLifeAwardedEvent`.
6. `AddLifeHandler` (feature Player) escucha ese evento, suma una vida y publica `PlayerLivesChangedEvent`.
7. El HUD escucha ese otro evento y actualiza las vidas.

Ni la moneda, ni Economy, ni Player saben que el HUD existe.

---

## 7. Estructura de carpetas

```text
mario-clon/
├── AGENTS.md                                  <-- Reglas para agentes de IA
├── README.md                                  <-- Este documento
├── project.godot                              <-- Configuración de Godot (escena principal, teclas)
├── scenes/                                    <-- Escenas (.tscn) = las "vistas"
│   ├── Main.tscn                              <-- Escena principal: compone todo
│   ├── Player.tscn · Goomba.tscn · Coin.tscn
│   ├── Platform.tscn · Ground.tscn · Hud.tscn
│
└── src/
    ├── Shared/                                <-- Infraestructura técnica común
    │   ├── Messaging/
    │   │   ├── IEvent.cs                      <-- Marca de "esto es un evento"
    │   │   ├── IEventHandler.cs               <-- Contrato para Handlers que escuchan eventos
    │   │   └── EventBus.cs                    <-- Bus estático 1 a N
    │   └── Bootstrap/
    │       └── GameBootstrap.cs               <-- Punto de entrada: registra los módulos
    │
    └── Features/                              <-- Un módulo por capacidad del juego
        ├── [Feature]/                         <-- Ej: Player, Economy, Enemies, World
        │   ├── [Feature]ModuleExtensions.cs   <-- Add[Feature]Module(): crea repositorio y Handlers
        │   ├── Models/
        │   │   ├── [Entity]Data.cs            <-- Entidad: un elemento (como una fila)
        │   │   └── [Entity]Repository.cs      <-- Colección indexada por ID (como un DbSet)
        │   ├── UseCases/
        │   │   └── [Acción]/                  <-- Una carpeta por caso de uso
        │   │       ├── [Acción]Handler.cs     <-- La lógica
        │   │       ├── [Algo]Input.cs         <-- DTO de entrada (opcional)
        │   │       └── [Hecho]Event.cs        <-- Eventos que produce este caso de uso
        │   ├── Events/                        <-- Solo eventos producidos por VARIOS casos de uso
        │   └── Presentation/
        │       └── [Entity]Controller.cs      <-- Nodo de Godot (Humble Object)
        │
        ├── Collectibles/   Presentation/CoinNode.cs
        ├── Economy/        Billetera: monedas, puntaje, vida extra
        ├── Enemies/        Repositorio de enemigos, caminar, pisotón/golpe, GoombaController
        ├── Player/         Vidas, movimiento, sumar/restar vidas, PlayerController
        ├── UI/             Presentation/HudController.cs
        └── World/          Generación de tramos (GenerateSegment) y WorldSpawner
```

**Regla de los namespaces:** siguen las carpetas.
`src/Features/Player/UseCases/AddLife/` → `namespace MarioClone.Features.Player.UseCases.AddLife;`

---

## 8. Las piezas, una por una

### 8.1 Entidad y repositorio (`Models/`)

La entidad son los datos de **un** elemento. El repositorio es la **colección**, indexada por ID.

```csharp
public class PlayerData
{
    public int Lives { get; set; } = 3;
}

public class PlayerRepository
{
    private readonly Dictionary<int, PlayerData> _players = new();

    public PlayerData GetPlayer(int playerId = 1) { /* crea si no existe */ }
}
```

El repositorio lleva el nombre de **la entidad que guarda**, no el de la feature: Economy guarda `WalletData`, por eso su repositorio es `WalletRepository`.

### 8.2 Handler (caso de uso)

Recibe sus dependencias por **constructor primario** (C# 12), aplica las reglas y publica eventos.

```csharp
public class CollectCoinHandler(WalletRepository walletRepository)
{
    public void Handle(int playerId = 1, int value = 1)
    {
        var wallet = walletRepository.GetWallet(playerId);
        wallet.Coins += value;
        EventBus.Publish(new WalletUpdatedEvent(playerId, wallet.Coins, wallet.Score));
        // ...
    }
}
```

No existen clases `...UseCase` aparte: **el Handler ES el caso de uso**.

### 8.3 Handler que reacciona a un evento

Implementa `IEventHandler<TEvent>` y se registra con una línea.

```csharp
public class AddLifeHandler(PlayerRepository playerRepository) : IEventHandler<BonusLifeAwardedEvent>
{
    public void Handle(BonusLifeAwardedEvent @event)
    {
        var player = playerRepository.GetPlayer(@event.PlayerId);
        player.Lives += 1;
        EventBus.Publish(new PlayerLivesChangedEvent(@event.PlayerId, player.Lives));
    }
}
```

### 8.4 Registro del módulo y punto de entrada

```csharp
// Features/Player/PlayerModuleExtensions.cs
public static void AddPlayerModule()
{
    var repository = new PlayerRepository();
    AddLife = new AddLifeHandler(repository);
    TakeDamage = new TakeDamageHandler(repository);

    EventBus.Register(AddLife);      // C# deduce el evento desde IEventHandler<T>
    EventBus.Register(TakeDamage);
}

// Shared/Bootstrap/GameBootstrap.cs (nodo dentro de Main.tscn)
public override void _EnterTree()
{
    EventBus.Clear();
    EconomyModuleExtensions.AddEconomyModule();
    PlayerModuleExtensions.AddPlayerModule();
    EnemiesModuleExtensions.AddEnemiesModule();
}
```

Se usa `_EnterTree` porque Godot lo ejecuta en todos los nodos **antes** de cualquier `_Ready`: cuando los demás nodos arrancan, los módulos ya existen.

### 8.5 Controller como "Humble Object"

El nodo **no calcula**: junta datos, llama al Handler y aplica el resultado.
`MoveAndSlide()` es el equivalente a `SaveChanges()`: el Handler decide la velocidad y Godot la aplica resolviendo los choques.

```csharp
public partial class PlayerController : CharacterBody2D
{
    [Export] public int PlayerId = 1;
    private readonly UpdateLocomotionHandler _locomotion = new();

    public override void _PhysicsProcess(double delta)
    {
        var input = new LocomotionInput(
            CurrentVelocity: Velocity,
            Direction: Input.GetAxis("move_left", "move_right"),
            JumpRequested: Input.IsActionJustPressed("jump"),
            IsOnFloor: IsOnFloor(),
            DeltaTime: (float)delta,
            Gravity: _gravity,
            BounceRequested: _bounceRequested
        );

        Velocity = _locomotion.Handle(input, PlayerId);   // decide
        MoveAndSlide();                                    // aplica
    }
}
```

### 8.6 Ciclo de vida: alta en `_Ready`, baja en `_ExitTree`

Godot llama a `_ExitTree()` **siempre** que un nodo sale de la escena: lo mataron, se cayó, lo borró el spawner o cambió el nivel. Por eso:

| Si el nodo... | En `_Ready` | En `_ExitTree` |
|---|---|---|
| tiene datos en un repositorio | se da de alta (`SpawnEnemy`) | se da de baja (`DespawnEnemy`) |
| escucha eventos | `EventBus.Subscribe(...)` | `EventBus.Unsubscribe(...)` |

Si no se respeta, quedan "fantasmas": datos de enemigos que ya no existen o eventos enviados a nodos borrados.

```csharp
public override void _Ready()    => EnemiesModuleExtensions.SpawnEnemy.Handle(GetInstanceId());
public override void _ExitTree() => EnemiesModuleExtensions.DespawnEnemy.Handle(GetInstanceId());
```

`GetInstanceId()` es un ID único que Godot asigna a cada nodo.

### 8.7 EventBus

Está en `src/Shared/Messaging/EventBus.cs`. Métodos:

| Método | Para qué |
|---|---|
| `Register(handler)` | Suscribe un Handler que implementa `IEventHandler<T>` |
| `Subscribe<T>(callback)` / `Unsubscribe<T>(callback)` | Suscribe/desuscribe un método (lo usan los nodos) |
| `Publish(evento)` | Avisa a todos los suscriptores, en orden de registro |
| `Clear()` | Borra todo (se llama al iniciar la escena) |

`Publish` recorre una **copia** de la lista, así un suscriptor puede desuscribirse en medio de un evento sin romper nada.

---

## 9. Convenciones

| Cosa | Nombre | Dónde |
|---|---|---|
| Entidad | `[Entity]Data` | `Models/` |
| Repositorio | `[Entity]Repository` | `Models/` |
| Caso de uso | `[Acción]Handler` | `UseCases/[Acción]/` |
| DTO de entrada | `[Algo]Input` (record) | junto a su Handler |
| Evento | `[Hecho]Event` (record, implementa `IEvent`) | junto al Handler que lo produce |
| Evento de varios casos de uso | `[Hecho]Event` | `[Feature]/Events/` |
| Nodo de Godot | `[Entity]Controller` | `Presentation/` |
| Registro del módulo | `[Feature]ModuleExtensions.Add[Feature]Module()` | raíz de la feature |

Además:
- **Una clase o record por archivo**, con el nombre exacto.
- **Constructores primarios** en toda clase que reciba dependencias.
- **Siempre `using` + nombre corto**, nunca nombres completos (`MarioClone.Features...`) dentro del código.
- **Eventos y Handlers que se refieren a una entidad llevan su ID**: `int PlayerId` (por defecto 1) o `ulong EnemyId`.
- **Carpetas `Events/` sueltas, prohibidas**, salvo la excepción de la tabla.

---

## 10. Capas de colisión

| Capa | Bit (valor en `.tscn`) | Quién está | Choca con (mask) |
|---|---|---|---|
| 1 — Mundo | 1 | Piso, plataformas | — |
| 2 — Jugador | 2 | `Player` | Mundo (1) |
| 3 — Enemigos | 4 | `Goomba` | Mundo (1) |

Los detectores (Area2D) no ocupan capa (`collision_layer = 0`) y miran al jugador (`collision_mask = 2`): la moneda y el `Hitbox` del Goomba.
Jugador y enemigos **se atraviesan**: el contacto lo detecta el `Hitbox` y lo decide `TouchEnemyHandler`.

---

## 11. Recetas: cómo agregar cosas

### Un caso de uso nuevo
1. Crear `Features/[Feature]/UseCases/[Acción]/[Acción]Handler.cs` con constructor primario.
2. Si necesita varios datos de entrada, crear `[Algo]Input.cs` (record) en la misma carpeta.
3. Si produce hechos, crear `[Hecho]Event.cs` en la misma carpeta y publicarlo con `EventBus.Publish`.
4. Crear la instancia en `[Feature]ModuleExtensions` y exponerla como propiedad estática.
5. Llamarlo desde el nodo que corresponda en `Presentation/`.

### Reaccionar a un evento de otra feature
1. Crear un Handler que implemente `IEventHandler<ElEvento>`.
2. Registrarlo con `EventBus.Register(...)` en el `ModuleExtensions` de **tu** feature.
3. Si quien reacciona es un **nodo** (por ejemplo, el HUD): `Subscribe` en `_Ready` y `Unsubscribe` en `_ExitTree`.

### Un enemigo nuevo
- **Mismo comportamiento, otros valores** (Goomba rápido): en Godot, *New Inherited Scene* desde `Goomba.tscn` y cambiar valores exportados.
- **Comportamiento distinto** (uno que vuela): un controller nuevo en `Enemies/Presentation/`, un caso de uso de movimiento nuevo (por ejemplo `UseCases/Fly/`) y reutilizar `EnemyRepository`, `SpawnEnemy`, `DespawnEnemy` y `TouchEnemy`.
- Todos los enemigos van dentro de la feature `Enemies`. Solo se separa en otra feature algo con reglas propias grandes (por ejemplo, un jefe con fases).

### Algo que aparezca en el mundo
1. Crear la escena en `scenes/`.
2. Agregar su lista de posiciones a `SegmentPlan` y decidir dónde va en `GenerateSegmentHandler`.
3. Agregar un `[Export] PackedScene` en `WorldSpawner`, instanciarlo en `Spawn()` y asignarlo en `Main.tscn`.

### Una feature nueva
1. Crear `Features/[Feature]/` con `Models/`, `UseCases/` y `Presentation/` según haga falta.
2. Crear `[Feature]ModuleExtensions.Add[Feature]Module()`.
3. Llamarlo desde `GameBootstrap._EnterTree()`.

---

## 12. Decisiones y límites conocidos

- **Los datos viven fuera de los nodos**, en repositorios. Ideal para cosas únicas y permanentes (vidas, monedas, puntaje) y para simulaciones con muchas entidades. Para entidades que nacen y mueren seguido, es obligatoria la regla de alta/baja de la sección 8.6.
- **Acceso estático:** los Handlers se usan desde cualquier lado a través de `[Feature]ModuleExtensions`. Es simple y navegable, a cambio de ser, en la práctica, global.
- **Inspector de Godot:** los valores de ajuste (`Speed`, `JumpVelocity`...) son propiedades de los Handlers, así que no se ven en el editor. Cuando haga falta (variantes de enemigos), declarar `[Export]` en el controller y pasárselos al Handler en `_Ready`.
- **Más archivos que un proyecto Godot típico.** Es el costo de que cada pieza se entienda sola.
- **Exportación web:** hasta abril de 2026, Godot no exportaba proyectos C# para navegador. Verificarlo en *Proyecto → Exportar* antes de prometer una versión web.
