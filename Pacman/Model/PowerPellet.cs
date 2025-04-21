namespace Pacman.Model;

public class PowerPellet : Item
{
    public override void Init(MazeLayer layer)
    {
        Layer = layer;
        Name = "PowerPellet";
    }
}