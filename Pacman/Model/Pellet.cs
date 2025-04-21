using System;
using Mars.Interfaces.Agents;
using Mars.Interfaces.Environments;
using Mars.Interfaces.Layers;

namespace Pacman.Model;

public class Pellet : Item
{
    public override void Init(MazeLayer layer)
    {
        Layer = layer;
        Name = "Pellet";
    }
}