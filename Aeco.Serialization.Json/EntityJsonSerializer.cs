namespace Aeco.Serialization.Json;

using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Collections.Generic;

public class JsonEntitySerializer<TComponent> : IEntitySerializer<TComponent>
{
    private DataContractJsonSerializer _serializer;

    public JsonEntitySerializer()
    {
        _serializer = new DataContractJsonSerializer(
            typeof(IEnumerable<TComponent>), SerializationUtil.KnownTypes);
    }

    public void Write(Stream stream, IDataLayer<TComponent> dataLayer, Guid entityId)
    {
        _serializer.WriteObject(
            stream,
            dataLayer.GetAll(entityId).Where(p => {
                var type = p.GetType();
                return typeof(TComponent).IsAssignableFrom(type)
                    && Attribute.IsDefined(type, typeof(DataContractAttribute));
            }).Select(p => (TComponent)p));
    }

    public bool Read(Stream stream, IDataLayer<TComponent> dataLayer, Guid entityId)
    {
        try {
            var components = (TComponent[]?)_serializer.ReadObject(stream);
            if (components == null) { return false; }

            SerializationUtil<TComponent>.Set(dataLayer, entityId, components);
            return true;
        }
        catch {
            return false;
        }
    }
}