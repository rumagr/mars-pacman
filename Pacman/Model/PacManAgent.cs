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
        var powerPellets = ExplorePowerPellets();
        var pellets = ExplorePellets();
        var ghosts = ExploreGhosts();
        var occupiablePositions = ExploreOccupiablePositions();

        if (powerPellets.Count > 0) MoveTowardsGoal(powerPellets.First());
        else if (pellets.Count > 0) MoveTowardsGoal(pellets.First());
        else if (ghosts.Count > 0)
        {
            var target = ghosts.First();
            if (PoweredUp) MoveTowardsGoal(target);
            else MoveTowardsGoal(occupiablePositions.FirstOrDefault(pos => !ghosts.Contains(pos)));
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
    private List<Position> ExploreGhosts()
    {
        return Layer.GhostAgentEnvironment.Explore(Position, VisualRange, -1).Select(agent => agent.Position).ToList();
    }
    
    /// <summary>
    /// Explores the environment and returns a list of positions of the pellets.
    /// </summary>
    /// <returns></returns>
    private List<Position> ExplorePellets()
    {
        return Layer.PelletEnvironment.Explore(Position, VisualRange, -1).Select(agent => agent.Position).ToList();
    }
    
    /// <summary>
    /// Explores the environment and returns a list of positions of the power pellets.
    /// </summary>
    /// <returns></returns>
    private List<Position> ExplorePowerPellets()
    {
        return Layer.PowerPelletEnvironment.Explore(Position, VisualRange, -1).Select(agent => agent.Position).ToList();
    }

    /// <summary>
    /// Explores the environment and returns a list of positions of the occupiable spots.
    /// </summary>
    private List<Position> ExploreOccupiablePositions()
    {
        return Layer.OccupiableSpotsEnvironment.Explore(Position, VisualRange, -1).Select(agent => agent.Position).ToList();
    }
    
    private int GetScore() => 
        Layer.Score;
    
    private readonly Random _random = new();
    
    public Direction Direction { get; set; }
    
    public bool PoweredUp { get; set; }
    
    public int PoweredUpTime { get; set; }
    
    [PropertyDescription]
    public int VisualRange { get; set; }
    
    [PropertyDescription]
    public int Lives { get; set; }

    
}