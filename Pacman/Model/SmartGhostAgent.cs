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
        var pacmanDirection = 0;
        if (pacMan != null)
        {
            pacmanDirection = getDirection(pacMan.Position); 
        }
        var ghost1Mode = mode2int(teammates[0].Mode);
        var ghost2Mode = mode2int(teammates[1].Mode);
        var ghost3Mode = mode2int(teammates[2].Mode);

        var actions = QTable[ghost1Mode][ghost2Mode][ghost3Mode][pacmanFound][pacmanPoweredUp][pacmanDirection].ToList();

        var action = getValidAction(actions, occupiablePositions); 
        var randomAction = new Random().Next(0,8); 
        
        if (_random.NextDouble() < explorationRate)
        {
            action = randomAction;
        }

        var newMode = GhostMode.Chase; 
        
        switch (action)
        {
            case up_chase:
                goUp(occupiablePositions);
                newMode = GhostMode.Chase; 
                break;
            case down_chase:
                goDown(occupiablePositions);
                newMode = GhostMode.Chase;
                break;
            case left_chase:
                goLeft(occupiablePositions);
                newMode = GhostMode.Chase;
                break;
            case right_chase:
                goRight(occupiablePositions);
                newMode = GhostMode.Chase;
                break;
            case up_scatter:
                goUp(occupiablePositions);
                newMode = GhostMode.Scatter; 
                break;
            case down_scatter:
                goDown(occupiablePositions);
                newMode = GhostMode.Scatter; 
                break;
            case left_scatter:
                goLeft(occupiablePositions);
                newMode = GhostMode.Scatter;
                break;
            case right_scatter:
                goRight(occupiablePositions);
                newMode = GhostMode.Scatter;
                break;
        }
        
        if (Mode != GhostMode.Frightened && Mode != GhostMode.Eaten)
        { 
            Mode = newMode;
        }

        
        var qValue = QTable[ghost1Mode][ghost2Mode][ghost3Mode][pacmanFound][pacmanPoweredUp][pacmanDirection][action] 
            + learningRate * (calculateReward(pacMan) + discountFactor * QTable[ghost1Mode][ghost2Mode][ghost3Mode][pacmanFound][pacmanPoweredUp][pacmanDirection][action] - QTable[ghost1Mode][ghost2Mode][ghost3Mode][pacmanFound][pacmanPoweredUp][pacmanDirection][action]);;

        if (qValue < 100000 && qValue > -100000)
        {
            QTable[ghost1Mode][ghost2Mode][ghost3Mode][pacmanFound][pacmanPoweredUp][pacmanDirection][action] = qValue; 
        }
           
        SaveQTable("../../../Model/QTable_" + Name + ".json");
    }

    private int calculateReward(PacManAgent pacMan)
    {
        if (pacMan == null)
        {
            return 0; 
        }
        
        if (Position.Equals(pacMan.Position) && !pacMan.PoweredUp)
        {
            return pacman_eaten; 
        }
        else if (Position.Equals(pacMan.Position) && pacMan.PoweredUp)
        {
            return eaten_by_pacman;
        }
        else
        {
            return 0; 
        }
    }
    
    private int getValidAction(List<double> actions, List<Position> occupiablePositions)
    {
        var AvailableActions = new List<int>();
        
        if (occupiablePositions.Contains(new Position(Position.X, Position.Y + 1)))
        { 
            AvailableActions.Add(up_chase);
            AvailableActions.Add(up_scatter);
        }
        else if (occupiablePositions.Contains(new Position(Position.X, Position.Y - 1)))
        { 
            AvailableActions.Add(down_chase);
            AvailableActions.Add(down_scatter);
        }
        else if (occupiablePositions.Contains(new Position(Position.X -1 , Position.Y)))
        { 
            AvailableActions.Add(left_chase);
            AvailableActions.Add(left_scatter);
        }
        else if (occupiablePositions.Contains(new Position(Position.X+1, Position.Y )))
        { 
            AvailableActions.Add(right_chase);
            AvailableActions.Add(right_scatter);
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
                QTable[i] = new double[4][][][][][];
                for (int j = 0; j < 4; j++)
                {
                    QTable[i][j] = new double[4][][][][];
                    
                    for (int k = 0; k < 4; k++)
                    {
                        QTable[i][j][k] = new double[2][][][];
                        for (int l = 0; l < 2; l++)
                        {
                            QTable[i][j][k][l] = new double[2][][];
                            for (int m = 0; m < 2; m++)
                            {
                                    QTable[i][j][k][l][m] = new double[5][];
                                    for (int o = 0; o < 5; o++)
                                    {
                                        QTable[i][j][k][l][m][o] = new double[8];
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
            QTable = JsonSerializer.Deserialize<double[][][][][][][]>(File.ReadAllText(filePath));
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
    private double[][][][][][][] QTable = new double[4][][][][][][];

    //QLearning parameters 
    private const double learningRate = 0.1;
    
    private const double discountFactor = 0.5;

    private const double explorationRate = 0.8;
    
    //Rewards and penalties 
    private const int pacman_eaten = 100;
    private const int eaten_by_pacman = -100;
    
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

