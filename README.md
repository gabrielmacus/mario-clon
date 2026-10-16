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
- Un nivel diseñado, el **1-1** (`levels/1-1.txt`, 200 tiles de largo): piso, ladrillos, bloques `?`, escaleras, caños, pozos, Goombas, monedas, decoración y bandera con castillo.
- 100 monedas dan 1 vida.
- Al Goomba lo matás pisándolo: se aplasta (animación) y rebotás. Los Goombas dan la vuelta al chocar caños, escaleras u otro Goomba.
- Power-ups como en el original: Mario arranca **chico**. El **hongo** lo hace crecer; si un enemigo toca a Mario **grande**, se achica y parpadea; si lo toca **chico**, muere. La **estrella** lo hace invencible 10 s (cambia de color) y mata a los enemigos que toca. Grande, **rompe ladrillos**.
- Todo tiene sprites (en `assets/`, recortados de las hojas de `raw_sprites/`; solo para práctica: son de Nintendo).
- Los bloques se golpean desde abajo: saltan; los `?` sueltan su contenido y quedan usados (marrones). La moneda sale volando y se suma; el hongo y el 1UP caminan, la estrella rebota. El **1UP suma una vida**.
- Al achicarse, Mario queda invulnerable 1,5 s (y parpadea ese tiempo).
- Vidas como en el original: al morir (último golpe o caída a un pozo) el juego se congela, Mario muestra el sprite de muerte, salta y cae. Con vidas de sobra se **reintenta el nivel** (se conservan vidas, monedas y puntaje); sin vidas aparece **GAME OVER** y arranca una partida nueva.
- Al tocar la bandera aparece **¡NIVEL COMPLETADO!** y arranca una partida nueva.

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
│   ├── Player.tscn · Goomba.tscn · Coin.tscn · Hud.tscn
├── levels/                                    <-- Niveles en texto (un carácter por celda)
│   └── 1-1.txt
├── assets/                                    <-- Sprites recortados que usa el juego
├── raw_sprites/                               <-- Hojas originales de donde se recortan
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
        │   ├── Models/                        <-- Entidades, repositorios y enums de la feature
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
        ├── Collectibles/   Ítems: moverse (MoveItem), agarrarlos (CollectItem), CoinNode e ItemController
        ├── Economy/        Billetera: monedas, puntaje, vida extra
        ├── Enemies/        Repositorio de enemigos, caminar, pisotón/golpe, GoombaController
        ├── Player/         Vidas, movimiento, sumar/restar vidas, PlayerController
        ├── UI/             Presentation/HudController.cs
        └── World/          Carga del nivel (LoadLevel), bandera (CompleteLevel) y LevelBuilder
```

**Regla de los namespaces:** siguen las carpetas.
`src/Features/Player/UseCases/AddLife/` → `namespace MarioClone.Features.Player.UseCases.AddLife;`

---

## 8. Las piezas, una por una

### 8.1 Entidad y repositorio (`Models/`)

`Models/` contiene los tipos que describen el dominio de la feature: **entidades, repositorios y enums** (por ejemplo `MovementState`, `EnemyStatus`, `PowerLevel`).

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
    Bounce = new BounceHandler(repository);
    Locomotion = new UpdateLocomotionHandler(repository);

    EventBus.Register(AddLife);      // C# deduce el evento desde IEventHandler<T>
    EventBus.Register(TakeDamage);
    EventBus.Register(Bounce);
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

    public override void _PhysicsProcess(double delta)
    {
        var input = new LocomotionInput(
            CurrentVelocity: Velocity,
            Direction: Input.GetAxis("move_left", "move_right"),
            JumpRequested: Input.IsActionJustPressed("jump"),
            IsOnFloor: IsOnFloor(),
            DeltaTime: (float)delta,
            Gravity: _gravity,
            CurrentFacing: _facing
        );

        var result = PlayerModuleExtensions.Locomotion.Handle(input, PlayerId);   // decide
        Velocity = result.Velocity;
        _facing = result.Facing;
        MoveAndSlide();                                                            // aplica
        _view.Render(result.State, result.Facing);                                 // dibuja
    }
}
```

### 8.6 Ciclo de vida: alta en `_Ready`, baja en `_ExitTree`

Godot llama a `_ExitTree()` **siempre** que un nodo sale de la escena: lo mataron, se cayó, lo borró el spawner o cambió el nivel. Por eso:

