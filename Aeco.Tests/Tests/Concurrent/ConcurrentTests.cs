namespace Aeco.Tests;

using Aeco.Local;
using Aeco.Concurrent;

public static class ConcurrentTests
{
    public interface ITestLayer : ILayer<IComponent>
    {
        void Update(ConcurrentDataLayer layer);
    }

    public struct TestChannel : IComponent
    {
    }

    public record struct EchoCmd(int Id) : ICommand
    {
        public void Dispose()
        {
            Id = 0;
        }
    }

    public class TestConcurrentLayer : VirtualLayer, ITestLayer
    {
        public Guid ChannelId { get; init; }

        public void Update(ConcurrentDataLayer world)
        {
            while (world.Remove<EchoCmd>(ChannelId, out var cmd)) {
                Console.WriteLine("Received: " + cmd.Id);
            }
        }
    }

    public static void Run()
    {
        var channelId = Guid.NewGuid();

        var world = new CompositeLayer(
            new PooledChannelLayer(),
            new PolyHashStorage(),
            new TestConcurrentLayer { ChannelId = channelId });
        var worldConcurrent = new ConcurrentDataLayer(world);

        var testLayers = world.GetSublayers<ITestLayer>().ToArray();
        
        var channel = world.GetEntity(channelId);
        channel.Acquire<TestChannel>();

        new Thread(() => {
            int i = 0;
            while (true) {
                channel.Set(new EchoCmd(++i));
                channel.Set(new EchoCmd(1000 + i));
                Console.WriteLine("Sent: " + i);
                Thread.Sleep(1000);
            }
        }).Start();

        new Thread(() => {
            int i = 2000;
            while (true) {
                channel.Set(new EchoCmd(++i));
                channel.Set(new EchoCmd(1000 + i));
                Console.WriteLine("Sent: " + i);
                Thread.Sleep(500);
            }
        }).Start();

        while (true) {
            foreach (var layer in testLayers) {
                layer.Update(worldConcurrent);
            }
            Thread.Sleep(100);
        }
    }
}