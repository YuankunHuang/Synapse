using Google.Protobuf;
using Microsoft.AspNetCore.SignalR;
using Synapse.Shared.Protocol;

namespace Synapse.Server.Hubs;

public class GameHub : Hub
{
    private WorldSimulation _world;
    
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

        _world.MovePlayer(Context.ConnectionId, new Vec3 { X = 0, Y = 0, Z = 0 });

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
        
        _world.RemovePlayer(Context.ConnectionId);
        
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Called by client to update player data.
    /// </summary>
    /// <param name="data"></param>
    public async Task SyncPosition(byte[] data)
    {
        try
        {
            // update the client itself
            var state = PlayerState.Parser.ParseFrom(data);
            var connectionId = Context.ConnectionId;
            state.Id = connectionId;
            _world.MovePlayer(connectionId, state.Position); // cannot use state.Id (from the user, not trustworthy)

            // broadcast to observers
            var nearbyIds = _world.Grid.GetObservers(state.Position);
            
            // responses (to all observers)
            var response = new WorldState();
            response.ServerTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            foreach (var nearbyId in nearbyIds)
            {
                if (_world.Players.TryGetValue(nearbyId, out var playerState))
                {
                    response.Players.Add(playerState);
                }
            }

            var responseBytes = response.ToByteArray();
            await Clients.Clients(nearbyIds).SendAsync("ReceiveWorldState", responseBytes);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error syncing position: {e.Message}");
        }
    }
}