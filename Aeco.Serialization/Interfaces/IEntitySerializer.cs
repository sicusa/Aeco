namespace Aeco.Serialization;

public interface IEntitySerializer<out TComponent>
{
    void Write(Stream stream, IDataLayer<TComponent> dataLayer, Guid id);
    bool Read(Stream stream, IDataLayer<TComponent> dataLayer, Guid id);
}