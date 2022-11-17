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

    public override bool TryGet<UComponent>(Guid entityId, [MaybeNullWhen(false)] out UComponent component)
    {
        var dataLayer = FindTerminalDataLayer<UComponent>();
        bool success;
        if (dataLayer == null) {
            component = default;
            success = false;
        }
        else {
            success = dataLayer.TryGet<UComponent>(entityId, out component);
        }
        if (IsReadLogEnabled) {
            Log($"TryGet [{typeof(UComponent)}] {entityId} => {success} ({dataLayer})");
        }
        return success;
    }

    public override ref readonly UComponent Inspect<UComponent>(Guid entityId)
    {
        var dataLayer = RequireTerminalDataLayer<UComponent>();
        if (IsReadLogEnabled) {
            Log($"Inspect [{typeof(UComponent)}] {entityId} ({dataLayer})");
        }
        return ref dataLayer.Inspect<UComponent>(entityId);
    }

    public override ref UComponent Require<UComponent>(Guid entityId)
    {
        var dataLayer = RequireTerminalDataLayer<UComponent>();
        if (IsReadLogEnabled) {
            Log($"Require [{typeof(UComponent)}] {entityId} ({dataLayer})");
        }
        return ref dataLayer.Require<UComponent>(entityId);
    }

    public override ref UComponent Acquire<UComponent>(Guid entityId)
    {
        var dataLayer = RequireTerminalDataLayer<UComponent>();
        Log($"Acquire [{typeof(UComponent)}] {entityId} ({dataLayer})");
        return ref dataLayer.Acquire<UComponent>(entityId);
    }

    public override ref UComponent Acquire<UComponent>(Guid entityId, out bool exists)
    {
        var dataLayer = RequireTerminalDataLayer<UComponent>();
        ref UComponent success = ref dataLayer.Acquire<UComponent>(entityId, out exists);
        Log($"Acquire [{typeof(UComponent)}] {entityId} => exists: {exists}");
        return ref success;
    }

    public override bool Contains<UComponent>(Guid entityId)
    {
        var dataLayer = FindTerminalDataLayer<UComponent>();
        bool success = dataLayer != null ? dataLayer.Contains<UComponent>(entityId) : false;
        if (IsReadLogEnabled) {
            Log($"Contains [{typeof(UComponent)}] {entityId} => {success} ({dataLayer})");
        }
        return success;
    }

    public override ref UComponent Set<UComponent>(Guid entityId, in UComponent component)
    {
        var dataLayer = RequireTerminalDataLayer<UComponent>();
        Log($"Set [{typeof(UComponent)}] {entityId} ({dataLayer})");
        return ref dataLayer.Set<UComponent>(entityId, component);
    }

    public override bool Remove<UComponent>(Guid entityId)
    {
        var dataLayer = FindTerminalDataLayer<UComponent>();
        bool success = dataLayer != null ? dataLayer.Remove<UComponent>(entityId) : false;
        Log($"Remove [{typeof(UComponent)}] {entityId} => {success} ({dataLayer})");
        return success;
    }

    public override bool Remove<UComponent>(Guid entityId, [MaybeNullWhen(false)] out UComponent component)
    {
        var dataLayer = FindTerminalDataLayer<UComponent>();
        bool success;
        if (dataLayer == null) {
            component = default;
            success = false;
        }
        else {
            success = dataLayer.Remove<UComponent>(entityId, out component);
        }
        Log($"Remove [{typeof(UComponent)}] {entityId} => {success} ({dataLayer})");
        return success;
    }

    public override void Clear(Guid entityId)
    {
        Log($"Clear {entityId}");
        base.Clear(entityId);
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