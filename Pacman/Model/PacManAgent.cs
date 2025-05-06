using System;
using System.Collections.Generic;
using System.Linq;
using Mars.Interfaces.Annotations;
using Mars.Interfaces.Environments;

namespace Pacman.Model;

public class PacManAgent : MovingAgent
{
    public override void Init(MazeLayer layer)
    {
        Layer = layer;
        Position = new Position(StartX, StartY);
        Layer.PacManAgentEnvironment.Insert(this);
    }

    public override void Tick()
    {
        var powerPelletPositions = ExplorePowerPelletPositions();
        var pelletPositions = ExplorePelletPositions();
        // var ghostPositions = ExploreGhostPositions();
        var ghostPositions = ExploreGhosts().Select(agent => agent.Position).ToList();
        var occupiablePositions = ExploreOccupiablePositions();
        if (powerPelletPositions.Count > 0) MoveTowardsGoal(powerPelletPositions.First());
        else if (pelletPositions.Count > 0) MoveTowardsGoal(pelletPositions.First());
        else if (ghostPositions.Count > 0)
        {
            var target = ghostPositions.First();
            if (PoweredUp) MoveTowardsGoal(target);
            else MoveTowardsGoal(occupiablePositions.FirstOrDefault(pos => !ghostPositions.Contains(pos)));
        }
        else
        {
            var randomPosition = occupiablePositions[_random.Next(0, occupiablePositions.Count)];
            MoveTowardsGoal(randomPosition);
        }
    }

    /// <summary>
    /// Explores the environment and returns a list of positions of the ghosts.
    /// </summary>
    /// <returns></returns>
    private List<Position> ExploreGhostPositions()
    {
        return Layer.GhostAgentEnvironment.Explore(Position, VisualRange, -1).Select(agent => agent.Position).ToList();
    }

    /// <summary>
    /// Explores the environment and returns a list of GhostAgent instances.
    /// </summary>
    private List<GhostAgent> ExploreGhosts()
    {
        return Layer.GhostAgentEnvironment.Explore(Position, VisualRange, -1).ToList();
    }
    
    private int GetScore() => 
        Layer.Score;
    
    private readonly Random _random = new();
    
    public Direction Direction { get; set; }
    
    public bool PoweredUp { get; set; }
    
    public int PoweredUpTime { get; set; }
    
    [PropertyDescription]
    public int Lives { get; set; }
}