namespace Aeco.Tests.RPGGame;

using Aeco.Local;
using Aeco.Reactive;

public class Config
{
    public int Seed { get; set; }

    public IDataLayer<IComponent> EventDataLayer { get; set; }
        = new PolyHashStorage<IReactiveEvent>();
}