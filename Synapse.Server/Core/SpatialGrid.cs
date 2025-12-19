using System.Collections.Concurrent;
using Synapse.Shared.Protocol;

namespace Synapse.Server;

public class SpatialGrid
{
    private const int CellSize = 50;

    private readonly ConcurrentDictionary<(int, int), HashSet<string>> _grid = new();
    private readonly ConcurrentDictionary<string, (int, int)> _playerCells = new();

    private readonly object _lock = new object();

    private (int, int) GetCell(float x, float y)
    {
        return ((int)Math.Floor(x / CellSize), (int)Math.Floor(y / CellSize));
    }

    public void UpdateObject(string id, Vec3 pos)
    {
        var newCell = GetCell(pos.X, pos.Y);
        if (_playerCells.TryGetValue(id, out var oldCell) && oldCell == newCell)
        {
            return;
        }

        lock (_lock)
        {
            // remove from old set
            if (_grid.TryGetValue(oldCell, out var oldSet))
            {
                oldSet.Remove(id);
            }
            
            // add to new set
            var newSet = _grid.GetOrAdd(newCell, _ => new HashSet<string>());
            newSet.Add(id);

            // update player cell
            _playerCells[id] = newCell;
        }
    }

    public void RemoveObject(string id)
    {
        lock (_lock)
        {
            if (_playerCells.TryRemove(id, out var cell))
            {
                if (_grid.TryGetValue(cell, out var set))
                {
                    set.Remove(id);
                }
            }
        }
    }

    public List<string> GetObservers(Vec3 pos)
    {
        var centerCell = GetCell(pos.X, pos.Z);
        var result = new List<string>();

        lock (_lock)
        {
            for (var x = -1; x <= 1; ++x)
            {
                for (var z = -1; z <= 1; ++z)
                {
                    var cell = (centerCell.Item1 + x, centerCell.Item2 + z);
                    if (_grid.TryGetValue(cell, out var set))
                    {
                        result.AddRange(set);
                    }
                }
            }
        }
        
        return result;
    }
}