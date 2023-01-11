namespace Aeco.Serialization;

public interface IEntitySerializer<out TComponent>
{
    void Write(Stream stream, IReadableDataLayer<TComponent> dataLayer, Guid id);
    bool Read(Stream stream, ISettableDataLayer<TComponent> dataLayer, Guid id);
}