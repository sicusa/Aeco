namespace Aeco.Renderer.GL;

using Aeco.Reactive;

public abstract class ObjectManagerBase<TObject, TObjectData>
    : VirtualLayer, IGLUpdateLayer, IGLLateUpdateLayer
    where TObject : IGLObject
    where TObjectData : IComponent, new()
{
    private Guid _libraryId = Guid.NewGuid();
    private Query<Modified<TObject>, TObject> _q = new();
    private Query<Destroy, TObject> _destroy_q = new();

    public void OnUpdate(IDataLayer<IComponent> context, float deltaTime)
    {
        foreach (var id in _q.Query(context)) {
            try {
                ref var obj = ref context.UnsafeInspect<TObject>(id);
                ref var data = ref context.Acquire<TObjectData>(id, out bool exists);

                if (exists) {
                    Initialize(context, id, ref obj, ref data, true);
                    Console.WriteLine($"{typeof(TObject)} [{id}] reinitialized.");
                }
                else {
                    Initialize(context, id, ref obj, ref data, false);
                    Console.WriteLine($"{typeof(TObject)} [{id}] initialized.");
                }

            }
            catch (Exception e) {
                Console.WriteLine($"Failed to initialize {typeof(TObject)} [{id}]: " + e);
            }
        }
    }

    public void OnLateUpdate(IDataLayer<IComponent> context, float deltaTime)
        => DoUninitialize(context, _destroy_q.Query(context));

    private void DoUninitialize(IDataLayer<IComponent> context, IEnumerable<Guid> ids)
    {
        foreach (var id in ids) {
            try {
                if (!context.Remove<TObject>(id, out var obj)) {
                    throw new KeyNotFoundException($"{typeof(TObject)} [{id}] does not have object component.");
                }
                if (!context.Remove<TObjectData>(id, out var data)) {
                    throw new KeyNotFoundException($"{typeof(TObject)} [{id}] does not have object data component.");
                }
                Uninitialize(context, id, in obj, in data);
                Console.WriteLine($"{typeof(TObject)} [{id}] uninitialized.");
            }
            catch (Exception e) {
                Console.WriteLine($"Failed to uninitialize {typeof(TObject)} [{id}]: " + e);
            }
        }
    }

    protected abstract void Initialize(
        IDataLayer<IComponent> context, Guid id, ref TObject obj, ref TObjectData data, bool updating);
    protected abstract void Uninitialize(
        IDataLayer<IComponent> context, Guid id, in TObject obj, in TObjectData data);
}