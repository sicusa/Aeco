namespace Aeco.Tests;

using Aeco.Local;
using Aeco.Reactive;

public static class ReactiveTests
{
    public interface IReactiveComponent : IComponent
    {
    }

    public struct Position : IReactiveComponent
    {
        public int X;
        public int Y;
    }

    public struct Rotation : IReactiveComponent
    {
        public int Angle;
    }

    public static void Run()
    {
        Console.WriteLine("== Reactive ==");

        var eventStorage = new PolyHashStorage<IReactiveEvent>();
        var anyEventStorage = new PolyHashStorage<IAnyReactiveEvent>();

        var world = new CompositeLayer(
            eventStorage,
            anyEventStorage,

            new ReactiveCompositeLayer(
                new PolyClosedHashStorage<IReactiveComponent>(),
                new PolyHashStorage<IReactiveEvent<Position>>()) {
                EventDataLayer = eventStorage,
                AnyEventDataLayer = anyEventStorage
            });

        var id = Guid.NewGuid();
        world.Acquire<Position>(id);

        Console.WriteLine(world.Remove<Created<Position>>(id)); // true
        Console.WriteLine(world.Remove<Modified<Position>>(id)); // true

        world.Acquire<Position>(id).X = 10;
        Console.WriteLine(world.Remove<Created<Position>>(id)); // false
        Console.WriteLine(world.Remove<Modified<Position>>(id)); // true

        world.Acquire<Rotation>(id).Angle = 1;

        var anotherId = Guid.NewGuid();
        world.Acquire<Position>(anotherId);
        world.Acquire<Rotation>(anotherId);

        Group<Position, Rotation> group = new();
        foreach (var foundId in group.Query(world)) {
            Console.WriteLine(foundId);
        }
    }
}