namespace Aeco.Serialization.Binary;

using System.IO;
using System.Runtime.Serialization;
using System.Collections.Generic;

public class BinaryEntitySerializer<TComponent> : IEntitySerializer<TComponent>
{
    private DataContractSerializer _serializer;

    public BinaryEntitySerializer()
    {
        _serializer = new DataContractSerializer(
            typeof(IEnumerable<TComponent>), SerializationUtil.KnownTypes);
    }

    public void Write(Stream stream, IReadableDataLayer<TComponent> dataLayer, Guid id)
    {
        _serializer.WriteObject(
            stream,
            dataLayer.GetAll(id).Where(p => {
                var type = p.GetType();
                return typeof(TComponent).IsAssignableFrom(type)
                    && Attribute.IsDefined(type, typeof(DataContractAttribute));
            }).Select(p => (TComponent)p));
    }

    public bool Read(Stream stream, ISettableDataLayer<TComponent> dataLayer, Guid id)
    {
        var components = (TComponent[]?)_serializer.ReadObject(stream);
        if (components == null) { return false; }

        SerializationUtil<TComponent>.Set(dataLayer, id, components);
        return true;
    }
}