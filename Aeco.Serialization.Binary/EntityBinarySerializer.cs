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
            typeof(IEnumerable<TComponent>), SerializationUtils.KnownTypes);
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
        var components = (TComponent[]?)_serializer.ReadObject(stream);
        if (components == null) { return false; }

        SerializationUtils<TComponent>.Set(dataLayer, entityId, components);
        return true;
    }
}