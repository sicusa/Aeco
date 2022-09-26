namespace Aeco.Tests;

using Aeco.Local;
using Aeco.Reactive;

public static class ReactiveTests
{
    public interface IReactiveComponent : IComponent, IDisposable
    {
    }

    public struct Position : IReactiveComponent
    {
        public int X;
        public int Y;

        public void Dispose()
        {
            X = 0;
            Y = 0;
        }
    }

    public struct Rotation : IReactiveComponent
    {
        public int Angle;

        public void Dispose()
        {
            Angle = 0;
        }
    }

    public static void Run()
    {
        Console.WriteLine("== Reactive ==");

        var world = new ReactiveCompositeLayer(
            new PolyPoolStorage<IReactiveComponent>(),
            new PolyHashStorage<IReactiveEvent>());

        var entity = world.CreateEntity();
        entity.Acquire<Position>();
        entity.Acquire<Rotation>();

        Console.WriteLine(entity.Remove<Created<Position>>());
        Console.WriteLine(entity.Remove<Modified<Position>>());

        entity.Acquire<Position>().X = 10;
        Console.WriteLine(entity.Remove<Created<Position>>());
        Console.WriteLine(entity.Remove<Modified<Position>>());
    }
}