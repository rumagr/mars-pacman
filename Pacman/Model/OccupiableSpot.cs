using System;
using Mars.Interfaces.Agents;
using Mars.Interfaces.Environments;

namespace Pacman.Model;

public class OccupiableSpot : IAgent<MazeLayer>, IPositionable
{
    public void Init(MazeLayer layer)
    {
        
    }

    public void Tick()
    {
        
    }

    public Guid ID { get; set; }
    public Position Position { get; set; }
}