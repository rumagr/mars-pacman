using System;
using System.Collections.Generic;
using System.Linq;
using Mars.Interfaces.Annotations;
using Mars.Interfaces.Environments;

namespace Pacman.Model;

public class GhostAgent : MovingAgent
{


    /// <summary>
    ///     The initialization method of the GhostAgent is executed once at the beginning of a simulation.
    /// </summary>
    /// <param name="layer">The MazeLayer that manages the agents</param>
    public override void Init(MazeLayer layer)
    {
        Layer = layer;
        Position = new Position(StartX, StartY);
        Layer.GhostAgentEnvironment.Insert(this);
        Mode = GhostMode.Scatter;
        ReleaseTimer = 0;
    }

    /// <summary>
    ///     The tick method of the GhostAgent is executed during each time step of the simulation.
    /// </summary>
    public override void Tick()
    {
        if (ReleaseTimer <= ReleaseTick)
        {
            ReleaseTimer++;
            return;
        }
        
        if (Mode != GhostMode.Eaten && Mode != GhostMode.Frightened)
        {
            Mode = Layer.GetCurrentTick() % 54 < 14 ? GhostMode.Scatter : GhostMode.Chase;
        }
        
        Position target;

        switch (Mode)
        {
            case GhostMode.Chase:
                target = GetChaseTarget();
                break;

            case GhostMode.Scatter:
                target = new Position(ScatterCellX, ScatterCellY);
                break;

            case GhostMode.Frightened:
                if (Layer.GetCurrentTick() % 2 == 0) return;
                target = GetRandomCell();
                break;

            case GhostMode.Eaten:
                target = new Position(HouseCellX, HouseCellY); 
                break;

            default:
                return;
        }
        MoveTowardsGoal(target);
    }

    /// <summary>
    /// Selects a random, walkable cell from the ghost's surroundings.
    /// </summary>
    /// <returns></returns>
    protected Position GetRandomCell()
    {
        var randomIndex = new Random().Next(Layer.OccupiableSpots.Count);
        return Layer.OccupiableSpots[randomIndex];
    }
    
    private Position GetPinkyTarget()
    {
        var pacman = Layer.PacManAgent;
        var target = pacman.Position;
        if (pacman.Direction == Direction.Right)
            target = new Position(target.X + 1, target.Y);
        else if (pacman.Direction == Direction.Left)
            target = new Position(target.X - 1, target.Y);
        else if (pacman.Direction == Direction.Up)
            target = new Position(target.X, target.Y + 1);
        else if (pacman.Direction == Direction.Down)
            target = new Position(target.X, target.Y - 1);
        return GetClosestOccupiablePosition(target);
    }
    
    private Position GetInkyTarget()
    {
        var pacman = Layer.PacManAgent;
        var current = pacman.Position;

        var neighbors = new List<Position>
        {
            Position.CreatePosition(current.X + 1, current.Y),     
            Position.CreatePosition(current.X - 1, current.Y),     
            Position.CreatePosition(current.X, current.Y + 1),     
            Position.CreatePosition(current.X, current.Y - 1),     
            Position.CreatePosition(current.X + 1, current.Y + 1), 
            Position.CreatePosition(current.X - 1, current.Y + 1), 
            Position.CreatePosition(current.X + 1, current.Y - 1), 
            Position.CreatePosition(current.X - 1, current.Y - 1), 
        };
        var validTargets = neighbors
            .Where(pos => Layer.IsRoutable(pos))
            .ToList();

        if (validTargets.Count == 0)
            return current;

        var rnd = new Random();
        return validTargets[rnd.Next(validTargets.Count)];
    }


    
    private Position GetClydeTarget()
    {
        var pacman = Layer.PacManAgent;
        
        var target = Math.Abs(pacman.Position.X - Position.X) + Math.Abs(pacman.Position.Y - Position.Y) < 4 ?
            new Position(ScatterCellX, ScatterCellY) :
            pacman.Position;
        return target;
    }

    private Position FindPacman() =>
        Layer.PacManAgentEnvironment.Explore(Position, -1, 1).First().Position;

    protected bool IsPacmanPoweredUp() =>
        Layer.PacManAgent.PoweredUp;

    private Position GetChaseTarget()
    {
        return Name switch
        {
            "Blinky" => FindPacman(),
            "Pinky" => GetPinkyTarget(),
            "Inky" => GetInkyTarget(),
            "Clyde" => GetClydeTarget(),
            _ => FindPacman()
        };
    }
    
    protected Position GetClosestOccupiablePosition(Position target)
    {
        return Layer.OccupiableSpots
            .OrderBy(spot => Math.Abs(spot.X - target.X) + Math.Abs(spot.Y - target.Y))
            .FirstOrDefault();
    }
    
    
    
    public GhostMode Mode { get; set; }
    
    public int ReleaseTimer { get; set; }
    
    
    [PropertyDescription]
    public int ReleaseTick { get; set; }
    
    [PropertyDescription]
    public int ScatterCellX { get; set; }
    
    [PropertyDescription]
    public int ScatterCellY { get; set; }
    
    [PropertyDescription]
    public int HouseCellX { get; set; }
    
    [PropertyDescription]
    public int HouseCellY { get; set; }
}