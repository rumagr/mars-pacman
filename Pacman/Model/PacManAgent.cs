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
        var dangerousGhosts = ExploreDangerousGhosts();
        var nextPelletPosition = getNearestPelletPosition(pelletPositions); 
        
        if (PoweredUp)
        {
            var target = getNearestGhostPosition(ghostPositions);
            if (target != null)
            {
                MoveTowardsGoal(target);
                return;
            }
            else
            {
                MoveTowardsGoal(nextPelletPosition);
                return; 
                //esse pallets 
            }
        } 
        else
        {
            if (dangerousGhosts.Count > 0) // ansonsten, wenn geist bestimmte distanz unterschreitet: fliehen Richtung Power Pellet, wenn es näher ist als Geist
            {
                var nearestPowerPelletPosition = getNearestPowerPelletPosition(powerPelletPositions);
                var nearestGhostPosition = getNearestGhostPosition(ghostPositions);
                if (nearestPowerPelletPosition != null && ((nearestGhostPosition.X > Position.X && nearestPowerPelletPosition.X < Position.X) || (nearestGhostPosition.X < Position.X && nearestPowerPelletPosition.X > Position.X)
                    || (nearestGhostPosition.Y > Position.Y && nearestPowerPelletPosition.Y < Position.Y) || (nearestGhostPosition.Y < Position.Y && nearestPowerPelletPosition.Y > Position.Y)))
                {
                    MoveTowardsGoal(nearestPowerPelletPosition);
                }
                else
                {
                    var target = Position;
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
                    else
                    {
                        MoveTowardsGoal(occupiablePositions.FirstOrDefault(pos => !ghostPositions.Contains(pos)));
                    }
                }
            }
            
            // wenn 2 oder mehr geister sichtbar sind und power pellet in der nähe, dann pp essen
        
            // wenn counter < pelletthreshold, normale pellets essen (closest)
            MoveTowardsGoal(nextPelletPosition);
            return; 
            // nähestes powerpellet essen
                
        }
   
        
        
        
        // Rule-based behaviour
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
                if (GetDistance(nearestGhostPosition) > GetDistance(ghostPositions[i]))
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
}