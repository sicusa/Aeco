namespace Aeco.Serialization;

public interface IEntitySerializer<out TComponent>
{
    void Write(Stream stream, IReadableDataLayer<TComponent> dataLayer, uint id);
    bool Read(Stream stream, ISettableDataLayer<TComponent> dataLayer, uint id);
}