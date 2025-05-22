#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Mars.Interfaces.Environments;
using Mars.Numerics;
using System.IO;
using System.Text.Json;


namespace Pacman.Model;

public class SmartGhostAgent : GhostAgent
{
    public override void Tick()
    {
        ladeQTable(); 
        
        if (ProcessGhostState()) return;
        
        var pacMan = ExplorePacMan();
        var teammates = ExploreTeam();
        var foundPacMan = pacMan != null;
        var target = GetRandomCell();
        var occupiablePositions = Layer.OccupiableSpotsEnvironment.Explore(Position, VisualRange, -1).Select(agent => agent.Position).ToList();
        
        var pacmanFound = pacMan != null ? 1 : 0;
        var pacmanPoweredUp = (pacMan != null && pacMan.PoweredUp) ? 1 : 0;
        var pacmanDirection = getDirection(pacMan.Position); 
        var ghost1Mode = mode2int(teammates[0].Mode);
        var ghost2Mode = mode2int(teammates[1].Mode);
        var ghost3Mode = mode2int(teammates[2].Mode);

        var actions = QTable[ghost1Mode][ghost2Mode][ghost3Mode][pacmanFound][pacmanPoweredUp][pacmanDirection];

        var action = getValidAction(actions); 
        var randomAction = new Random().Next(0,8); 
        
        if (_random.NextDouble() < explorationRate)
        {
            action = randomAction;
        }
 
        switch (action)
        {
            case up_chase:
                goUp(occupiablePositions);
                break;
            case down_chase:
                goDown(occupiablePositions);
                break;
            case left_chase:
                goLeft(occupiablePositions);
                break;
            case right_chase:
                goRight(occupiablePositions);
                break;
            case up_scatter:
                goUp(occupiablePositions);
                break;
            case down_scatter:
                goDown(occupiablePositions);
                break;
            case left_scatter:
                goLeft(occupiablePositions);
                break;
            case right_scatter:
                goRight(occupiablePositions);
                break;
        }

        //todo calculate qValue 

        QTable[ghost1Mode][ghost2Mode][ghost3Mode][pacmanFound][pacmanPoweredUp][pacmanDirection][action] =
            calculateReward(pacMan, pacmanPoweredUp); 
        
        SaveQTable("../../../Model/QTable_" + Name + ".json");
    }

    private int calculateReward(PacManAgent pacMan,int pacmanPoweredUp)
    {
        if (pacmanPoweredUp!=0 && Position.Equals(pacMan.Position))
        {
            return eaten_by_pacman; 
        }
        else if (pacMan.PoweredUp && Position.Equals(pacMan.Position))
        {
            return eaten_by_pacman;
        }
        else
        {
            return 0; 
        }
    }
    
    private int getValidAction(int[] actions)
    {
        //todo find valid action 
        return -1; 
    }

    private int mode2int(GhostMode mode)
    {
        switch (mode)
        {
            case GhostMode.Chase:
                return 0;
            case GhostMode.Scatter:
                return 1;
            case GhostMode.Frightened:
                return 2;
            case GhostMode.Eaten:
                return 3;
            default:
                throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
        }
    }
        
    
    
    private void ladeQTable()
    {
        if (!LoadQTable("../../../Model/QTable_" + Name + ".json"))
        {
            for (int i = 0; i < 4; i++)
            {
                QTable[i] = new int[4][][][][][];
                for (int j = 0; j < 4; j++)
                {
                    QTable[i][j] = new int[4][][][][];
                    
                    for (int k = 0; k < 4; k++)
                    {
                        QTable[i][j][k] = new int[2][][][];
                        for (int l = 0; l < 2; l++)
                        {
                            QTable[i][j][k][l] = new int[2][][];
                            for (int m = 0; m < 2; m++)
                            {
                                    QTable[i][j][k][l][m] = new int[5][];
                                    for (int o = 0; o < 4; o++)
                                    {
                                        QTable[i][j][k][l][m][o] = new int[4];
                                        for (int f = 0; f < 4; f++)
                                        {
                                            QTable[i][j][k][l][m][o][f] = 0;
                                        }
                                    }
                            }
                        }
                    }
                    
                }
            }
        }       
    }
    
    /// <summary>
    /// returns the directon of the other position 
    /// </summary>
    /// <param name="other_position"></param>
    /// <returns>the direction of the other position relative to this one</returns>
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
    
    /// Movement
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
    
    public void SaveQTable(string filePath)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        File.WriteAllText(filePath, JsonSerializer.Serialize(QTable, options));
    }

    public bool LoadQTable(string filePath)
    {
        if (File.Exists(filePath))
        {
            QTable = JsonSerializer.Deserialize<int[][][][][][][]>(File.ReadAllText(filePath));
            return true;
        }
        else
        {
            return false; 
        }
    }
    
    /// <summary>
    /// Processes the ghost state and returns true if the ghost is not controllable.
    /// </summary>
    /// <returns></returns>
    private bool ProcessGhostState()
    {
        if (ReleaseTimer <= ReleaseTick)
        {
            ReleaseTimer++;
            return true;
        }
        if (Mode == GhostMode.Frightened)
        {
            if (Layer.GetCurrentTick() % 2 == 0) return true;
            return false;
        }
        if (Mode == GhostMode.Eaten)
        {
            MoveTowardsGoal(new Position(HouseCellX, HouseCellY));
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// Explores the environment and returns a list of the teammates.
    /// </summary>
    private List<GhostAgent> ExploreTeam()
    {
        return Layer.GhostAgents
            .Where(agent => agent != this)
            .ToList();;
    }
    
    /// <summary>
    /// Explores the environment and returns the PacManAgent instance.
    /// Can be null if no PacManAgent is found.
    /// </summary>
    private PacManAgent? ExplorePacMan() => Layer.PacManAgentEnvironment.Explore(Position, VisualRange).FirstOrDefault();
    
    
    /// <summary>
    /// Enters the chase mode if the ghost is in scatter mode.
    /// </summary>
    /// <returns></returns>
    private bool EnterChaseMode()
    {
        if (Mode == GhostMode.Scatter)
        {
            Mode = GhostMode.Chase;
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// Enters the scatter mode if the ghost is in chase mode.
    /// </summary>
    /// <returns></returns>
    private bool EnterScatterMode()
    {
        if (Mode == GhostMode.Chase)
        {
            Mode = GhostMode.Scatter;
            return true;
        }
        return false;
    }
    
    private bool Frightened() => Mode == GhostMode.Frightened;
    
    //directions
    private const int no_position = 4; 
    private const int up = 0;
    private const int down = 1;
    private const int left = 2;
    private const int right = 3;
    
    //QTable Ghost1,2,3 frightened/chase/scatter/eaten, pacman_found/pacman_not_found, pacman powered_up/not_powered_up, pacman direction
    private int[][][][][][][] QTable = new int[4][][][][][][];

    //QLearning parameters 
    private const double learningRate = 0.1;
    
    private const double discountFactor = 0.5;

    private const double explorationRate = 0.2;
    
    //Rewards and penalties 
    private const int pacman_eaten = 10;
    private const int eaten_by_pacman = -10;
    
    //modes
    private const int up_chase = 0;
    private const int down_chase = 1;
    private const int left_chase = 2;
    private const int right_chase = 3;
    private const int up_scatter = 4;
    private const int down_scatter = 5;
    private const int left_scatter = 6;
    private const int right_scatter = 7;
    
    private readonly Random _random = new();
}

