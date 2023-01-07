namespace Aeco.Local;

using System.Reactive.Subjects;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

public class EventDataLayer<TComponent, TSelectedComponent>
    : LocalDataLayerBase<TComponent, TSelectedComponent>, IObservable<TSelectedComponent>
    where TSelectedComponent : TComponent
{
    private Subject<TSelectedComponent> _subject = new();

    public IDisposable Subscribe(IObserver<TSelectedComponent> observer)
        => _subject.Subscribe(observer);

    public override ref UComponent Acquire<UComponent>(Guid id)
        => throw new NotSupportedException("Event component only supports Set method");

    public override ref UComponent Acquire<UComponent>(Guid id, out bool exists)
        => throw new NotSupportedException("Event component only supports Set method");

    public override ref UComponent Set<UComponent>(Guid id, in UComponent component)
    {
        var convertedSubject = _subject as Subject<UComponent>
            ?? throw new NotSupportedException("Event component not supported");
        convertedSubject.OnNext(component);
        return ref Unsafe.NullRef<UComponent>();
    }

    public override ref UComponent Require<UComponent>(Guid id)
        => throw new KeyNotFoundException("Component not found");

    public override bool Contains<UComponent>(Guid id)
        => false;

    public override bool ContainsAny<UComponent>()
        => false;

    public override bool Remove<UComponent>(Guid id)
        => false;

    public override bool Remove<UComponent>(Guid id, [MaybeNullWhen(false)] out UComponent component)
    {
        component = default;
        return false;
    }

    public override void RemoveAll<UComponent>()
    {
    }

    public override Guid? Singleton<UComponent>()
        => null;

    public override bool TryGet<UComponent>(Guid id, [MaybeNullWhen(false)] out UComponent component)
    {
        component = default;
        return false;
    }

    public override IEnumerable<object> GetAll(Guid id)
        => Enumerable.Empty<object>();

    public override IEnumerable<Guid> Query<UComponent>()
        => Enumerable.Empty<Guid>();
        
    public override IEnumerable<Guid> Query()
        => Enumerable.Empty<Guid>();

    public override int GetCount()
        => 0;

    public override int GetCount<UComponent>()
        => 0;

    public override void Clear(Guid id)
    {
    }

    public override void Clear()
    {
    }
}