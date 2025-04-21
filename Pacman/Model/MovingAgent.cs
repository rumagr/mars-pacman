using System;
using Mars.Components.Environments;
using Mars.Interfaces.Agents;
using Mars.Interfaces.Environments;
using Mars.Interfaces.Layers;
using Mars.Interfaces.Annotations;

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
    
    public abstract void Tick();
    
    [PropertyDescription]
    public int StartX { get; set; }
    
    [PropertyDescription]
    public int StartY { get; set; }
    
    [PropertyDescription]
    public String Name { get; set; }
}