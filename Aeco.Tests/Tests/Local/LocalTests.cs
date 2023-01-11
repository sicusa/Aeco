namespace Aeco.Tests;

using System.Runtime.Serialization;

using Aeco.Local;

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

    public interface ICommand : IComponent
    {
    }

    public record struct TestCommand(string Name = "unknown") : ICommand
    {
    }

    public static void Run()
    {
        Console.WriteLine("== Local ==");

        var compositeLayer = new CompositeLayer(
            new MonoClosedHashStorage<TestComponent>(),
            new PolyClosedHashStorage<ITestComponentB>(),
            new ChannelLayer<ICommand>(),
            new PolyHashStorage()
        );

        var id = Guid.NewGuid();

        Console.WriteLine("[TestComponent1]");
        Console.WriteLine($"ContainsComponent<TestComponent>(): {compositeLayer.Contains<TestComponent>(id)}");
        compositeLayer.Acquire<TestComponent>(id);
        Console.WriteLine($"ContainsComponent<TestComponent>(): {compositeLayer.Contains<TestComponent>(id)}");
        compositeLayer.Remove<TestComponent>(id);
        Console.WriteLine($"ContainsComponent<TestComponent>(): {compositeLayer.Contains<TestComponent>(id)}");
        compositeLayer.Acquire<TestComponent>(id);
        Console.WriteLine($"ContainsComponent<TestComponent>(): {compositeLayer.Contains<TestComponent>(id)}");

        Console.WriteLine("\n[TestComponentB]");
        Console.WriteLine($"ContainsComponent<TestComponentB>(): {compositeLayer.Contains<TestComponentB>(id)}");
        compositeLayer.Acquire<TestComponentB>(id);
        Console.WriteLine($"ContainsComponent<TestComponentB>(): {compositeLayer.Contains<TestComponentB>(id)}");
        compositeLayer.Remove<TestComponentB>(id);
        Console.WriteLine($"ContainsComponent<TestComponentB>(): {compositeLayer.Contains<TestComponentB>(id)}");
        compositeLayer.Acquire<TestComponentB>(id);
        Console.WriteLine($"ContainsComponent<TestComponentB>(): {compositeLayer.Contains<TestComponentB>(id)}");

        Console.WriteLine("\n[TestComponentBB]");
        Console.WriteLine($"ContainsComponent<TestComponentBB>(): {compositeLayer.Contains<TestComponentBB>(id)}");
        compositeLayer.Acquire<TestComponentBB>(id);
        Console.WriteLine($"ContainsComponent<TestComponentB>(): {compositeLayer.Contains<TestComponentBB>(id)}");
        compositeLayer.Remove<TestComponentBB>(id);
        Console.WriteLine($"ContainsComponent<TestComponentB>(): {compositeLayer.Contains<TestComponentBB>(id)}");
        compositeLayer.Acquire<TestComponentBB>(id);
        Console.WriteLine($"ContainsComponent<TestComponentB>(): {compositeLayer.Contains<TestComponentBB>(id)}");

        Console.WriteLine($"ID: {id}");
        foreach (var component in compositeLayer.GetAll(id)){
            Console.WriteLine($"\tComponent: {component}");
        }

        var q = new Query<TestComponent, TestComponentB, TestComponentBB>();
        foreach (var foundId in q.Query(compositeLayer)) {
            Console.WriteLine(foundId);
        }



        Console.WriteLine("\n[TestCommand]");
        compositeLayer.Set(id, new TestCommand("Test"));
        compositeLayer.Set(id, new TestCommand("Test2"));
        compositeLayer.Set(id, new TestCommand("Test3"));
        compositeLayer.Set(id, new TestCommand("Test4"));

        while (compositeLayer.TryGet<TestCommand>(id, out var recCmd)) {
            Console.WriteLine($"TestCommand.Name: {recCmd.Name}");
            Console.WriteLine(compositeLayer.Remove<TestCommand>(id));
        }
    }
}