| Si el nodo... | En `_Ready` | En `_ExitTree` |
|---|---|---|
| tiene datos en un repositorio | se da de alta (`SpawnEnemy`) | se da de baja (`DespawnEnemy`) |
| escucha eventos (solo Views/HUD, ver 8.9) | `EventBus.Subscribe(...)` | `EventBus.Unsubscribe(...)` |

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

### 8.8 View: cómo se ve (sprites, animaciones, efectos)

La View es un nodo de `Presentation/` que **solo dibuja**. No decide nada: ni reglas, ni estados.
Hay dos tipos de animación, y cada una se dispara distinto:

| Tipo | Ejemplos | Cómo se dispara |
|---|---|---|
| **De estado** (continua) | quieto, corriendo, saltando, cayendo | El Handler devuelve el estado y la View lo **traduce** a una animación en cada frame (sin eventos) |
| **De momento** (una vez) | golpe, pisotón, moneda, muerte | La View **escucha un evento** del `EventBus` |

**De estado:** decidir en qué estado está la entidad es una regla, así que lo decide el Handler. La View solo traduce, como un diccionario.

```csharp
// Player/Models/MovementState.cs
public enum MovementState { Idle, Running, Jumping, Falling }

// Player/UseCases/Locomotion/LocomotionResult.cs
public record LocomotionResult(Vector2 Velocity, MovementState State, int Facing);

// Player/Presentation/PlayerController.cs
var result = _locomotion.Handle(input, PlayerId);
Velocity = result.Velocity;
MoveAndSlide();
_view.Render(result.State, result.Facing);

// Player/Presentation/PlayerView.cs (en un AnimatedSprite2D)
public void Render(MovementState state, int facing)
{
    Play(state switch
    {
        MovementState.Running => "run",
        MovementState.Jumping => "jump",
        MovementState.Falling => "fall",
        _ => "idle",
    });

    FlipH = facing < 0;   // hacia dónde mira también lo decide el Handler
}
```

Además, el estado queda como dato del dominio: otros Handlers pueden usarlo (por ejemplo, "¿estaba en `Falling`?" para decidir un pisotón).

**De momento:** la View se suscribe en `_Ready`, se desuscribe en `_ExitTree` y filtra por su ID.

```csharp
private void OnPunched(EnemyPunchedEvent e)
{
    if (e.EnemyId != GetInstanceId()) return;   // no es conmigo
    if (e.BodyPart == BodyPart.Face) _blood.Emitting = true;
}
```

**Regla de oro:** el nodo detecta **dónde** pasó algo, el Handler decide **qué significa** (y en qué estado queda la entidad) y la View decide **cómo se ve**. El Handler nunca sabe que existe una animación.

El método se llama `Render` y no `Show` porque los nodos de Godot ya tienen un `Show()` (hacerlos visibles).

> Nota: los sprites de Mario están en `assets/mario.png` (grande, chico y muerte; solo para práctica: es arte de Nintendo).

### 8.9 ¿Estado o evento? El criterio

Cada vez que algo tiene que reaccionar a lo que pasa en el juego, la pregunta es: **¿esto dura o pasa una vez?**

| | **Dura** (estado) | **Pasa una vez** (evento) |
|---|---|---|
| Ejemplos | Mario corriendo, Goomba aplastado | Te pegaron, juntaste una moneda, ganaste una vida |
| Quién lo decide | Un Handler, que lo devuelve en su `Result` | Un Handler, que publica el evento |
| Cómo llega a la pantalla | El controller se lo pasa a la View en cada frame (`Render`) | La View o el HUD escuchan el evento |

**Quién se suscribe al `EventBus`:**

| Quién | ¿Se suscribe? | Cómo |
|---|---|---|
| Handlers (lógica) | ✅ | `EventBus.Register(...)` en `[Feature]ModuleExtensions`. **Nunca** desde un nodo |
| Controllers (nodos de física) | ❌ | Reciben todo de los Handlers |
| Views y HUD | ✅ solo para efectos de un instante | `Subscribe` en `_Ready`, `Unsubscribe` en `_ExitTree`, filtrando por ID |
| `GameBootstrap` | ✅ solo para el **flujo de la aplicación** (reiniciar, cambiar de nivel) | `Subscribe` en `_EnterTree`, `Unsubscribe` en `_ExitTree`. Ejemplo: `GameOverEvent` → pausa y recarga la escena |

**Banderas en el repositorio** (por ejemplo `PlayerData.BounceRequested`):
- ✅ **Solo para lógica** que llega por un evento y se aplica en el frame siguiente. La prende un Handler y la consume (y apaga) otro Handler. Ejemplo: el rebote al pisar un enemigo cambia la física.
- ❌ **Nunca para efectos visuales o de sonido** (parpadear, brillar, sangre). El dominio no debe saber cómo se ve el juego, varios interesados se pisarían al apagarla y se acumularían. Para eso, evento a la View.

