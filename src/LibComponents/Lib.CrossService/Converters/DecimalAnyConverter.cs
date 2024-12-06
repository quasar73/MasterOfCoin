using AutoMapper;
using Lib.CrossService.Extensions;
using Google.Protobuf;
using GT = Google.Protobuf.WellKnownTypes;

namespace Lib.CrossService.Converters
{
    public class DecimalAnyConverter : ITypeConverter<decimal, GT.Any>, ITypeConverter<GT.Any, decimal>
    {
        private const string DecimalTypeUrl = "decimal";

        public GT.Any Convert(decimal source, GT.Any destination, ResolutionContext context)
        {
            return new GT.Any
            {
                TypeUrl = DecimalTypeUrl,
                Value = ByteString.CopyFrom(source.ToBytes())
            };
        }

        public decimal Convert(GT.Any source, decimal destination, ResolutionContext context)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (source.TypeUrl != DecimalTypeUrl)
            {
                throw new ArgumentException($"Invalid message type_url '{source.TypeUrl}'");
            }

            return source.Value.ToByteArray().ToDecimal();
        }
    }
}
