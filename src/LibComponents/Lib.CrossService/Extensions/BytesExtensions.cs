namespace Lib.CrossService.Extensions
{
    internal static class BytesExtensions
    {
        public static byte[] ToBytes(this decimal dec)
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);

            writer.Write(dec);

            return stream.ToArray();
        }

        public static decimal ToDecimal(this byte[] bytes)
        {
            using var stream = new MemoryStream(bytes);
            using var reader = new BinaryReader(stream);

            return reader.ReadDecimal();
        }
    }
}