**Ejemplos del proyecto:**

| Caso | Tipo | Cómo está resuelto |
|---|---|---|
| Mario corre, salta, cae | Dura | `UpdateLocomotionHandler` → `LocomotionResult.State` → `PlayerView.Render` |
| Goomba aplastado | Dura | `WalkHandler` → `WalkResult.Status` → `GoombaView.Render`; el nodo se borra con la señal `AnimationFinished` |
| Rebote al pisar | Lógica | `BounceHandler` (registrado) → `BounceRequested` → lo consume `UpdateLocomotionHandler` |
| Parpadeo al achicarse | Pasa una vez | `TakeDamageHandler` publica `PlayerDamagedEvent` → `PlayerView` lo escucha |
| Crecer con el hongo | Pasa una vez | `PowerUpHandler` (escucha `ItemCollectedEvent`) cambia el poder y publica `PlayerGrewEvent` → `PlayerView` reproduce "grow" |
| Tamaño de Mario | Dura | `LocomotionResult.IsBig` → `PlayerController` ajusta la colisión y `PlayerView` elige animaciones `small_*` o grandes |
| Estrella | Dura | `PowerUpHandler` carga `StarTimeLeft`; `LocomotionResult.IsInvincible` → `PlayerView` cambia los colores |
| Enemigo tocado con estrella | Pasa una vez | `TakeDamage` publica `StarHitEnemyEvent` → `DefeatByStar` (Enemies) lo derrota |
| Romper un ladrillo | Pasa una vez | `HitBlock` (con `IsBig`) publica `BlockBrokenEvent` → `LevelBuilder` borra el tile y lanza los pedazos |
| Monedas y vidas en el HUD | Pasa una vez (cambia de vez en cuando) | Valor inicial con `GetWallet`/`GetLives`; cambios por `WalletUpdatedEvent` / `PlayerLivesChangedEvent` |
| Invulnerabilidad tras un golpe | Lógica | `TakeDamageHandler` la arranca; `UpdateInvulnerabilityHandler` la descuenta cada frame (también la estrella) |
| Muerte de Mario | Pasa una vez (morir) + dura (estar muerto) | `TakeDamage` o `FallIntoPit` publican `PlayerDiedEvent` → `DieHandler` marca `IsDead`; `UpdateLocomotionHandler` lee `IsDead` y hace el movimiento de muerte (`MovementState.Dead`); `PlayerView` muestra "dead" |
| Game over / reintento | Pasa una vez | `Die` descuenta la vida y publica `LevelRestartEvent` (quedan vidas) o `GameOverEvent` → `GameBootstrap` congela y recarga; el HUD muestra el cartel |
| Caída a un pozo | Pasa una vez | La línea de `LevelBuilder` llama a `FallIntoPit`, que publica `PlayerDiedEvent` |
| Bandera | Pasa una vez | `CompleteLevel` publica `LevelCompletedEvent` → `FinishLevel` (Player) quita los controles; HUD y `GameBootstrap` reaccionan |
| Golpear un bloque | Pasa una vez | `PlayerController` detecta el cabezazo y llama a `HitBlock` → `BlockBumpedEvent` o `BlockEmptiedEvent` → `LevelBuilder` hace el rebote, deja el bloque usado y suelta moneda o ítem; Economy suma la moneda |
| Ítem que camina o rebota | Dura | `ItemController` → `MoveItem` (velocidad y dirección) en cada frame |
| Agarrar un ítem | Pasa una vez | `CollectItem` publica `ItemCollectedEvent` → `AddLife` suma una vida si es 1UP |

**Valores iniciales: casos de uso de consulta.** Un evento solo avisa de *cambios*; al arrancar todavía no hubo ninguno. Por eso, quien necesita el valor actual (por ejemplo el HUD en `_Ready`) lo pide **una vez** a un Handler de consulta: `GetLivesHandler`, `GetWalletHandler`. Es una *query* de sistemas de gestión: **solo lee**, no modifica nada ni publica eventos, y devuelve un valor o un `[Algo]Result` (nunca la entidad, para que la presentación no pueda modificarla).

