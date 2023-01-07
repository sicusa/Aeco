namespace Aeco.Persistence.Local;

using System;
using System.IO;
using System.Collections.Generic;
using System.Reactive.Disposables;

using Aeco.Local;
using Aeco.Serialization;

public class ReadOnlyFileSystemPersistenceLayer<TComponent, TSelectedComponent>
    : VirtualLayer<TComponent>, IPersistenceLayer<TComponent, TSelectedComponent>
    , IParentLayerListener<TComponent, ITrackableDataLayer<TComponent>>
    where TSelectedComponent : TComponent
{
    public IReadOnlySet<Guid> SavedEntities => _savedEntities;

    public IDataLayer<Persistent>? PersistentDataLayer { get; }
    public IEntitySerializer<TSelectedComponent> Serializer { get; init; }

    public string DataDirectory { get; private set; }

    private DirectoryInfo? _dirInfo;
    private HashSet<Guid> _savedEntities = new();
    private Dictionary<ILayer<TComponent>, IDisposable> _subscriptions = new();

    public ReadOnlyFileSystemPersistenceLayer(
        string dataDirectory, IEntitySerializer<TSelectedComponent> serializer)
    {
        DataDirectory = dataDirectory;
        Serializer = serializer;
        UpdateSavedEntitiesSet();
    }

    public ReadOnlyFileSystemPersistenceLayer(
        IDataLayer<Persistent> persistentDataLayer, string dataDirectory, IEntitySerializer<TSelectedComponent> serializer)
        : this(dataDirectory, serializer)
    {
        PersistentDataLayer = persistentDataLayer;
    }

    protected virtual void UpdateSavedEntitiesSet()
    {
        _savedEntities.Clear();
        if (_dirInfo == null) {
            _dirInfo = new DirectoryInfo(DataDirectory);
        }
        foreach (var file in _dirInfo.GetFiles()) {
            if (Guid.TryParse(file.Name, out var id)) {
                _savedEntities.Add(id);
            }
        }
    }

    protected void RawAddSavedEntity(Guid id)
        => _savedEntities.Add(id);
    
    protected bool RawRemoveSavedEntity(Guid id)
        => _savedEntities.Remove(id);

    public virtual void OnLayerAdded(ITrackableDataLayer<TComponent> parent)
    {
        _subscriptions[parent] = CreateSubscription(parent);
    }

    protected IDataLayer<Persistent> GetPersistentDataLayer(ITrackableDataLayer<TComponent> parent)
        => PersistentDataLayer ?? parent as IDataLayer<Persistent>
            ?? throw new NotSupportedException("Persistent data layer not set and parent layer does not support Persistent component.");

    protected virtual IDisposable CreateSubscription(ITrackableDataLayer<TComponent> parent)
    {
        var persistentDataLayer = GetPersistentDataLayer(parent);
        return parent.EntityCreated.Subscribe(id => {
            if (!_savedEntities.Contains(id)) {
                return;
            }
            persistentDataLayer.Acquire<Persistent>(id);
            var stream = OpenReadStream(id);
            if (stream != null) {
                Serializer.Read(stream,
                    (IDataLayer<TSelectedComponent>)parent, id);
            }
        });
    }

    public virtual void OnLayerRemoved(ITrackableDataLayer<TComponent> parent)
    {
        if (_subscriptions.Remove(parent, out var sub)) {
            sub.Dispose();
        }
    }
    
    protected Stream? OpenReadStream(Guid id)
    {
        try {
            var path = Path.Combine(DataDirectory, id.ToString());
            return new FileStream(path, FileMode.Open);
        }
        catch (FileNotFoundException) {
            return null;
        }
    }
}

public class ReadOnlyFileSystemPersistenceLayer<TSelectedComponent>
    : ReadOnlyFileSystemPersistenceLayer<IComponent, TSelectedComponent>
    where TSelectedComponent : IComponent
{
    public ReadOnlyFileSystemPersistenceLayer(
        string dataDirectory, IEntitySerializer<TSelectedComponent> serializer)
        : base(dataDirectory, serializer)
    {
    }

    public ReadOnlyFileSystemPersistenceLayer(
        IDataLayer<Persistent> persistentDataLayer, string dataDirectory, IEntitySerializer<TSelectedComponent> serializer)
        : base(persistentDataLayer, dataDirectory, serializer)
    {
    }
}

