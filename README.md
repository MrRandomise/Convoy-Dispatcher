# Convoy Dispatcher

> A Unity 6 simulation game about managing convoys, planning routes, and coordinating cargo deliveries across procedurally generated maps.

---

## Overview

**Convoy Dispatcher** is a real-time convoy management and simulation game developed in **Unity 6 (LTS)** with **C#**. Players act as a dispatcher: they plan routes, configure convoys of trucks, assign escort vehicles, set up dynamic event triggers, and monitor delivery missions as they unfold on a procedurally generated map.

The project demonstrates a range of game-systems engineering skills — from clean architecture patterns (Service Locator, Event Bus, State Machine) to procedural content generation and modular UI design.

---

## Key Features

| Feature | Description |
|---|---|
| **Procedural Map Generation** | Seed-based map generator creates unique road networks, spawn points, and delivery destinations for every session |
| **Convoy Simulation** | Multi-truck convoys follow waypoint routes in formation; simulation supports play / pause / resume |
| **Escort System** | Dedicated escort vehicles protect the convoy; escort-attack events fire dynamically during simulation |
| **Trigger System** | Visual trigger builder allows designers to wire custom gameplay events to route nodes without code |
| **Route System** | Routes are composed of typed nodes (Waypoint, DeliveryPoint, SpawnPoint) and validated at runtime |
| **Difficulty Levels** | Multiple difficulty presets (Training → Combat) that scale time limits, fuel budgets, and events |
| **Planning UI** | Pre-mission screen for route editing and convoy configuration |
| **Save / Load** | JSON-based persistent save system preserves player progress across sessions |
| **Localization** | Language-switching service with runtime locale support |
| **Input System** | Built on the new Unity Input System with full keyboard support; mobile-ready architecture |

---

## Architecture

The codebase follows clean architecture principles with clear separation of concerns:

```
Assets/GameCore/
├── Bootstrap/         # App entry point — registers all services via ServiceLocator
├── Services/          # Core services: SaveSystem, LocalizationSystem, ServiceLocator
│   ├── Save/
│   ├── Localization/
│   └── Tutorial/
├── Events/            # Event Bus (IEventBus) + typed game events (GameEvents, ConvoySimulationEvents)
├── SimulationSystem/  # Simulation state machine (Idle → Running ↔ Paused) + update loop
│   └── State/
├── Level/             # Procedural level data, generator, difficulty configs
│   ├── Generator/
│   └── Difficulty/
├── Convoy/            # Convoy domain: Truck, Escort, ConvoyVisualizer, Factory, Upgrades, Rules
│   ├── Truck/
│   ├── Escort/
│   ├── Factory/
│   ├── Upgrades/
│   └── Rules/
├── Routes/            # Route, RouteNode, RouteNodeType, RouteSegment, RouteSystem
├── Triggers/          # Trigger system for dynamic in-mission events
├── Input/             # Input abstraction (IInputSystem / MobileInputSystem)
├── UI/                # SimulationUIController, TriggerBuilderDialog, RouteNodeUI, Planning/
├── Config/            # ScriptableObject configs (GameConfig, MapGeneratorConfig)
├── GameManager.cs     # Scene-level orchestrator
└── MapCameraController.cs  # Smooth follow / pan camera for the map view
```

### Design Patterns Used

- **Service Locator** — all major systems (simulation, routes, save, localization, input, triggers) are registered and resolved through a central `ServiceLocator`, making them mockable and replaceable.
- **Event Bus** — a typed `IEventBus` decouples publishers from subscribers (e.g. `ConvoySimulationEvents`, `EscortAttackEvent`, `RouteNodeSelectedEvent`).
- **State Machine** — `SimulationSystem` implements an explicit `SimulationState` enum (`Idle / Running / Paused`) with guarded transitions.
- **Factory** — `IConvoyFactory` abstracts convoy instantiation and keeps `GameManager` free of construction logic.
- **Interface-First** — all major systems expose interfaces (`ISimulationSystem`, `IRouteSystem`, `IConvoy`, `IEventBus`, `ISaveSystem`, etc.), enabling substitution and testing.

---

## Tech Stack

| Category | Technology |
|---|---|
| **Engine** | Unity 6000.3.5f1 (Unity 6 LTS) |
| **Language** | C# |
| **Render Pipeline** | Universal Render Pipeline (URP 17.3.0) |
| **Input** | Unity Input System 1.17.0 |
| **Navigation** | Unity AI Navigation (NavMesh) 2.0.9 |
| **Animation / Sequencing** | Unity Timeline 1.8.10 |
| **UI** | Unity UI (uGUI) 2.0.0 |
| **Testing** | Unity Test Framework 1.6.0 |
| **Visual Scripting** | Unity Visual Scripting 1.9.9 |
| **IDE** | JetBrains Rider / Visual Studio |

---

## Getting Started

### Prerequisites

- **Unity 6000.3.5f1** — download via [Unity Hub](https://unity.com/download)
- Git (to clone the repository)

### Setup

```bash
# 1. Clone the repository
git clone https://github.com/MrRandomise/Convoy-Dispatcher.git

# 2. Open Unity Hub → Add project → select the cloned folder

# 3. Wait for Unity to import all packages (first launch may take a few minutes)
```

### Running in the Editor

1. Open the project in Unity 6.
2. In the **Project** window, navigate to `Assets/Scenes/` and open the main scene.
3. Press **Play** — the Bootstrap scene initialises all services automatically.
4. Controls:
   - `Space` — start / pause / resume simulation
   - `R` — regenerate the map with a new random seed

### Build

Use **File → Build Settings**, select your target platform (PC, Android, etc.), and click **Build**.

---

## Project Status

| Area | Status |
|---|---|
| Core simulation loop | ✅ Complete |
| Procedural map generation | ✅ Complete |
| Convoy movement & visualisation | ✅ Complete |
| Escort & attack events | ✅ Complete |
| Trigger builder UI | ✅ Complete |
| Route planning UI | ✅ In progress |
| Save / Load | ✅ Complete |
| Localization | ✅ Structure complete |
| Tutorial system | 🔄 Planned |
| Mobile touch controls | 🔄 Planned |
| Sound & music | 🔄 Planned |

---

## Roadmap

- [ ] Complete planning-phase UI (route assignment drag-and-drop)
- [ ] Add procedural event variety (ambushes, roadblocks, fuel shortages)
- [ ] Mobile touch-input polish
- [ ] Full localization pass (English + Russian)
- [ ] Tutorial for new players
- [ ] Unit tests for SimulationSystem and RouteSystem

---

## About This Project

This project was built as a personal portfolio piece to demonstrate skills relevant to a **Unity / C# Game Developer** role:

- Designing scalable game architecture with clean code practices
- Implementing real-time simulation systems
- Working with procedural generation algorithms
- Building editor-friendly tooling (ScriptableObject configs, trigger builder)
- Applying SOLID principles and common design patterns in a Unity context

**Developer:** [MrRandomise](https://github.com/MrRandomise)  
**Engine:** Unity 6 LTS  
**Language:** C#  

---

## License

This repository is a portfolio project. All rights reserved © MrRandomise.
