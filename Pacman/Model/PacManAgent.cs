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

        if (PoweredUp)
        {
            pelletsEaten = 0; 
            var target = getNearestGhostPosition(ghostPositions);
            if (target != null)
            {
                MoveTowardsGoal(target);
            }
            else
            {
                if (nextPelletPosition != null) //esse pallets
                {
                    MoveTowardsGoal(nextPelletPosition);
                }
                else 
                {
                    var randomPosition = occupiablePositions[_random.Next(0, occupiablePositions.Count)];
                    MoveTowardsGoal(randomPosition);
                }

            }
        } 
        else
        {
            if (dangerousGhosts.Count > 0) // ansonsten, wenn geist bestimmte distanz unterschreitet: fliehen Richtung Power Pellet, wenn es näher ist als Geist
            {
                var nearestPowerPelletPosition = getNearestPowerPelletPosition(powerPelletPositions);
                var nearestGhostPosition = getNearestGhostPosition(dangerousGhosts);
                if (nearestPowerPelletPosition != null && ((nearestGhostPosition.X > Position.X && nearestPowerPelletPosition.X < Position.X) || (nearestGhostPosition.X < Position.X && nearestPowerPelletPosition.X > Position.X)
                    || (nearestGhostPosition.Y > Position.Y && nearestPowerPelletPosition.Y < Position.Y) || (nearestGhostPosition.Y < Position.Y && nearestPowerPelletPosition.Y > Position.Y)))
                {
                    MoveTowardsGoal(nearestPowerPelletPosition);
                }
                else
                {
                    Position target = null;
                    if (nearestGhostPosition.X > Position.X)
                    {
                        target = new Position(Position.X - 1, Position.Y);
                    }
                    else if (nearestGhostPosition.X < Position.X)
                    {
                        target = new Position(Position.X + 1, Position.Y);
                    }
                    else if (nearestGhostPosition.Y > Position.Y)
                    {
                        target = new Position(Position.X, Position.Y - 1);
                    }
                    else
                    {
                        target = new Position(Position.X, Position.Y + 1);
                    }

                    if (occupiablePositions.Contains(target))
                    {
                        MoveTowardsGoal(target);
                    }
                    else //bewege in beliebige richtung, die nicht in die Richtung des Geistes ist
                    {
                        var safePositions = occupiablePositions
                            .Where(pos => !(Math.Sign(pos.X - Position.X) == Math.Sign(nearestGhostPosition.X - Position.X) &&
                                            Math.Sign(pos.Y - Position.Y) == Math.Sign(nearestGhostPosition.Y - Position.Y)))
                            .ToList();

                        if (safePositions.Any())
                        {
                            var targetPosition = safePositions[_random.Next(0, safePositions.Count)];
                            MoveTowardsGoal(targetPosition);
                        }
                        else
                        {
                            // Fallback if no safe positions are found
                            var randomPosition = occupiablePositions[_random.Next(0, occupiablePositions.Count)];
                            MoveTowardsGoal(randomPosition);
                        }
                    }
                }
            }
            else if (ghostPositions.Count >= 2 && powerPelletPositions.Count > 0) // wenn 2 oder mehr geister sichtbar sind und power pellet in der nähe, dann pp essen
            {
                MoveTowardsGoal(getNearestPowerPelletPosition(powerPelletPositions)); 
            }
            else if (pelletsEaten >= thresh && (powerPelletPositions.Count > 0)) // wenn counter > pelletthreshold, power pellets essen (closest)
            {

                    MoveTowardsGoal(getNearestPowerPelletPosition(powerPelletPositions)); 

            }
            else // nähestes pellet essen
            {
                if (nextPelletPosition != null) //esse pallets
                {
                    MoveTowardsGoal(nextPelletPosition);
                }
                else 
                {
                    var randomPosition = occupiablePositions[_random.Next(0, occupiablePositions.Count)];
                    MoveTowardsGoal(randomPosition);
                }
            }
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