public class ReadOnlyFileSystemPersistenceLayer : ReadOnlyFileSystemPersistenceLayer<IComponent>
{
    public ReadOnlyFileSystemPersistenceLayer(
        string dataDirectory, IEntitySerializer<IComponent> serializer)
        : base(dataDirectory, serializer)
    {
    }

    public ReadOnlyFileSystemPersistenceLayer(
        IDataLayer<Persistent> persistentDataLayer, string dataDirectory, IEntitySerializer<IComponent> serializer)
        : base(persistentDataLayer, dataDirectory, serializer)
    {
    }
}

public class FileSystemPersistenceLayer<TComponent, TSelectedComponent>
    : ReadOnlyFileSystemPersistenceLayer<TComponent, TSelectedComponent>
    where TSelectedComponent : TComponent
{
    public FileSystemPersistenceLayer(
        string dataDirectory, IEntitySerializer<TSelectedComponent> serializer)
        : base(dataDirectory, serializer)
    {
    }

    public FileSystemPersistenceLayer(
        IDataLayer<Persistent> persistentDataLayer, string dataDirectory, IEntitySerializer<TSelectedComponent> serializer)
        : base(persistentDataLayer, dataDirectory, serializer)
    {
    }

    protected override void UpdateSavedEntitiesSet()
    {
        if (!Directory.Exists(DataDirectory)) {
            Directory.CreateDirectory(DataDirectory);
        }
        base.UpdateSavedEntitiesSet();
    }

    protected override IDisposable CreateSubscription(ITrackableDataLayer<TComponent> parent)
    {
        var readSubscription = base.CreateSubscription(parent);
        var persistentDataLayer = GetPersistentDataLayer(parent);

        return new CompositeDisposable(
            readSubscription,
            parent.EntityDisposed.Subscribe(id => {
                if (!SavedEntities.Contains(id)) {
                    if (!persistentDataLayer.Contains<Persistent>(id)) {
                        return;
                    }
                    RawAddSavedEntity(id);
                }
                else if (!persistentDataLayer.Contains<Persistent>(id)) {
                    RemoveEntity(id);
                    return;
                }
                Serializer.Write(OpenWriteStream(id),
                    (IDataLayer<TSelectedComponent>)parent, id);
                parent.Clear(id);
            })
        );
    }

    public Stream OpenWriteStream(Guid id)
        => new FileStream(Path.Combine(DataDirectory, id.ToString()), FileMode.Create);
    
    public bool RemoveEntity(Guid id)
    {
        try {
            File.Delete(Path.Combine(DataDirectory, id.ToString()));
            RawRemoveSavedEntity(id);
            return true;
        }
        catch (DirectoryNotFoundException) {
            return false;
        }
    }
}

public class FileSystemPersistenceLayer<TSelectedComponent>
    : FileSystemPersistenceLayer<IComponent, TSelectedComponent>
    where TSelectedComponent : IComponent
{
    public FileSystemPersistenceLayer(
        string dataDirectory, IEntitySerializer<TSelectedComponent> serializer)
        : base(dataDirectory, serializer)
    {
    }

    public FileSystemPersistenceLayer(
        IDataLayer<Persistent> persistentDataLayer, string dataDirectory, IEntitySerializer<TSelectedComponent> serializer)
        : base(persistentDataLayer, dataDirectory, serializer)
    {
    }
}

public class FileSystemPersistenceLayer : FileSystemPersistenceLayer<IComponent>
{
    public FileSystemPersistenceLayer(
        string dataDirectory, IEntitySerializer<IComponent> serializer)
        : base(dataDirectory, serializer)
    {
    }

    public FileSystemPersistenceLayer(
        IDataLayer<Persistent> persistentDataLayer, string dataDirectory, IEntitySerializer<IComponent> serializer)
        : base(persistentDataLayer, dataDirectory, serializer)
    {
    }
}