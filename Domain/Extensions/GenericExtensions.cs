using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Domain.Extensions
{
    public static class GenericExtensions
    {
        public static byte[] ToByteArray<T>(this T item)
        {
            if (item == null)
            {
                return null;
            }

            var formatter = new BinaryFormatter();
            using (var stream = new MemoryStream())
            {
                formatter.Serialize(stream, item);
                return stream.ToArray();
            }
        }

        public static T FromByteArray<T>(this byte[] bytes)
        {
            if (bytes == null)
            {
                return default(T);
            }

            var formatter = new BinaryFormatter();
            using (var stream = new MemoryStream())
            {
                stream.Write(bytes, 0, bytes.Length);
                stream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(stream);
            }
        }
    }
}
