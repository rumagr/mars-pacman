# Pac-Man Simulation with MARS

This project implements a grid-based simulation of the classic **Pac-Man** game using the **MARS framework**. Agents such as Pac-Man and the ghosts operate within a grid environment and interact based on simple AI rules. The model is fully visualizable through a separate Python visualization component, allowing real-time monitoring of agent behavior.

## Project Structure

Here's an overview of the structure of the project:

- `Program.cs`: The main entry point that starts the simulation.
- `config.json`: Configures simulation parameters like runtime, visualization, and agent setup. See [Configuration](#configuration).
- `Model/`: Contains all the core logic including agent behavior, game rules, environment setup, and utility classes.
- `Resources/`: Contains initialization files for the grid and agents.
- `Visualization/main.py`: A Python-based visualization tool that connects via WebSocket to render the simulation in real-time.

## Model Description

This simulation includes the following main agent and layer types:

### Agents

- **PacManAgent**: The main agent controlled by logic defined in its `Tick()` method. It can collect pellets, power pellets, and eat ghosts if powered up. The behavior is customizable by adjusting the logic inside `Tick()`.
- **GhostAgent**: Enemies that alternate between chasing and scattering, with specific AI behaviors based on their type (Blinky, Pinky, Inky, Clyde). Ghosts can be eaten only if Pac-Man is powered up.
- **Pellet & PowerPellet**: Consumable items that increase the score. Power pellets grant Pac-Man the temporary ability to eat ghosts.

### Layers

- **MazeLayer**: The main layer where all agents interact. It manages the environment grid, agent spawning, and the game loop, including score, collisions, and lives.
- **Environments**: Spatial hash environments that manage agent placement and exploration for Pac-Man and ghosts.

## Pac-Man Behavior Customization

Pac-Man's behavior is defined in the `Tick()` method of `PacManAgent.cs`. This method can be customized to test different strategies. You can control Pac-Man’s decisions using the following helper methods:

- `ExploreGhosts()` – Returns positions of nearby ghosts.
- `ExplorePellets()` – Returns positions of nearby pellets.
- `ExplorePowerPellets()` – Returns positions of nearby power pellets.
- `ExploreOccupiablePositions()` – Returns all valid positions Pac-Man can move to.

To move Pac-Man, use:

- `MoveTowardsGoal(Position target)` – Moves Pac-Man **one field** toward the target. **Diagonal movement is supported**.

An example logic for prioritizing power pellets, pellets, or running from ghosts is already implemented in the code.

## Game Logic

- Pac-Man gains:
  - **10 points** for a pellet,
  - **50 points** for a power pellet,
  - **200 points** for eating a ghost (only when powered up).
- Pac-Man has **3 lives**. A life is lost when a ghost catches him while not powered up.
- If Pac-Man loses a life:
  - He and all ghosts are reset to their **starting positions**.
- If all lives are lost, the simulation ends.
- The visibility of Pac-Man is defined by a **visual range** property.

To interact agents must be on the same position after their Tick() action.
This means that Pac-Man and a ghost may pass each other without interacting if they cross paths but do not end up on the same cell. Interactions only occur if they share the same grid cell at the end of the tick.

## Configuration

The simulation is controlled via the `config.json` file:

- `step`: Time steps of the simulation.
- `Visualization`: Set to `true` to enable the Python visualization client.
- `VisualizationTimeout`: Delay between ticks when visualization is active. A **lower number speeds up** the simulation visually.
- `agents` / `layers`: Defines which agents and layers are active and how many agents are spawned.

For more details on configuration, refer to the [MARS documentation](https://mars.haw-hamburg.de/articles/core/model-configuration/index.html).

## Simulation & Visualization Setup

### Prerequisites

- [.NET Core 8.0 or later](https://dotnet.microsoft.com/en-us/download)
- [Python 3.8+](https://www.python.org/)
- A C# IDE (e.g. [JetBrains Rider](https://www.jetbrains.com/rider/) recommended)

### Run Instructions

1. Clone or download the repository.
2. Open the solution file (`.sln`) in your IDE.
3. Make sure `Visualization` is set to `true` in `config.json`.
4. Start the simulation (`Program.cs`). It will log:  
   `Waiting for live visualization to run.`
5. In a terminal, navigate to the `Visualization/` folder and run:

   ```bash
   pip install -r requirements.txt
   python main.py
Note for macOS users: You may need to use pip3 and python3 instead:
   ```bash
   pip3 install -r requirements.txt
   python3 main.py
