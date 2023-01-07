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

    public void Write(Stream stream, IDataLayer<TComponent> dataLayer, Guid id)
    {
        _serializer.WriteObject(
            stream,
            dataLayer.GetAll(id).Where(p => {
                var type = p.GetType();
                return typeof(TComponent).IsAssignableFrom(type)
                    && Attribute.IsDefined(type, typeof(DataContractAttribute));
            }).Select(p => (TComponent)p));
    }

    public bool Read(Stream stream, IDataLayer<TComponent> dataLayer, Guid id)
    {
        try {
            var components = (TComponent[]?)_serializer.ReadObject(stream);
            if (components == null) { return false; }

            SerializationUtil<TComponent>.Set(dataLayer, id, components);
            return true;
        }
        catch {
            return false;
        }
    }
}