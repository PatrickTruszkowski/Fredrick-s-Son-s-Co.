# Fredrick's & Son's Co.

**A first-person multiplayer comedy-horror game built with Unity and C#.**  
**Status:** In development

Fredrick's & Son's Co. is a personal game-development project focused on building gameplay systems from the ground up. The current code explores procedurally generated environments, multiplayer interactions, enemy behaviors, and a movement system with an emphasis on player feel.

## Technical highlights

- **Procedural environment generation:** A tile-based basement generator creates a randomized 10 × 10 layout with configurable wall/intersection pieces, boundary handling, and decorations. The server provides a seed through a `NetworkVariable` so each client can generate the layout from the same starting state.
- **Multiplayer systems:** Unity Netcode for GameObjects supports player ownership, networked object spawning and removal, and RPC-based gameplay interactions, including items, switches, and security cameras.
- **Custom first-person movement:** A `CharacterController`-based movement system includes sprinting, crouching, sliding, stamina, exhaustion, slope handling, and movement responsiveness that changes with carried item mass.
- **Item and inventory framework:** Interactable products support pickup, carrying, storage, dropping, and throwing. Interfaces (`IInteractable`, `IUseable`, and `IAlternateInteractable`) define different interaction types, while `ScriptableObject` assets store item-specific properties.
- **Enemy behavior prototype:** A NavMesh-based NPC framework implements an evolving state machine with idle, roaming, intimidation, and chase states, using visibility and distance checks to react to players. Some combat behavior is still unfinished.
- **Additional gameplay systems:** Battery-powered equipment, a building power system, surveillance camera switching, breakable objects, audio effects, and UI feedback.

## Code worth exploring

| System | Source file |
| --- | --- |
| Procedural level generation | [`BasementGenerator.cs`](Assets/Scripts/Singletons/BasementGenerator.cs) |
| Multiplayer management | [`MultiplayerManager.cs`](Assets/Scripts/Components/MultiplayerManager.cs) |
| Player movement | [`Player.cs`](Assets/Scripts/Components/Player/Player.cs) |
| Networked item interactions | [`Interaction.cs`](Assets/Scripts/Components/Interaction.cs) |
| Item behavior and throwing physics | [`Product.cs`](Assets/Scripts/Components/Product/Product.cs) |
| Inventory management | [`Inventory.cs`](Assets/Scripts/Components/Inventory.cs) |
| Enemy AI state machine | [`BurntrapAI.cs`](Assets/Scripts/Bots/Burntrap/BurntrapAI.cs) |
| Input handling | [`InputManager.cs`](Assets/Scripts/Singletons/InputManager.cs) |

## Technologies and concepts

**C# · Unity · Unity Netcode for GameObjects · Unity Input System · Unity NavMesh · Object-oriented programming · State machines · Procedural generation · Vector math · Client-server networking**

## My contribution

I independently designed and implemented the project's gameplay systems and wrote the game scripts, using external resources and AI-assisted discussion for occasional debugging or feedback rather than having AI generate the game implementation. The included `ClientNetworkTransform.cs` is based on the Unity multiplayer sample utility and is separate from the gameplay systems described above.

## Development status and repository scope

This is an **unfinished project**, shared to demonstrate programming and system design rather than a finished game. Some systems are prototypes, and parts of the enemy combat and multiplayer logic remain under development.

**This repository contains C# scripts and Unity metadata only.** It does not include the Unity scenes, prefabs, models, project configuration, or a playable build, so it is not a standalone Unity project. Additional project dependencies and generated input classes are not included here.

<!-- Optional later: Add 1–2 real screenshots or a short gameplay clip here. -->
