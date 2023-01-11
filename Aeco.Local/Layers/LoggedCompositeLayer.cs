namespace Aeco.Local;

using System.Diagnostics.CodeAnalysis;

public class LoggedCompositeLayer<TComponent, TSublayer> : CompositeLayer<TComponent, TSublayer>
    where TSublayer : ILayer<TComponent>
{
    public string? LogPrefix { get; set; }
    public bool IsReadLogEnabled { get; set; }

    public LoggedCompositeLayer(string logPrefix, params TSublayer[] sublayers)
        : base(sublayers)
    {
        LogPrefix = logPrefix;
    }

    public LoggedCompositeLayer(params TSublayer[] sublayers)
        : base(sublayers)
    {
    }

    protected virtual void Log(string message)
    {
        if (LogPrefix != null) {
            Console.WriteLine(LogPrefix + " " + message);
        }
        else {
            Console.WriteLine(message);
        }
    }

    public override bool TryGet<UComponent>(Guid id, [MaybeNullWhen(false)] out UComponent component)
    {
        var dataLayer = GetReadableDataLayer<UComponent>();
        bool success;
        if (dataLayer == null) {
            component = default;
            success = false;
        }
        else {
            success = dataLayer.TryGet<UComponent>(id, out component);
        }
        if (IsReadLogEnabled) {
            Log($"TryGet [{typeof(UComponent)}] {id} => {success} ({dataLayer})");
        }
        return success;
    }

    public override ref readonly UComponent Inspect<UComponent>(Guid id)
    {
        var dataLayer = RequireDataLayer<UComponent>();
        if (IsReadLogEnabled) {
            Log($"Inspect [{typeof(UComponent)}] {id} ({dataLayer})");
        }
        return ref dataLayer.Inspect<UComponent>(id);
    }

    public override ref UComponent Require<UComponent>(Guid id)
    {
        var dataLayer = RequireDataLayer<UComponent>();
        if (IsReadLogEnabled) {
            Log($"Require [{typeof(UComponent)}] {id} ({dataLayer})");
        }
        return ref dataLayer.Require<UComponent>(id);
    }

    public override ref UComponent Acquire<UComponent>(Guid id)
    {
        var dataLayer = RequireExpandableDataLayer<UComponent>();
        Log($"Acquire [{typeof(UComponent)}] {id} ({dataLayer})");
        return ref dataLayer.Acquire<UComponent>(id);
    }

    public override ref UComponent Acquire<UComponent>(Guid id, out bool exists)
    {
        var dataLayer = RequireExpandableDataLayer<UComponent>();
        ref UComponent success = ref dataLayer.Acquire<UComponent>(id, out exists);
        Log($"Acquire [{typeof(UComponent)}] {id} => exists: {exists}");
        return ref success;
    }

    public override bool Contains<UComponent>(Guid id)
    {
        var dataLayer = GetReadableDataLayer<UComponent>();
        bool success = dataLayer != null ? dataLayer.Contains<UComponent>(id) : false;
        if (IsReadLogEnabled) {
            Log($"Contains [{typeof(UComponent)}] {id} => {success} ({dataLayer})");
        }
        return success;
    }

    public override ref UComponent Set<UComponent>(Guid id, in UComponent component)
    {
        var dataLayer = RequireSettableDataLayer<UComponent>();
        Log($"Set [{typeof(UComponent)}] {id} ({dataLayer})");
        return ref dataLayer.Set<UComponent>(id, component);
    }

    public override bool Remove<UComponent>(Guid id)
    {
        var dataLayer = GetShrinkableDataLayer<UComponent>();
        bool success = dataLayer != null ? dataLayer.Remove<UComponent>(id) : false;
        Log($"Remove [{typeof(UComponent)}] {id} => {success} ({dataLayer})");
        return success;
    }

    public override bool Remove<UComponent>(Guid id, [MaybeNullWhen(false)] out UComponent component)
    {
        var dataLayer = GetShrinkableDataLayer<UComponent>();
        bool success;
        if (dataLayer == null) {
            component = default;
            success = false;
        }
        else {
            success = dataLayer.Remove<UComponent>(id, out component);
        }
        Log($"Remove [{typeof(UComponent)}] {id} => {success} ({dataLayer})");
        return success;
    }

    public override void Clear(Guid id)
    {
        Log($"Clear {id}");
        base.Clear(id);
    }

    public override void Clear()
    {
        Log($"Clear");
        base.Clear();
    }
}

public class LoggedCompositeLayer<TComponent> : LoggedCompositeLayer<TComponent, ILayer<TComponent>>
{
    public LoggedCompositeLayer(string logPrefix, params ILayer<TComponent>[] sublayers)
        : base(logPrefix, sublayers)
    {
    }

    public LoggedCompositeLayer(params ILayer<TComponent>[] sublayers)
        : base(sublayers)
    {
    }
}

public class LoggedCompositeLayer : LoggedCompositeLayer<IComponent>
{
    public LoggedCompositeLayer(string logPrefix, params ILayer<IComponent>[] sublayers)
        : base(logPrefix, sublayers)
    {
    }

    public LoggedCompositeLayer(params ILayer<IComponent>[] sublayers)
        : base(sublayers)
    {
    }
}