using System;
using System.Collections.Generic;
using System.Linq;
using Mars.Interfaces.Annotations;
using Mars.Interfaces.Environments;
using Mars.Numerics;
using Mars.Numerics.Distances;
using System.IO;
using System.Text.Json;
using Microsoft.VisualBasic.CompilerServices;

namespace Pacman.Model;

public class PacManAgent : MovingAgent
{
    public override void Init(MazeLayer layer)
    {
        Layer = layer;
        Position = new Position(StartX, StartY);
        Layer.PacManAgentEnvironment.Insert(this);
        
        if (!LoadQTable("../../../Model/QTable.json"))
        {
            for (int i = 0; i < 2; i++)
            {
                QTable[i] = new int[5][][][];
                for (int j = 0; j < 5; j++)
                {
                    QTable[i][j] = new int[5][][];
                    
                    for (int k = 0; k < 5; k++)
                    {
                        QTable[i][j][k] = new int[5][];
                        for (int l = 0; l < 5; l++)
                        {
                            QTable[i][j][k][l] = new int[4];
                        }
                    }
                    
                }
            }
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
        
        var powered_up = PoweredUp ? 1 : 0;
        var ghost_direction = getDirection(nearestDangerousGhostPosition);
        var pellet_direction = getDirection(nextPelletPosition);
        var power_pellet_direction = getDirection(nearestPowerPelletPosition);

        var actions = QTable[powered_up][ghost_direction][pellet_direction][power_pellet_direction].ToList();
        var random_action = _random.Next(0, 4);
        
        var action = getValidAction(actions, occupiablePositions);
        
        if (_random.NextDouble() < explorationRate)
        {
            action = random_action;
        }
        
        if (action == 0)
        {
            goUp(occupiablePositions);
        }
        else if (action == 1)
        {
            goDown(occupiablePositions);
        }
        else if (action == 2)
        {
            goLeft(occupiablePositions);
        }
        else if (action == 3)
        {
            goRight(occupiablePositions);
        }
        
        //todo calculate qValue
        //calculate reward 
        var qValue = QTable[powered_up][ghost_direction][pellet_direction][power_pellet_direction][action] + learningRate * (calculateReward() + discountFactor * QTable[powered_up][ghost_direction][pellet_direction][power_pellet_direction][action]);
        
        QTable[powered_up][ghost_direction][pellet_direction][power_pellet_direction][action] = (int) qValue; 
        
        SaveQTable("../../../Model/QTable.json");
    }

    private int getValidAction(List<int> actions, List<Position> occupiablePostitions)
    {

        var AvailableActions = new List<int>();
        
        if (occupiablePostitions.Contains(new Position(Position.X, Position.Y + 1)))
        { 
            AvailableActions.Add(0);
        }
        else if (occupiablePostitions.Contains(new Position(Position.X, Position.Y - 1)))
        { 
            AvailableActions.Add(1);

        }
        else if (occupiablePostitions.Contains(new Position(Position.X -1 , Position.Y)))
        { 
            AvailableActions.Add(2);

        }
        else if (occupiablePostitions.Contains(new Position(Position.X+1, Position.Y )))
        { 
            AvailableActions.Add(3);

        }
        
        for (int i = 0; i < actions.Count; i++)
        {
            if (!AvailableActions.Contains(i))
            {
                actions[i] = int.MinValue;
            }
        }

        return actions.IndexOf(actions.Max()); 
    }


    private int calculateReward()
    {
        int reward = 0;
        var powerPelletPositions = ExplorePowerPelletPositions();
        var pelletPositions = ExplorePelletPositions();
        var dangerousGhosts = ExploreDangerousGhosts().Where(agent => agent.Mode != GhostMode.Eaten).Select(agent => agent.Position).ToList();
        
        if (pelletPositions.Contains(Position))
        {
            reward += pellet_reward;
        }
        
        if (powerPelletPositions.Contains(Position))
        {
            reward += power_pellet_reward;
        }
        
        if (dangerousGhosts.Contains(Position) && !PoweredUp)
        {
            reward += ghost_penalty;
        }
        else if (dangerousGhosts.Contains(Position) && PoweredUp)
        {
            reward += ghost_eaten_reward;
        }
        
        return reward; 
    }

    private int getDirection(Position other_position)
    {
        if (other_position == null)
        {
            return no_position;
        }
        
        if (Position.X < other_position.X)
        {
            return right;
        }
        else if (Position.X > other_position.X)
        {
            return left;
        }
        else if (Position.Y < other_position.Y)
        {
            return up;
        }
        else
        {
            return down;
        }
        
    }

    private void goUp(List<Position> occupiablePositions)
    {
        var newPosition = new Position(Position.X, Position.Y+1);

        if (occupiablePositions.Contains(newPosition))
        {
            MoveTowardsGoal(newPosition);
        }
    }

    private void goDown(List<Position> occupiablePositions)
    {
        var newPosition = new Position(Position.X , Position.Y-1);

        if (occupiablePositions.Contains(newPosition))
        {
            MoveTowardsGoal(newPosition);
        }
    }
    
    private void goRight(List<Position> occupiablePositions)
    {
        var newPosition = new Position(Position.X + 1, Position.Y);

        if (occupiablePositions.Contains(newPosition))
        {
            MoveTowardsGoal(newPosition);
        }
    }
    
    private void goLeft(List<Position> occupiablePositions)
    {
        var newPosition = new Position(Position.X - 1, Position.Y);

        if (occupiablePositions.Contains(newPosition))
        {
            MoveTowardsGoal(newPosition);
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

    public void SaveQTable(string filePath)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        File.WriteAllText(filePath, JsonSerializer.Serialize(QTable, options));
    }

    public bool LoadQTable(string filePath)
    {
        if (File.Exists(filePath))
        {
            QTable = JsonSerializer.Deserialize<int[][][][][]>(File.ReadAllText(filePath));
            return true;
        }
        else
        {
            return false; 
        }
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
        
    //QTable powered up/not powered up, ghost directions, pellet directions, power pellet directions 
    private int[][][][][] QTable = new int[2][][][][];
    
    //
    private static double learningRate = 0.1;
    
    private static double discountFactor = 0.5;

    private static double explorationRate = 0.8;
    
    private int pelletsEaten = 0;
    
    private static int thresh = 5; 
    
    //Rewards and penalties
    private static int pellet_reward = 1;
    private static int power_pellet_reward = 5;
    private static int ghost_eaten_reward = 10;
    
    //private static int wall_penalty = -1;
    private static int ghost_penalty = -20;
    
    //directions
    private static int no_position = 4; 
    private static int up = 0;
    private static int down = 1;
    private static int left = 2;
    private static int right = 3;
}