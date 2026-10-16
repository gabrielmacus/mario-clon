# Instrucciones para agentes de IA

Proyecto Godot 4.7 (.NET) + C# 12 (`net8.0`), namespace raíz `MarioClone`.
La explicación completa está en [`README.md`](README.md). Este archivo es la versión corta y **obligatoria**.

Respondé en **español**.

---

## 1. Forma de trabajar

1. **Ir por partes.** Hacé solo lo pedido. Si ves algo más para mejorar, proponelo y esperá aprobación antes de hacerlo.
2. **Solución mínima.** Nada de sobreingeniería: código que se entienda de una lectura.
3. **Nada de git sin permiso.** No hagas `commit` ni comandos destructivos (`checkout --`, `restore`, `reset`) sin autorización explícita.
4. **Verificá las APIs de Godot vos mismo** antes de entregar, contra el assembly real: `.godot/mono/temp/bin/Debug/GodotSharp.dll`. No delegues esa verificación al compilador ni al usuario.
5. **Si no pudiste compilar ni probar, decilo.** El usuario prueba con F5 en Godot.
6. **Al renombrar o mover un `.cs`, mové también su `.cs.uid`.**
7. **Si cambia una regla o la estructura, actualizá `README.md` y este archivo.**

---

## 2. Reglas de código

| # | Regla | ✅ Sí | ❌ No |
|---|---|---|---|
| 1 | El caso de uso **es** el Handler | `CollectCoinHandler` | `CollectCoinUseCase` + Handler |
| 2 | Constructor primario en toda clase con dependencias | `class X(PlayerRepository playerRepository)` | campos + constructor clásico |
| 3 | Los nodos de `Presentation/` no calculan ni deciden | arman un `Input`, llaman al Handler, aplican el resultado | fórmulas de física o reglas en el nodo |
| 4 | Una clase/record por archivo, con su nombre exacto | `TouchInput.cs` → `record TouchInput` | dos records en un archivo |
| 5 | Eventos junto al Handler que los produce | `UseCases/TouchEnemy/EnemyStompedEvent.cs` | `Enemies/Events/` para un evento de un solo caso de uso |
| 6 | Excepción: evento producido por **varios** casos de uso de la misma feature | `Player/Events/PlayerLivesChangedEvent.cs` | — |
| 7 | Datos en `[Entity]Data`, colección en `[Entity]Repository` (nombre de la **entidad**) | `WalletRepository` guarda `WalletData` | `EconomyRepository` |
| 8 | Handlers que escuchan eventos implementan `IEventHandler<T>` y se registran con `EventBus.Register` | `EventBus.Register(AddLife)` | suscripciones sueltas desde Handlers |
| 9 | `EventBus` solo para hechos puntuales | `PlayerJumpedEvent` | un evento por frame |
| 10 | Nodo con datos en un repositorio: alta en `_Ready`, baja en `_ExitTree` | `SpawnEnemy` / `DespawnEnemy` | baja solo cuando "muere" |
| 11 | Nodo que hace `Subscribe` en `_Ready` hace `Unsubscribe` en `_ExitTree` | ver `HudController` | suscribirse sin desuscribirse |
| 12 | Siempre `using` + nombre corto | `using Godot;` … `Vector2` | `Godot.Vector2`, `MarioClone.Features.X.Y` en el código |
| 13 | Eventos y Handlers sobre una entidad llevan su ID | `int PlayerId = 1`, `ulong EnemyId` | sin ID |
| 14 | Namespace = carpeta | `src/Features/World/UseCases/LoadLevel` → `MarioClone.Features.World.UseCases.LoadLevel` | — |
| 15 | Todos los enemigos dentro de la feature `Enemies` | `Enemies/Presentation/KoopaController.cs` | feature `Koopa/` |
| 16 | La View solo traduce: el **estado** (Idle, Running...) lo decide el Handler y la View lo mapea a una animación en cada frame | `_view.Render(result.State, result.Facing)` con un `switch` estado → animación | `if`s en la View que deciden si corre o salta; publicar `PlayerRunningEvent` cada frame |
| 17 | Animaciones de **momento** (golpe, pisotón) se disparan escuchando eventos, filtrando por ID | `EnemyView` escucha `EnemyPunchedEvent` | el Handler llamando a una animación |
| 18 | Los Handlers publican hechos de negocio, nunca piden efectos visuales | `EnemyPunchedEvent(EnemyId, BodyPart, ...)` | `ShowBloodEvent` |
| 19 | Criterio: **¿dura o pasa una vez?** Lo que dura es estado (Handler → `Result` → `View.Render`); lo que pasa una vez es evento | Goomba aplastado = `EnemyStatus.Squashed` | animar el aplastado escuchando un evento |
| 20 | La lógica se suscribe **solo** en `[Feature]ModuleExtensions` con `EventBus.Register`. Los controllers nunca se suscriben; solo Views y HUD, y solo para efectos de un instante | `EventBus.Register(Bounce)` | `PlayerController` suscripto a `EnemyStompedEvent` |
| 21 | Banderas en el repositorio solo para lógica que consume otro Handler; nunca para efectos visuales o de sonido | `PlayerData.BounceRequested` | `PlayerData.BlinkRequested` |
| 22 | Valores iniciales con Handlers de **consulta** (`Get[Algo]Handler`): solo leen, no modifican ni publican, devuelven un valor o un `Result` (nunca la entidad) | `HudController._Ready` → `GetLives.Handle(PlayerId)` | texto fijo en el `.tscn` que repite un valor del dominio |
| 23 | El flujo de la aplicación (reiniciar, cambiar de nivel, pausar) lo maneja `GameBootstrap`, escuchando eventos | `GameOverEvent` → `GameBootstrap` recarga la escena | el HUD o un controller recargando la escena |
| 24 | Los Handlers **nunca** usan otros Handlers (ni por constructor ni por `ModuleExtensions`): se comunican solo por eventos | `TakeDamage` publica `PlayerDiedEvent` → `Die` lo escucha | `UpdateLocomotionHandler(PlayerRepository, DieHandler die)` |

