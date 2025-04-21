using System;
using System.Collections.Generic;
using System.Linq;
using Mars.Interfaces.Agents;
using System.Text.Json;
using Fleck;
using System.Threading;
using System.Threading.Tasks;

namespace Pacman.Model;
public static class DataVisualizationServer
{
    public static volatile int CurrentTick = -1;
        
    private static IWebSocketConnection _client; 
    private static WebSocketServer _server;
    private static CancellationTokenSource _cts;
    private static Task _serverTask;

    public static void Start()
    {
        _cts = new CancellationTokenSource();
        var token = _cts.Token;
        _server = new WebSocketServer("ws://127.0.0.1:8181");

        _server.Start(socket =>
        {
            socket.OnOpen = () =>
            {
                _client = socket; 
            };

            socket.OnMessage = message =>
            {
                if (int.TryParse(message, out var tick))
                {
                    CurrentTick = tick;
                }
            };

            socket.OnClose = () =>
            {
                _client = null;
            };
        });
        
        while (!token.IsCancellationRequested)
        {
            Thread.Sleep(100);
        }
    }
    
    public static void RunInBackground()
    {
        _serverTask = Task.Run(() => Start());
    }

    public static void Stop()
    {
        if (_cts == null)
            return;
        
        if (_client != null && _client.IsAvailable)
        {
            _client.Send("close");
            Thread.Sleep(100);
        }
        
        _cts.Cancel();       
        _serverTask?.Wait(); 
        _server?.Dispose();  

        _cts.Dispose();
        _cts = null;
        _serverTask = null;
        _server = null;
        _client = null;
    }
        
    public static void SendData(int score, IEnumerable<IAgent<MazeLayer>> agents)
    {
        var payload = new
        {
            score = score,
            agents = agents.Select<IAgent<MazeLayer>, object>(a =>
            {
                if (a is GhostAgent ghost)
                {
                    return new
                    {
                        type = "GhostAgent",
                        name = ghost.Name,
                        mode = ghost.Mode.ToString(),
                        x = ghost.Position.X,
                        y = ghost.Position.Y
                    };
                }
                if (a is PacManAgent pacman)
                {
                    return new
                        {
                            type = "PacManAgent",
                            poweredUp = pacman.PoweredUp,
                            lives = pacman.Lives,
                            x = pacman.Position.X,
                            y = pacman.Position.Y,
                            direction = pacman.Direction.ToString()
                        };
                }
                if (a is Pellet pellet)
                {
                    return new
                    {
                        type = "Pellet",
                        id = pellet.ID,
                        x = pellet.Position.X,
                        y = pellet.Position.Y
                    };
                }
                if (a is PowerPellet powerPellet)
                {
                    return new
                    {
                        type = "PowerPellet",
                        id = powerPellet.ID,
                        x = powerPellet.Position.X,
                        y = powerPellet.Position.Y
                    };
                }
                {
                    return new
                    {
                        type = "UnknownAgent"
                    };
                }
            })
        };

        string json = JsonSerializer.Serialize(payload);
        _client?.Send(json);
    }
    
    
        
    public static bool Connected()
    {
        return _client != null && _client.IsAvailable;
    }
}