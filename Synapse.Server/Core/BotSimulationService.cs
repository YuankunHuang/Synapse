using System.Diagnostics;
using Synapse.Shared.Protocol;

namespace Synapse.Server;

public class BotSimulationService : BackgroundService
{
    private readonly WorldSimulation _world;
    private readonly int _botCount = 1000;
    private readonly int _botAreaSize = 200;
    private readonly List<BotAgent> _bots = new();

    public BotSimulationService(WorldSimulation world)
    {
        _world = world;

        // initialize bots
        for (var i = 0; i < _botCount; ++i)
        {
            _bots.Add(new BotAgent()
            {
                Id = $"bot_{i + 1}",
                X = Random.Shared.NextSingle() * _botAreaSize,
                Z = Random.Shared.NextSingle() * _botAreaSize,
                Angle = Random.Shared.NextSingle() * MathF.PI * 2,
            });
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine($"[BotService] Starting simulation for {_botCount} bots.");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            foreach (var bot in _bots)
            {
                bot.Angle += 0.1f;
                bot.X += MathF.Cos(bot.Angle) * .5f;
                bot.Z += MathF.Sin(bot.Angle) * .5f;
                if (bot.X < -50 || bot.X > 50)
                    bot.Angle += MathF.PI;
                if (bot.Z < -50 || bot.Z > 50)
                    bot.Angle += MathF.PI;
                _world.MovePlayer(bot.Id, new Vec3()
                {
                    X = bot.X,
                    Y = 0,
                    Z = bot.Z,
                });
            }

            await Task.Delay(100, stoppingToken);
        }
    }
}

public class BotAgent
{
    public required string Id { get; init; }
    public float X { get; set; }
    public float Z { get; set; }
    public float Angle { get; set; }
}