---

## 3. Mapa rápido

| Necesito... | Archivo |
|---|---|
| Registrar módulos | `src/Shared/Bootstrap/GameBootstrap.cs` |
| Bus de eventos | `src/Shared/Messaging/EventBus.cs` |
| Movimiento del jugador | `src/Features/Player/UseCases/Locomotion/UpdateLocomotionHandler.cs` |
| El nivel (texto) | `levels/1-1.txt` (leyenda en README, sección 11b) |
| Leer el nivel | `src/Features/World/UseCases/LoadLevel/LoadLevelHandler.cs` |
| Armar el nivel en Godot (tiles, entidades, bandera, pozos) | `src/Features/World/Presentation/LevelBuilder.cs` |
| Morir, reintentar, game over | `src/Features/Player/UseCases/Die/DieHandler.cs` + `GameBootstrap` |
| Golpear bloques (reglas) | `src/Features/World/UseCases/HitBlock/HitBlockHandler.cs` |
| Rebote, bloque usado, moneda e ítem que salen | `LevelBuilder.OnBlockBumped` / `OnBlockEmptied` |
| Ítems (movimiento y recolección) | `src/Features/Collectibles/` |
| Pisotón vs. golpe | `src/Features/Enemies/UseCases/TouchEnemy/TouchEnemyHandler.cs` |
| HUD | `src/Features/UI/Presentation/HudController.cs` |
| Escena principal | `scenes/Main.tscn` |
| Teclas (input map) | `project.godot`, sección `[input]` |

**Capas de colisión:** 1 = mundo (bit 1), 2 = jugador (bit 2), 3 = enemigos (bit 4), 4 = ítems (bit 8). Las Area2D detectoras usan `collision_layer = 0` y `collision_mask = 2`.

**Medidas útiles:** viewport 640×360; tiles de 16×16; nivel 1-1 de 200×22 celdas; superficie del piso en y = 320 (fila 20); salto del jugador ≈ 78 px (≈ 4 tiles) de alto.

**Repositorios por partida:** `PlayerRepository` y `WalletRepository` sobreviven a la recarga de escena (vidas, monedas); se descartan con `ResetSession()` en game over o nivel completado. No los recrees en cada `Add[Feature]Module()`.

---

## 4. Escenas `.tscn`

- Preferí que el usuario arme lo visual en el editor. Si tenés que escribir un `.tscn`: `[gd_scene format=3]`, recursos con rutas `res://...` y sin inventar `uid`.
- Para exportar un nodo (`[Export] public Node2D Target`), en el `.tscn` va `node_paths=PackedStringArray("Target")` en el encabezado del nodo y `Target = NodePath("../Player")`.

---

## 5. Lista de verificación antes de entregar

- [ ] Hice solo lo pedido (lo demás quedó como propuesta).
- [ ] Cada clase/record en su archivo, con namespace = carpeta.
- [ ] Sin nombres completos en el código; todo con `using`.
- [ ] Ninguna lógica nueva en nodos de `Presentation/` (las Views solo traducen estados y reaccionan a eventos).
- [ ] Handlers nuevos creados en su `ModuleExtensions` (y registrados si escuchan eventos).
- [ ] Toda suscripción o alta tiene su baja en `_ExitTree`.
- [ ] APIs de Godot verificadas contra `GodotSharp.dll`.
- [ ] `.uid` movidos junto con sus `.cs`.
- [ ] `README.md` y `AGENTS.md` actualizados si cambió alguna regla o estructura.
- [ ] Aviso explícito si no compilé ni probé.
