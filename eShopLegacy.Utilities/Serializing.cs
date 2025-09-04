using System.IO;
using System.Text.Json;

namespace eShopLegacy.Utilities
{
    public class Serializing
    {
        public Stream SerializeBinary(object input)
        {
            var stream = new MemoryStream();
            // TODO: This is a proposed fix for SYSLIB0011. System.Text.Json does not support all types and may not be compatible with previous binary format.
            JsonSerializer.Serialize(stream, input);
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        public object DeserializeBinary(Stream stream)
        {
            // TODO: This is a proposed fix for SYSLIB0011. System.Text.Json does not support all types and may not be compatible with previous binary format.
            stream.Seek(0, SeekOrigin.Begin);
            return JsonSerializer.Deserialize<object>(stream);
        }
    }
}
