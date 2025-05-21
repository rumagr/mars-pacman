using System;
using System.Collections.Generic;
using System.Linq;
using Mars.Interfaces.Annotations;
using Mars.Interfaces.Environments;
using Mars.Numerics;
using Mars.Numerics.Distances;

namespace Pacman.Model;

public class PacManAgent : MovingAgent
{
    public override void Init(MazeLayer layer)
    {
        Layer = layer;
        Position = new Position(StartX, StartY);
        Layer.PacManAgentEnvironment.Insert(this);
        for (int i = 0; i < QTable.Length; i++)
        {
            QTable[i] = new int[5];
        }
    }

public override void Tick()
    {
        var powerPelletPositions = ExplorePowerPelletPositions();
        var pelletPositions = ExplorePelletPositions();
        // var ghostPositions = ExploreGhostPositions();
        var ghostPositions = ExploreGhosts().Where(agent => agent.Mode != GhostMode.Eaten).Select(agent => agent.Position).ToList();
        var occupiablePositions = ExploreOccupiablePositions();
        var dangerousGhosts = ExploreDangerousGhosts().Where(agent => agent.Mode != GhostMode.Eaten).Select(agent => agent.Position).ToList();
        var nextPelletPosition = getNearestPelletPosition(pelletPositions); 
        var nearestGhostPosition = getNearestGhostPosition(ghostPositions);
        var nearestPowerPelletPosition = getNearestPowerPelletPosition(powerPelletPositions);
        var nearestDangerousGhostPosition = getNearestGhostPosition(dangerousGhosts);
        
        var state = 0;
        
        if (PoweredUp)
        {
            
            if (nearestDangerousGhostPosition != null)
            {
                state = powered_up_ghost; 
            }
            else
            {
                if (nextPelletPosition != null) //esse pallets
                {
                    state = powered_up_no_ghost_pellet;
                }
                else 
                {
                    state = powered_up_no_ghost_no_pellet;
                }

            }
        } 
        else
        {
            if (dangerousGhosts.Count > 0) // ansonsten, wenn geist bestimmte distanz unterschreitet: fliehen Richtung Power Pellet, wenn es näher ist als Geist
            {
                if (nearestPowerPelletPosition != null)
                {
                    state = no_powered_up_ghost_powerpellet; 
                }
                else
                {
                    state = no_powered_up_ghost_no_powerpellet;
                }
            }
            else 
            {
                if (nextPelletPosition != null) 
                {
                    state = no_powered_up_no_ghost_no_pellet;
                }
                else 
                {
                    state = no_powered_up_no_ghost_no_pellet;
                }
            }
        }
        
        
        
        var action = QTable[state].ToList().IndexOf(QTable[state].Max());
        
        
    }


    private void goUp(List<Position> occupiablePositions)
    {
        Position = new Position(Position.X, Position.Y+1);

        if (occupiablePositions.Contains(Position))
        {
            MoveTowardsGoal(Position);
        }
    }

    private void goDown(List<Position> occupiablePositions)
    {
        Position = new Position(Position.X , Position.Y-1);

        if (occupiablePositions.Contains(Position))
        {
            MoveTowardsGoal(Position);
        }
    }
    
    private void goRight(List<Position> occupiablePositions)
    {
        Position = new Position(Position.X + 1, Position.Y);

        if (occupiablePositions.Contains(Position))
        {
            MoveTowardsGoal(Position);
        }
    }
    
    private void goLeft(List<Position> occupiablePositions)
    {
        Position = new Position(Position.X - 1, Position.Y);

        if (occupiablePositions.Contains(Position))
        {
            MoveTowardsGoal(Position);
        }
    }
    
    private List<GhostAgent> ExploreDangerousGhosts()
    {
        return Layer.GhostAgentEnvironment.Explore(Position, 3, -1).ToList();
    }
    
    private Position getNearestPowerPelletPosition(List<Position> powerPelletPositions)
    {
        if (powerPelletPositions.Count > 0)
        {
            Position nearestPowerPelletPosition = powerPelletPositions[0];
            for (int i = 0; i < powerPelletPositions.Count; i++)
            {
                if (GetDistance(nearestPowerPelletPosition) > GetDistance(powerPelletPositions[i]))
                {
                    nearestPowerPelletPosition = powerPelletPositions[i];
                }
            }
            return nearestPowerPelletPosition;
        }
        return null; 
    }
    private Position getNearestGhostPosition(List<Position> ghostPositions)
    {
        if (ghostPositions.Count > 0)
        {
            Position nearestGhostPosition = ghostPositions[0];
            for (int i = 0; i < ghostPositions.Count; i++)
            {
                if (GetDistance(nearestGhostPosition) > GetDistance(ghostPositions[i]) )
                {
                    nearestGhostPosition = ghostPositions[i];
                }
            }
            return nearestGhostPosition;
        }
        return null; 
    }
    
    private Position getNearestPelletPosition(List<Position> pelletPositions)
    {
        if (pelletPositions.Count > 0)
        {
            Position nearestPelletPosition = pelletPositions[0];
            for (int i = 0; i < pelletPositions.Count; i++)
            {
                if (GetDistance(nearestPelletPosition) > GetDistance(pelletPositions[i]) && (pelletPositions[i].X == Position.X || pelletPositions[i].Y == Position.Y))
                {
                    nearestPelletPosition = pelletPositions[i];
                }
            }
            return nearestPelletPosition;
        }
        return null; 
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
        
    //QTable
    private int[][] QTable = new int[7][];
    
    //
    private static double learningRate = 0.1;
    
    private static double discountFactor = 0.5;

    private static double explorationRate = 0.2;
    
    private int pelletsEaten = 0;
    
    private static int thresh = 5; 
    
    //Rewards and penalties
    private static int pellet_reward = 1;
    private static int power_pellet_reward = 5;
    private static int ghost_eaten_reward = 10;
    
    //private static int wall_penalty = -1;
    private static int ghost_penalty = -20;
    
    //states 
    private static int powered_up_ghost = 0;
    private static int powered_up_no_ghost_pellet = 1;
    private static int powered_up_no_ghost_no_pellet = 2;
    private static int no_powered_up_ghost_no_powerpellet = 3;
    private static int no_powered_up_ghost_powerpellet = 4;
    private static int no_powered_up_no_ghost_pellet = 5;
    private static int no_powered_up_no_ghost_no_pellet = 6;
    
    //actions 
    private static int eat_pellets = 0;
    private static int hunt_ghost = 1;
    private static int eat_power_pellet = 2;
    private static int run_away = 3;
    private static int random_walk = 4;
}