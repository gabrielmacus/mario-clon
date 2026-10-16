# 🎮 Clean Game Architecture Blueprint (Godot 4 + C# 12)
### Framework Arquitectónico Agnóstico: Vertical Slice, Handlers como Casos de Uso y EventBus

Este documento define la arquitectura estándar, patrones de diseño y directivas de desarrollo para proyectos en **Godot 4 (.NET / C# 12)**. 

Está diseñado para ser **agnóstico al género del juego** (plataformas, top-down, survival, roguelike, RTS) y optimizado para ser interpretado y ejecutado tanto por **desarrolladores de software** como por **Agentes de Inteligencia Artificial (LLMs)**.

---

## 🏛️ Filosofía Central: El Caso de Uso ES el Handler

Para evitar la duplicación de abstracciones (`UseCase` vs `Handler`), **cada Caso de Uso es directamente su clase `Handler`**:
* `CollectCoinHandler.cs` es el Caso de Uso de recolección de monedas.
* `AddLifeHandler.cs` es el Caso de Uso de sumar vidas.
* `UpdateLocomotionHandler.cs` es el Caso de Uso de locomoción y físicas.

Todo se apoya en dos mecanismos claros:

1. **Invocación Directa (1 a 1):**  
   Cuando un nodo de Godot necesita ejecutar lógica, **llama directamente al Handler** (`_locomotion.Handle(...)` o `EconomyModuleExtensions.CollectCoin.Handle(...)`).  
   * **Ventaja:** Navegabilidad instantánea con `F12`, cero indirección, cero mediadores innecesarios para llamadas 1 a 1.
2. **EventBus para Difusión Desacoplada (1 a N):**  
   Cuando un Handler procesa sus reglas y muta su estado local, **emite un Evento al `EventBus`**.  
   * **Ventaja:** La UI (HUD), el Audio, las Partículas y otros módulos reaccionan de forma 100% desacoplada.

```
┌────────────────────────────────────────────────────────────────────────┐
│  CAPA 1: ADAPTADORES / PRESENTACIÓN (Nodos como "Humble Objects")      │
│  - Captura estímulos de hardware (Inputs, Colisiones, Reloj Delta)     │
│  - Cero fórmulas matemáticas y cero lógica de decisión interna         │
│  - Empaqueta los datos en un DTO inmutable y llama al Handler          │
│  - Recibe el resultado y llama al driver físico: MoveAndSlide()        │
└───────────────────────────────────┬────────────────────────────────────┘
                                    │  1. Invocación Directa (F12)
                                    ▼
┌────────────────────────────────────────────────────────────────────────┐
│  CAPA 2: DOMINIO Y CASOS DE USO (Handlers con Constructores Primarios) │
│  - Clases Handler puras en C# 12 con constructores primarios           │
│  - Mutan su PROPIO Estado Interno (PlayerWallet, PlayerState)          │
│  - Aplican reglas de negocio (gravedad, inercia, 100 monedas = 1 vida) │
│  - Emiten Eventos de Estado al EventBus: WalletUpdatedEvent, etc.      │
└───────────────────────────────────┬────────────────────────────────────┘
                                    │  2. Notifica Hechos al EventBus (1 a N)
                                    ▼
┌────────────────────────────────────────────────────────────────────────┐
│  CAPA 3: REACCIÓN Y SISTEMAS SATÉLITE (Desacoplados)                   │
│  - UI (HUD): Escucha WalletUpdatedEvent y actualiza el saldo en pantalla│
│  - Audio: Escucha PlayerJumpedEvent y reproduce "jump.wav"             │
│  - Player: Auto-wired a BonusLifeAwardedEvent mediante su Handler      │
└────────────────────────────────────────────────────────────────────────┘
```

---

## ⚡ Constructores Primarios de C# 12 (Obligatorios)

Para eliminar boilerplate de campos privados y asignaciones manuales, **todas las clases y Handlers deben usar Constructores Primarios de C# 12**:

```csharp
// CORRECTO (C# 12 Primary Constructor):
public class CollectCoinHandler(EconomyState economy)
{
    public void Handle(int playerId = 1, int value = 1)
    {
        var wallet = economy.GetWallet(playerId);
        // ...
    }
}
```

---

## 🔌 Auto-Wiring de Handlers de Eventos (`EventBus.Register`)

Cuando un Handler reacciona a un evento, implementa la interfaz `IEventHandler<TEvent>`.  
El `EventBus` deduce automáticamente el evento a partir de la interfaz (estilo Wolverine, sin reflexión pesada):

```csharp
// Features/Player/UseCases/AddLife/AddLifeHandler.cs
public class AddLifeHandler(PlayerState playerState) : IEventHandler<BonusLifeAwardedEvent>
{
    // Único método: reacciona directamente al evento desacoplado
    public void Handle(BonusLifeAwardedEvent @event)
    {
        var player = playerState.GetPlayer(@event.PlayerId);
        player.Lives += 1;
        EventBus.Publish(new PlayerLivesChangedEvent(@event.PlayerId, player.Lives));
    }
}
```

En la extensión del módulo, el registro es de una sola línea:
```csharp
// Features/Player/PlayerModuleExtensions.cs
public static void AddPlayerModule()
{
    var state = new PlayerState();
    AddLife = new AddLifeHandler(state);

    // Auto-wiring instantáneo:
    EventBus.Register(AddLife);
}
```

---

## 🏃 El Patrón "Humble Object" y `MoveAndSlide()`

`MoveAndSlide()` es el equivalente exacto a **`_dbContext.SaveChangesAsync()`** de Entity Framework:
* El **Handler (`UpdateLocomotionHandler`)** es la *mente* que decide: calcula aceleración, fricción, gravedad y salto, devolviendo la `Velocity` resultante.
* **`MoveAndSlide()`** es el *driver de hardware/física en C++*: aplica la velocidad en la pantalla evitando colisiones geométricas contra paredes y suelo.

El controlador queda convertido en un **Humble Object** puro:

```csharp
// Features/Player/Presentation/PlayerController.cs
public partial class PlayerController : CharacterBody2D
{
    private readonly UpdateLocomotionHandler _locomotion = new();
    private float _gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

    public override void _PhysicsProcess(double delta)
    {
        // 1. Empaquetar entradas en un DTO inmutable
        var input = new LocomotionInput(
            CurrentVelocity: Velocity,
            Direction: Input.GetAxis("move_left", "move_right"),
            JumpRequested: Input.IsActionJustPressed("jump"),
            IsOnFloor: IsOnFloor(),
            DeltaTime: (float)delta,
            Gravity: _gravity
        );

        // 2. El Handler calcula y decide la nueva velocidad
        Velocity = _locomotion.Handle(input, PlayerId: 1);

        // 3. El driver físico de Godot ejecuta los choques en pantalla
        MoveAndSlide();
    }
}
```

---

## 🔒 Regla Estricta: Cohesión de Casos de Uso ("Cero Carpetas Sueltas")

**No existen carpetas `Events/` genéricas a nivel de módulo.**  
Todo Caso de Uso reside en su propia subcarpeta junto con los Eventos que produce:

```text
Features/[ModuleName]/UseCases/[UseCaseName]/
├── [UseCaseName]Handler.cs       <-- Clase Handler con Constructor Primario
├── [InputDto].cs                 <-- DTO inmutable de entrada (si aplica)
└── [ProducedFact]Event.cs        <-- Evento producido por este caso de uso
```

Cada clase o record reside en **su propio archivo con su nombre exacto** (una clase/record por archivo).

---

## 📂 Estructura Universal por Vertical Slice

```text
GameProject/
├── src/
│   ├── Shared/                               <-- Infraestructura técnica transversal
│   │   ├── Messaging/
│   │   │   ├── IEvent.cs                     <-- Contrato de Eventos
│   │   │   ├── IEventHandler.cs              <-- Contrato de Event Handlers
│   │   │   └── EventBus.cs                   <-- Bus estático ligero (Zero-GC, 1 a N)
│   │   └── Bootstrap/
│   │       └── GameBootstrap.cs              <-- Composition Root (Punto de entrada)
│   │
│   ├── Features/                             <-- Módulos Funcionales (Slices)
│   │   │
│   │   ├── [FeatureName]/                    <-- Ej: Economy, Player, Combat, Inventory
│   │   │   ├── [Feature]ModuleExtensions.cs  <-- Add[Feature]Module()
│   │   │   ├── Models/                       <-- Estado LOCAL (1 archivo por clase/enum)
│   │   │   │   ├── [Entity]Data.cs
│   │   │   │   └── [Feature]State.cs
│   │   │   │
│   │   │   ├── UseCases/                     <-- Casos de Uso autocontenidos
│   │   │   │   └── [ActionName]/             <-- 1 carpeta por caso de uso
│   │   │   │       ├── [ActionName]Handler.cs <-- Lógica pura (Constructor Primario)
│   │   │   │       ├── [InputDto].cs         <-- DTO de entrada (opcional)
│   │   │   │       └── [EventName]Event.cs   <-- Evento producido
│   │   │   │
│   │   │   └── Presentation/                 <-- Adaptadores de Godot (Humble Objects)
│   │   │       ├── [Entity]Controller.cs     <-- Nodos físicos (CharacterBody, RigidBody)
│   │   │       └── [Entity]View.cs           <-- Render (Sprites, Shaders, ColorRect)
│   │   │
│   │   ├── UI/                               <-- Slice de Interfaz
│   │   │   └── Presentation/
│   │   │       └── HudController.cs          <-- Escucha eventos del EventBus y actualiza textos
│   │   │
│   │   └── Audio/                            <-- Slice de Sonido
│   │       └── Presentation/
│   │           └── AudioController.cs        <-- Escucha eventos del EventBus y dispara .wav
│   │
│   └── Scenes/                               <-- Escenas (.tscn) y Prefabs de Godot
```

---

## ⚙️ El Núcleo de Difusión: `EventBus.cs`

```csharp
using System;
using System.Collections.Generic;

namespace Game.Shared.Messaging;

public interface IEvent { }

public interface IEventHandler<TEvent> where TEvent : IEvent
{
    void Handle(TEvent @event);
}

public static class EventBus
{
    private static readonly Dictionary<Type, List<Delegate>> _subscribers = new();

    public static void Register<TEvent>(IEventHandler<TEvent> handler) where TEvent : IEvent
    {
        Subscribe<TEvent>(handler.Handle);
    }

    public static void Subscribe<TEvent>(Action<TEvent> callback) where TEvent : IEvent
    {
        var type = typeof(TEvent);
        if (!_subscribers.ContainsKey(type)) _subscribers[type] = new List<Delegate>();
        _subscribers[type].Add(callback);
    }

    public static void Unsubscribe<TEvent>(Action<TEvent> callback) where TEvent : IEvent
    {
        var type = typeof(TEvent);
        if (_subscribers.TryGetValue(type, out var list)) list.Remove(callback);
    }

    public static void Publish<TEvent>(TEvent @event) where TEvent : IEvent
    {
        if (_subscribers.TryGetValue(typeof(TEvent), out var list))
        {
            foreach (var del in list) ((Action<TEvent>)del)(@event);
        }
    }

    public static void Clear()
    {
        _subscribers.Clear();
    }
}
```

---

## 🤖 Directivas de Codificación para Agentes de IA

Cuando un agente de IA genere o refactorice código en este proyecto, debe cumplir estrictamente las siguientes reglas:

1. **EL CASO DE USO ES EL HANDLER:** Nombrar las clases de casos de uso con el sufijo `Handler` (ej: `CollectCoinHandler`, `UpdateLocomotionHandler`). Prohibido crear clases duplicadas `...UseCase`.
2. **CONSTRUCTORES PRIMARIOS (C# 12):** Usar constructores primarios en todas las clases y Handlers que reciban dependencias o estados.
3. **AUTO-WIRING DE EVENTOS:** Los Handlers que escuchen eventos deben implementar `IEventHandler<TEvent>` y registrarse mediante `EventBus.Register(handler)`.
4. **CONTROLADORES COMO "HUMBLE OBJECTS":** Los scripts de nodos en `Presentation/` no deben contener fórmulas de aceleración, inercia o cálculos de negocio. Empaquetan entradas en DTOs, llaman al Handler, y delegan la física al motor (`MoveAndSlide()`).
5. **UNA SOLA CLASE/RECORD POR ARCHIVO:** El nombre del archivo debe ser exactamente el nombre de la clase/record.
6. **COHESIÓN DE CASOS DE USO:** Queda estrictamente prohibido crear carpetas `Events/` sueltas a nivel de módulo. Todo handler y los eventos que produce pertenecen a su subcarpeta `UseCases/[UseCaseName]/`.
7. **IDENTIFICADORES DE ENTIDAD/JUGADOR:** Todos los Handlers y Eventos deben contemplar `int PlayerId = 1` o `string TargetId`.
8. **PROHIBIDO EL ESTADO GLOBAL MONOLÍTICO:** Cada feature posee su propio estado local (`[Feature]State.cs`) indexado por ID en diccionarios.
9. **PROHIBIDO SATURAR EL BUCLE A 60 FPS:** El `EventBus` es exclusivamente para **eventos discretos**.
