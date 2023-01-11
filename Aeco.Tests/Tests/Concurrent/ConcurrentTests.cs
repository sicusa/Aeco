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

    public interface ICommand : IComponent
    {
    }

    public record struct EchoCmd(int Id) : ICommand
    {
    }

    public class TestConcurrentLayer : ITestLayer
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
            new ChannelLayer(),
            new PolyHashStorage(),
            new TestConcurrentLayer { ChannelId = channelId });
        var concurrentWorld = new ConcurrentDataLayer(world);

        var testLayers = world.GetSublayers<ITestLayer>().ToArray();
        
        world.Acquire<TestChannel>(channelId);

        new Thread(() => {
            int i = 0;
            while (true) {
                concurrentWorld.Set(channelId, new EchoCmd(++i));
                concurrentWorld.Set(channelId, new EchoCmd(1000 + i));
                Console.WriteLine("Sent: " + i);
                Thread.Sleep(1000);
            }
        }).Start();

        new Thread(() => {
            int i = 2000;
            while (true) {
                concurrentWorld.Set(channelId, new EchoCmd(++i));
                concurrentWorld.Set(channelId, new EchoCmd(1000 + i));
                Console.WriteLine("Sent: " + i);
                Thread.Sleep(500);
            }
        }).Start();

        while (true) {
            foreach (var layer in testLayers) {
                layer.Update(concurrentWorld);
            }
            Thread.Sleep(100);
        }
    }
}