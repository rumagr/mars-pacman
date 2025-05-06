#nullable enable
using System.Collections.Generic;
using System.Linq;
using Mars.Interfaces.Environments;
using Mars.Numerics;


namespace Pacman.Model;

public class SmartGhostAgent : GhostAgent
{
    public override void Tick()
    {
        if (ProcessGhostState()) return;
        
        var pacMan = ExplorePacMan();
        var teammates = ExploreTeam();
        var foundPacMan = pacMan != null;
        var target = GetRandomCell();
        
        if (foundPacMan)
        {
            if (Frightened())
            {
                var pacManPosition = pacMan.Position;
                target = ExploreOccupiablePositions()
                    .OrderByDescending(spot => Distance.Euclidean(spot.X, spot.Y, pacManPosition.X, pacManPosition.Y))
                    .FirstOrDefault();
            }
            else
            {
                EnterChaseMode();
                target = pacMan.Position;
            }
        }
        else
        {
            EnterScatterMode();
        }
        MoveTowardsGoal(target);
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
}