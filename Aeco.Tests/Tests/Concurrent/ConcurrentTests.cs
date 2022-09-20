namespace Aeco.Tests;

using System.Diagnostics.CodeAnalysis;

using Aeco.Local;
using Aeco.Concurrent;

public static class ConcurrentTests
{
    public interface ITestLayer : ILayer<object>
    {
        void Update(ConcurrentCompositeLayer layer);
    }

    public struct Channel : IComponent
    {
    }

    public struct EchoCmd : ICommand
    {
        public int Id { get; set; }
        public void Dispose() {}
    }

    public class TestConcurrentLayer : VirtualLayer, ITestLayer
    {
        public void Update(ConcurrentCompositeLayer world)
        {
            var channelId = world.Singleton<Channel>();
            while (world.TryGet<EchoCmd>(channelId, out var cmd)) {
                Console.WriteLine("Received: " + cmd.Id);
                world.Remove<EchoCmd>(channelId);
            }
        }
    }

    public static void Run()
    {
        var world = new ConcurrentCompositeLayer(
            new PooledChannelLayer(),
            new PolyHashStorage(),
            new TestConcurrentLayer());
        var testLayers = world.GetSublayers<ITestLayer>().ToArray();
        
        var channel = world.GetConcurrentEntity(Guid.NewGuid());
        channel.Acquire<Channel>();

        new Thread(() => {
            int i = 0;
            while (true) {
                channel.Acquire<EchoCmd>().Id = ++i;
                channel.Acquire<EchoCmd>().Id = 1000 + i;
                Console.WriteLine("Sent: " + i);
                Thread.Sleep(1000);
            }
        }).Start();

        new Thread(() => {
            int i = 2000;
            while (true) {
                channel.Acquire<EchoCmd>().Id = ++i;
                channel.Acquire<EchoCmd>().Id = 1000 + i;
                Console.WriteLine("Sent: " + i);
                Thread.Sleep(500);
            }
        }).Start();

        var lockSlim = world.LockSlim;
        while (true) {
            foreach (var layer in testLayers) {
                lockSlim.EnterWriteLock();
                try {
                    layer.Update(world);
                }
                finally {
                    lockSlim.ExitWriteLock();
                }
            }
            Thread.Sleep(100);
        }
    }
}