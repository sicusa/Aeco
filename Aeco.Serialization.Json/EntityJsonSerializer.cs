namespace Aeco.Serialization.Json;

using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Collections.Generic;

public class JsonEntitySerializer<TComponent> : IEntitySerializer<TComponent>
{
    private List<Type> _componentTypes;
    private DataContractJsonSerializer _serializer;

    private static readonly Type kComponentType = typeof(TComponent);
    private static readonly Type kDataContractType = typeof(DataContractAttribute);

    public JsonEntitySerializer()
    {
        _componentTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(t => kComponentType.IsAssignableFrom(t) && Attribute.IsDefined(t, kDataContractType))
            .ToList();

        _serializer = new DataContractJsonSerializer(
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
        try {
            var components = (TComponent[]?)_serializer.ReadObject(stream);
            if (components == null) { return false; }

            SerializationUtils<TComponent>.Set(dataLayer, entityId, components);
            return true;
        }
        catch {
            return false;
        }
    }
}