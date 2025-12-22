using System.Collections.Concurrent;
using Google.Protobuf;
using Microsoft.AspNetCore.SignalR;
using Synapse.Shared.Protocol;

namespace Synapse.Server.Hubs;

public struct BotMoveData
{
    public string Id { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
}

public class GameHub : Hub
{
    private WorldSimulation _world;
    
    private static readonly ConcurrentDictionary<string, HashSet<string>> _botOwners = new();
    private static readonly ConcurrentDictionary<string, bool> _activeConnections = new();
    
    public GameHub(WorldSimulation world)
    {
        _world = world;
    }
    
    /// <summary>
    /// Triggered when connected to client
    /// </summary>
    /// <returns></returns>
    public override async Task OnConnectedAsync()
    {
        Console.WriteLine($"[Server] Client connected: {Context.ConnectionId}");
        
        _activeConnections.TryAdd(Context.ConnectionId, true);
        
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Triggered when disconnected from client
    /// </summary>
    /// <param name="exception"></param>
    /// <returns></returns>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        Console.WriteLine($"[Server] Client disconnected: {Context.ConnectionId}");

        _activeConnections.TryRemove(Context.ConnectionId, out _);
        
        _world.RemovePlayer(Context.ConnectionId);

        if (_botOwners.TryGetValue(Context.ConnectionId, out var botIds))
        {
            foreach (var botId in botIds)
            {
                _world.RemovePlayer(botId);
                Console.WriteLine($"[Server] Cleaned up bot: {botId}");
            }
        }
        
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Called by client to update player data.
    /// </summary>
    public async Task SyncPosition(byte[] data)
    {
        try
        {
            // update the client itself
            var state = PlayerState.Parser.ParseFrom(data);
            state.Id = Context.ConnectionId;
            await ProcessMovement(state);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error syncing position: {e.Message}");
        }
    }

    /// <summary>
    /// Called by client to update bot data.
    /// </summary>
    public async Task SyncBotPosition(IEnumerable<BotMoveData> bots)
    {
        try
        {
            var connId = Context.ConnectionId;
            
            _botOwners.AddOrUpdate(connId, _ => new HashSet<string>(bots.Select(b => b.Id)), 
                (_, set) =>
                {
                    foreach (var b in bots)
                    {
                        set.Add(b.Id);
                    }
                    return set;
                });
            
            var updatedPerRealObserver = new Dictionary<string, WorldState>();
            var serverTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            foreach (var bot in bots)
            {
                var pos = new Vec3 {X = bot.X, Y = bot.Y, Z = bot.Z};
                _world.MovePlayer(bot.Id, pos);
                
                var observers = _world.Grid.GetObservers(pos);

                var botState = new PlayerState()
                {
                    Id = bot.Id,
                    Position = pos,
                };
                
                foreach (var observer in observers)
                {
                    if (observer == connId || !_activeConnections.ContainsKey(observer)) // do not send to any robot or the python client itself
                    {
                        continue;
                    }

                    if (!updatedPerRealObserver.TryGetValue(observer, out var worldState))
                    {
                        worldState = new WorldState()
                        {
                            ServerTime = serverTime,
                        };
                        updatedPerRealObserver[observer] = worldState;
                    }
                    worldState.Players.Add(botState);
                }
            }

            var tasks = new List<Task>();
            foreach (var kv in updatedPerRealObserver)
            {
                var observerId = kv.Key;
                var worldState = kv.Value;
                tasks.Add(Clients.Client(observerId).SendAsync("ReceiveWorldState", worldState.ToByteArray()));
            }

            await Task.WhenAll(tasks);            
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error batch syncing: {e.Message}");
        }
    }

    private async Task ProcessMovement(PlayerState state)
    {
        _world.MovePlayer(state.Id, state.Position); // cannot use state.Id (from the user, not trustworthy)

        // broadcast to observers
        var nearbyIds = _world.Grid.GetObservers(state.Position);
            
        var responseForOthers = new WorldState();
        responseForOthers.ServerTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var responseForSelf = new WorldState();
        responseForSelf.ServerTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        
        foreach (var nearbyId in nearbyIds)
        {
            if (_world.Players.TryGetValue(nearbyId, out var playerState))
            {
                responseForOthers.Players.Add(playerState);

                if (nearbyId != state.Id)
                {
                    responseForSelf.Players.Add(playerState);
                }
            }
        }

        var tasks = new List<Task>();
        if (_activeConnections.ContainsKey(state.Id))
        {
            tasks.Add(Clients.Client(state.Id).SendAsync("ReceiveWorldState", responseForSelf.ToByteArray()));
        }
        
        var otherRealIds = nearbyIds
            .Where(id => _activeConnections.ContainsKey(id) && id != state.Id)
            .ToList();
        if (otherRealIds.Count > 0)
        {
            tasks.Add(Clients.Clients(otherRealIds).SendAsync("ReceiveWorldState", responseForOthers.ToByteArray()));
        }
        
        await Task.WhenAll(tasks);
    }
}