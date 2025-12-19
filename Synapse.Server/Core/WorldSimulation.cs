using System.Collections.Concurrent;
using Synapse.Shared.Protocol;

namespace Synapse.Server;

public class WorldSimulation
{
    // all player data (PlayerState)
    public ConcurrentDictionary<string, PlayerState> Players { get; } = new();
    
    // grid data
    public SpatialGrid Grid { get; } = new SpatialGrid();

    public void MovePlayer(string connectionId, Vec3 pos)
    {
        var player = Players.GetOrAdd(connectionId, id => new PlayerState() { Id = id });
        player.Position = pos;
        player.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        
        Grid.UpdateObject(connectionId, pos);
    }

    public void RemovePlayer(string connectionId)
    {
        Players.TryRemove(connectionId, out _);
        Grid.RemoveObject(connectionId);
    }
}