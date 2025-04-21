using System;
using Mars.Interfaces.Agents;
using Mars.Interfaces.Environments;
using Mars.Interfaces.Layers;

namespace Pacman.Model;

public class Item: IAgent<MazeLayer>, IPositionable
{
    public virtual void Init(MazeLayer layer)
    {
        
    }

    public void Tick()
    {
        // do nothing
    }
    
    /// <summary>
    ///     Removes this agent from the simulation and, by extension, from the visualization.
    /// </summary>
    public void RemoveFromSimulation()
    {
        switch (this)
        {
            case Pellet:
                Layer.PelletEnvironment.Remove((Pellet)this);
                Layer.Pellets.Remove((Pellet)this);
                break;
            case PowerPellet:
                Layer.PowerPelletEnvironment.Remove((PowerPellet)this);
                Layer.PowerPellets.Remove((PowerPellet)this);
                break;
            default:
                break;
        }
        UnregisterAgentHandle.Invoke(Layer, this);
    }
    
    public UnregisterAgent UnregisterAgentHandle { get; set; }

    public Guid ID { get; set; }
    public Position Position { get; set; }
    
    protected MazeLayer Layer;
    
    public String Name { get; protected set; }
}