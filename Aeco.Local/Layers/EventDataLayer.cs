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

    public override ref UComponent Acquire<UComponent>(Guid entityId)
        => throw new NotSupportedException("Event component only supports Set method");

    public override ref UComponent Acquire<UComponent>(Guid entityId, out bool exists)
        => throw new NotSupportedException("Event component only supports Set method");

    public override ref UComponent Set<UComponent>(Guid entityId, in UComponent component)
    {
        var convertedSubject = _subject as Subject<UComponent>
            ?? throw new NotSupportedException("Event component not supported");
        convertedSubject.OnNext(component);
        return ref Unsafe.NullRef<UComponent>();
    }

    public override ref UComponent Require<UComponent>(Guid entityId)
        => throw new KeyNotFoundException("Component not found");

    public override bool Contains<UComponent>(Guid entityId)
        => false;

    public override bool Contains<UComponent>()
        => false;

    public override bool Remove<UComponent>(Guid entityId)
        => false;

    public override bool Remove<UComponent>(Guid entityId, [MaybeNullWhen(false)] out UComponent component)
    {
        component = default;
        return false;
    }

    public override Guid Singleton<UComponent>()
        => throw new KeyNotFoundException("Singleton not found");

    public override bool TryGet<UComponent>(Guid entityId, [MaybeNullWhen(false)] out UComponent component)
    {
        component = default;
        return false;
    }

    public override IEnumerable<object> GetAll(Guid entityId)
        => Enumerable.Empty<object>();

    public override IEnumerable<Guid> Query<UComponent>()
        => Enumerable.Empty<Guid>();
        
    public override IEnumerable<Guid> Query()
        => Enumerable.Empty<Guid>();

    public override void Clear(Guid entityId)
    {
    }

    public override void Clear()
    {
    }
}