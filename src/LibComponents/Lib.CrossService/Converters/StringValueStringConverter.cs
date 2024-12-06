using AutoMapper;
using GT = Google.Protobuf.WellKnownTypes;

namespace Lib.CrossService.Converters
{
    public class StringValueStringConverter : ITypeConverter<string, GT.StringValue>, ITypeConverter<GT.StringValue, string>
    {
        public GT.StringValue Convert(string source, GT.StringValue destination, ResolutionContext context)
        {
            return source == null
                ? new GT.StringValue()
                : new GT.StringValue { Value = source };
        }

        public string Convert(GT.StringValue source, string destination, ResolutionContext context)
        {
            return source?.Value!;
        }
    }
}
