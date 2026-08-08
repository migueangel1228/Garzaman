# Project Overview
- Game Title: Puchi's Escape: The Goose Sanctuary
- High-Level Concept: A first-person horror MVP where Puchi, trapped in a tribal-futuristic sanctuary, must collect 10 feathers to escape while evading a mutated monster goose.
- Players: Single player
- Inspiration / Reference Games: Amnesia, Alien: Isolation (simplified), Slender.
- Tone / Art Direction: Tribal-futuristic / Horror Jungle.
- Target Platform: Standalone Windows.
- Screen Orientation / Resolution: Landscape 1920x1080.
- Render Pipeline: URP (Universal Render Pipeline).

# Game Mechanics
## Core Gameplay Loop
1. Explore the sanctuary to find 10 sacred feathers.
2. Manage noise and visibility (Walk/Run/Crouch) to avoid the monster goose.
3. Use safe rooms when chased or to hide.
4. Interact with the exit once all feathers are collected.
5. Game over if caught by the goose.

## Controls and Input Methods
- Movement: WASD
- Run: Left Shift (increases noise and visibility)
- Crouch: Left Ctrl (decreases noise and visibility, lower height)
- Jump: Space
- Interact: E (pick up feathers, open doors)
- Camera: Mouse Look

# UI
- HUD: 
    - Feather Counter (e.g., "Feathers: 0 / 10")
    - Interaction Prompt (e.g., "[E] Pick up Feather")
- Screens:
    - Main Menu (Existing)
    - Game Over Screen (with "Retry" and "Quit")
    - Escape/Win Screen

# Key Asset & Context
- Scripts:
    - `GooseAI.cs`: State machine for Patrol, Investigate, Chase, and Return.
    - `PlayerStats.cs`: Tracks feather count and game state (Safe/Detected).
    - `Interactable.cs`: Base class for feathers and doors.
    - `SafeZone.cs`: Trigger to hide player from AI.
- Scene: `Garzaman2077.unity` (Update with NavMesh and interactable placements).

# Implementation Steps
1. **Player Enhancements**:
    - Add Crouch and Interact actions to `StarterAssets` Input Action Asset.
    - Update `StarterAssetsInputs.cs` to handle these actions.
    - Update `FirstPersonController.cs` to implement Crouch logic (CharacterController height/center change) and interaction raycasting.
2. **Game Logic & HUD**:
    - Implement `PlayerStats.cs` to manage the feather count.
    - Create a basic HUD using UI Toolkit or Canvas to display the feather count.
3. **Feather Collection**:
    - Create a `Feather` prefab with an `Interactable` script.
    - Implement the interaction logic to increment feather count and destroy the feather.
4. **Goose AI (State Machine)**:
    - Implement `GooseAI.cs` using `NavMeshAgent`.
    - States: 
        - **Patrol**: Move between waypoints.
        - **Investigate**: Move to a noise location or last seen player position.
        - **Chase**: Move directly to player position.
        - **Return**: Head back to patrol route if player is lost.
    - Add vision (cone) and hearing (radius based on player movement speed).
5. **Safe Rooms & Contact**:
    - Implement `SafeZone.cs` to mark player as "Hidden".
    - Implement contact logic on the Goose (OnTriggerEnter with Player) to trigger Game Over.
6. **Exit & Game Flow**:
    - Implement `ExitDoor.cs` that checks `PlayerStats.FeatherCount >= 10`.
    - Connect UI screens (Game Over, Win).

# Verification & Testing
- Verify Crouch reduces player height and speed.
- Verify Interaction raycast correctly detects Feathers.
- Verify Goose AI detects player at different speeds (Run vs Walk vs Crouch).
- Verify Goose loses track of player in Safe Zones.
- Verify game ends correctly on capture or escape.
