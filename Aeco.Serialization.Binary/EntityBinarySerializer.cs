namespace Aeco.Serialization.Binary;

using System.IO;
using System.Runtime.Serialization;
using System.Collections.Generic;

public class BinaryEntitySerializer<TComponent> : IEntitySerializer<TComponent>
{
    private List<Type> _componentTypes;
    private DataContractSerializer _serializer;

    private static readonly Type kComponentType = typeof(TComponent);
    private static readonly Type kDataContractType = typeof(DataContractAttribute);

    public BinaryEntitySerializer()
    {
        _componentTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => kComponentType.IsAssignableFrom(p) && Attribute.IsDefined(p, kDataContractType))
            .ToList();

        _serializer = new DataContractSerializer(
            typeof(IEnumerable<TComponent>), _componentTypes);
    }

    public void Write(Stream stream, IDataLayer<TComponent> dataLayer, Guid entityId)
    {
        _serializer.WriteObject(
            stream,
            dataLayer.GetAll(entityId).Where(p => {
                var type = p.GetType();
                return kComponentType.IsAssignableFrom(type)
                    && Attribute.IsDefined(type, kDataContractType);
            }).Select(p => (TComponent)p));
    }

    public bool Read(Stream stream, IDataLayer<TComponent> dataLayer, Guid entityId)
    {
        var components = (TComponent[]?)_serializer.ReadObject(stream);
        if (components == null) { return false; }

        SerializationUtils<TComponent>.Set(dataLayer, entityId, components);
        return true;
    }
}