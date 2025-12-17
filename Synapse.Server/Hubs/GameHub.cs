using Google.Protobuf;
using Microsoft.AspNetCore.SignalR;
using Synapse.Shared.Protocol;

namespace Synapse.Server.Hubs;

public class GameHub : Hub
{
    /// <summary>
    /// Triggered when connected to client
    /// </summary>
    /// <returns></returns>
    public override async Task OnConnectedAsync()
    {
        Console.WriteLine($"[Server] Client connected: {Context.ConnectionId}");

        var mockState = new WorldState()
        {
            ServerTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };
        mockState.Players.Add(new PlayerState() {Id = "Bot1", Position = new Vec3() {X = 1, Y = 0, Z = 0}});
        mockState.Players.Add(new PlayerState() {Id = "Bot2", Position = new Vec3() {X = 2, Y = 0, Z = 1}});
        await Clients.Caller.SendAsync("ReceiveMessage", mockState.ToByteArray());
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
        await base.OnDisconnectedAsync(exception);
    }
}