using System;
using System.IO;
using Mars.Components.Starter;
using Mars.Interfaces.Model;
using Pacman.Model;

namespace Pacman;

internal static class Program
{
    private static void Main()
    {
        // Create a new model description and add model components to it
        var description = new ModelDescription();
        description.AddLayer<MazeLayer>();
        description.AddAgent<GhostAgent, MazeLayer>();
        description.AddAgent<PacManAgent, MazeLayer>();
        description.AddAgent<Pellet, MazeLayer>();
        description.AddAgent<PowerPellet, MazeLayer>();
        description.AddAgent<OccupiableSpot, MazeLayer>();
        description.AddAgent<SmartGhostAgent, MazeLayer>();

        // Load the simulation configuration from a JSON configuration file
        var file = File.ReadAllText("config.json");
        var config = SimulationConfig.Deserialize(file);

        for(int i = 0; i < 100; ++i)
        {
            // Couple model description and simulation configuration
            var starter = SimulationStarter.Start(description, config);

            // Run the simulation
            var handle = starter.Run();
        
            // Close the program
            Console.WriteLine("Successfully executed iterations: " + handle.Iterations);
            starter.Dispose();
        }

    }
}