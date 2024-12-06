using AutoMapper;
using Lib.CrossService.Extensions;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using GT = Google.Protobuf.WellKnownTypes;

namespace Lib.CrossService.Converters
{
    public class DecimalValueAnyConverter : ITypeConverter<decimal?, GT.Any>, ITypeConverter<GT.Any, decimal?>
    {
        private const string NullabelDecimalTypeUrl = "decimal";

        public GT.Any Convert(decimal? source, GT.Any destination, ResolutionContext context)
        {
            return new GT.Any
            {
                TypeUrl = NullabelDecimalTypeUrl,
                Value = source != null ? ByteString.CopyFrom(source.Value.ToBytes()) : ByteString.Empty
            };
        }

        public decimal? Convert(GT.Any source, decimal? destination, ResolutionContext context)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (source.TypeUrl != NullabelDecimalTypeUrl)
            {
                throw new ArgumentException($"Invalid message type_url '{source.TypeUrl}'");
            }

            var decimalBytes = source.Value.ToByteArray();

            return decimalBytes.Count() > 0 ? decimalBytes.ToDecimal() : null;
        }
    }
}
