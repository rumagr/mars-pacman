using System;
using Mars.Interfaces.Agents;
using Mars.Interfaces.Environments;
using Mars.Interfaces.Annotations;
using Mars.Numerics;
using System.Collections.Generic;
using System.Linq;

namespace Pacman.Model;

public abstract class MovingAgent : IAgent<MazeLayer>, IPositionable
{
    protected MazeLayer Layer;
    
    public Guid ID { get; set; }
    public Position Position { get; set; }
    
    public bool HasMoved { get; set; }

    public abstract void Init(MazeLayer layer);
    
    /// <summary>
    ///     Moves the agent one step along the shortest routable path towards a chosen goal.
    /// </summary>
    protected bool MoveTowardsGoal(Position goal)
    {
        if (HasMoved) return false;
        
        var path = Layer.FindPath(Position, goal).GetEnumerator();
        
        if (path.MoveNext() && path.MoveNext())
        {
            switch (this)
            {
                case GhostAgent:
                    var speed = ((GhostAgent)this).Mode == GhostMode.Eaten ? 2 : 1;
                    Layer.GhostAgentEnvironment.MoveTo((GhostAgent)this, path.Current, speed);
                    break;
                case PacManAgent:
                    var nextPosition = path.Current;
                    if (nextPosition.X > Position.X)
                        ((PacManAgent)this).Direction = Direction.Right;
                    else if (nextPosition.X < Position.X)
                        ((PacManAgent)this).Direction = Direction.Left;
                    else if (nextPosition.Y > Position.Y)
                        ((PacManAgent)this).Direction = Direction.Up;
                    else if (nextPosition.Y < Position.Y)
                        ((PacManAgent)this).Direction = Direction.Down;
                    Layer.PacManAgentEnvironment.MoveTo((PacManAgent)this, path.Current, 1);
                    break;
                default:
                    Console.WriteLine("Unknown agent type.");
                    break;
            }
            HasMoved = true;
        }
        return HasMoved;
    }
    
    /// <summary>
    /// Gets the distance between the Agent and the target position.
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    protected double GetDistance(Position target)
    {
        return Distance.Euclidean(Position.X, Position.Y, target.X, target.Y);
    }
    
    /// <summary>
    /// Explores the environment and returns a list of positions of the pellets.
    /// </summary>
    /// <returns></returns>
    protected List<Position> ExplorePelletPositions()
    {
        return Layer.PelletEnvironment.Explore(Position, VisualRange, -1).Select(agent => agent.Position).ToList();
    }
    
    /// <summary>
    /// Explores the environment and returns a list of positions of the power pellets.
    /// </summary>
    /// <returns></returns>
    protected List<Position> ExplorePowerPelletPositions()
    {
        return Layer.PowerPelletEnvironment.Explore(Position, VisualRange, -1).Select(agent => agent.Position).ToList();
    }

    /// <summary>
    /// Explores the environment and returns a list of positions of the occupiable spots.
    /// </summary>
    protected List<Position> ExploreOccupiablePositions()
    {
        return Layer.OccupiableSpotsEnvironment.Explore(Position, VisualRange, -1).Select(agent => agent.Position).ToList();
    }
    
    public abstract void Tick();
    
    [PropertyDescription]
    public int StartX { get; set; }
    
    [PropertyDescription]
    public int StartY { get; set; }
    
    [PropertyDescription]
    public String Name { get; set; }
    
    [PropertyDescription]
    public int VisualRange { get; set; }
}