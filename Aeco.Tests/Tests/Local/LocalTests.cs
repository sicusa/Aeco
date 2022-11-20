namespace Aeco.Tests;

using System.Runtime.Serialization;

using Aeco.Local;
using Aeco.Serialization.Json;
using Aeco.Persistence;
using Aeco.Persistence.Local;

public static class LocalTests
{
    [DataContract]
    public class TestComponent : IComponent
    {
        [DataMember]
        public int A { get; set; } = 5;
    }

    public interface ITestComponentB : IComponent
    {
    }

    [DataContract]
    public class TestComponentB : ITestComponentB
    {
        [DataMember]
        public int A { get; set; } = 5;
    }

    [DataContract]
    public class TestComponentBB : ITestComponentB
    {
        [DataMember]
        public int A { get; set; } = 5;
    }

    public record struct TestCommand(string Name = "unknown") : ICommand
    {
    }

    public static void Run()
    {
        Console.WriteLine("== Local ==");

        var entityFactory = new EntityFactory();

        var compositeLayer = new CompositeLayer(
            new MonoPoolStorage<TestComponent>(),
            new PolyPoolStorage<ITestComponentB>(),
            new ChannelLayer<ICommand>(),
            new PolyHashStorage(),
            new FileSystemPersistenceLayer(
                "./Entities", new JsonEntitySerializer<IComponent>())
        );
        compositeLayer.EntityFactory = entityFactory;

        var entity = compositeLayer.CreateEntity();
        entity.Acquire<Persistent>();
        Console.WriteLine(compositeLayer.Contains<Persistent>(entity.Id));

        Console.WriteLine($"ID: {entity.Id}");
        foreach (var component in compositeLayer.GetAll(entity.Id)){
            Console.WriteLine($"\tComponent: {component}");
        }

        Console.WriteLine("[TestComponent1]");
        Console.WriteLine($"ContainsComponent<TestComponent>(): {entity.Contains<TestComponent>()}");
        entity.Acquire<TestComponent>();
        Console.WriteLine($"ContainsComponent<TestComponent>(): {entity.Contains<TestComponent>()}");
        entity.Remove<TestComponent>();
        Console.WriteLine($"ContainsComponent<TestComponent>(): {entity.Contains<TestComponent>()}");
        entity.Acquire<TestComponent>();
        Console.WriteLine($"ContainsComponent<TestComponent>(): {entity.Contains<TestComponent>()}");

        Console.WriteLine("\n[TestComponentB]");
        Console.WriteLine($"ContainsComponent<TestComponentB>(): {entity.Contains<TestComponentB>()}");
        entity.Acquire<TestComponentB>();
        Console.WriteLine($"ContainsComponent<TestComponentB>(): {entity.Contains<TestComponentB>()}");
        entity.Remove<TestComponentB>();
        Console.WriteLine($"ContainsComponent<TestComponentB>(): {entity.Contains<TestComponentB>()}");
        entity.Acquire<TestComponentB>();
        Console.WriteLine($"ContainsComponent<TestComponentB>(): {entity.Contains<TestComponentB>()}");

        Console.WriteLine("\n[TestComponentBB]");
        Console.WriteLine($"ContainsComponent<TestComponentBB>(): {entity.Contains<TestComponentBB>()}");
        entity.Acquire<TestComponentBB>();
        Console.WriteLine($"ContainsComponent<TestComponentB>(): {entity.Contains<TestComponentBB>()}");
        entity.Remove<TestComponentBB>();
        Console.WriteLine($"ContainsComponent<TestComponentB>(): {entity.Contains<TestComponentBB>()}");
        entity.Acquire<TestComponentBB>();
        Console.WriteLine($"ContainsComponent<TestComponentB>(): {entity.Contains<TestComponentBB>()}");

        var q = new Query<Persistent, TestComponent, TestComponentB, TestComponentBB>();
        foreach (var id in q.Query(compositeLayer)) {
            Console.WriteLine(id);
        }

        Console.WriteLine("\n[TestCommand]");
        entity.Set(new TestCommand("Test"));
        entity.Set(new TestCommand("Test2"));
        entity.Set(new TestCommand("Test3"));
        entity.Set(new TestCommand("Test4"));

        while (entity.TryGet<TestCommand>(out var recCmd)) {
            Console.WriteLine($"TestCommand.Name: {recCmd.Name}");
            Console.WriteLine(entity.Remove<TestCommand>());
        }

        entity.Dispose();
    }
}