Un **hecho** y su **consecuencia** pueden ser dos eventos distintos sin ser redundantes: `PlayerDamagedEvent` ("te pegaron") y `PlayerLivesChangedEvent` ("ahora tenés X vidas"), igual que `PagoRecibido` y `SaldoActualizado` en un sistema de gestión.

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
| Visual de una entidad | `[Entity]View` | `Presentation/` |
| Resultado de un Handler con varios datos | `[Algo]Result` (record) | junto a su Handler |
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
| 4 — Ítems | 8 | Hongo, 1UP, estrella | Mundo (1) |

Los detectores (Area2D) no ocupan capa (`collision_layer = 0`) y miran al jugador (`collision_mask = 2`): la moneda, el `Hitbox` del Goomba, el de los ítems y la bandera. La línea de los pozos mira a jugador, enemigos e ítems (`2 | 4 | 8`).
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

### Algo nuevo que se ubique en el nivel
1. Crear la escena en `scenes/` (o la textura en `assets/`, si es decoración).
2. Elegir un carácter libre y agregarlo a `LoadLevelHandler` (y a `EntityKind` o `TileKind`).
3. En `LevelBuilder.Place`, decir qué se crea para ese tipo; agregar el `[Export]` y asignarlo en `Main.tscn`.
4. Usar el carácter en `levels/*.txt` y documentarlo en la leyenda (sección 11b).

### 11b. Niveles: formato del archivo

Cada nivel es un `.txt` en `levels/`: **una línea por fila y un carácter por celda de 16×16**. El piso son las filas 20 y 21; la fila 19 es "sobre el piso". Cualquier carácter que no esté en la leyenda es aire (se usa `.`).

| Carácter | Qué es | Tipo |
|---|---|---|
| `#` | Piso | Bloque |
| `B` | Ladrillo | Bloque |
| `E` | Ladrillo con estrella (Parte 2) | Bloque |
| `?` | Bloque `?` con moneda | Bloque |
| `M` | Bloque `?` con hongo (Parte 2) | Bloque |
| `L` | Bloque `?` con 1UP (Parte 2) | Bloque |
| `S` | Bloque de escalera | Bloque |
| `[` `]` | Boca del caño (izquierda, derecha) | Bloque |
| `(` `)` | Cuerpo del caño (izquierda, derecha) | Bloque |
| `P` | Inicio del jugador (los pies sobre el borde inferior de la celda) | — |
| `g` | Goomba | Entidad |
| `o` | Moneda | Entidad |
| `F` | Mástil con bandera (se apoya sobre la celda de abajo) | Entidad |
| `K` | Castillo (esquina inferior izquierda) | Decoración |
| `c` | Nube (esquina superior izquierda) | Decoración |
| `h` | Colina (esquina inferior izquierda) | Decoración |
| `b` | Arbusto (esquina inferior izquierda) | Decoración |

Debajo del nivel hay una línea invisible: quien la cruza cayó a un pozo.

### Una feature nueva
1. Crear `Features/[Feature]/` con `Models/`, `UseCases/` y `Presentation/` según haga falta.
2. Crear `[Feature]ModuleExtensions.Add[Feature]Module()`.
3. Llamarlo desde `GameBootstrap._EnterTree()`.

---

## 12. Decisiones y límites conocidos

- **Datos de la partida y datos de la vida.** `PlayerRepository` y `WalletRepository` se crean **una vez por partida** (no por escena): así vidas, monedas y puntaje sobreviven a la recarga de la escena al reintentar el nivel. Lo de "esta vida" (muerto, invulnerable, rebote, bandera) se reinicia con `PlayerData.StartNewLife()`. Con game over o nivel completado, `GameBootstrap` llama a `ResetSession()` y todo arranca de cero. El resto de los repositorios (enemigos) se recrean en cada carga.

- **Los datos viven fuera de los nodos**, en repositorios. Ideal para cosas únicas y permanentes (vidas, monedas, puntaje) y para simulaciones con muchas entidades. Para entidades que nacen y mueren seguido, es obligatoria la regla de alta/baja de la sección 8.6.
- **Acceso estático:** los Handlers se usan desde cualquier lado a través de `[Feature]ModuleExtensions`. Es simple y navegable, a cambio de ser, en la práctica, global.
- **Inspector de Godot:** los valores de ajuste (`Speed`, `JumpVelocity`...) son propiedades de los Handlers, así que no se ven en el editor. Cuando haga falta (variantes de enemigos), declarar `[Export]` en el controller y pasárselos al Handler en `_Ready`.
- **Más archivos que un proyecto Godot típico.** Es el costo de que cada pieza se entienda sola.
- **Exportación web:** hasta abril de 2026, Godot no exportaba proyectos C# para navegador. Verificarlo en *Proyecto → Exportar* antes de prometer una versión web.
