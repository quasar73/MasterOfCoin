using AutoMapper;

namespace Lib.CrossService.Converters
{
    public class DateTimeUtcTicksConverter : ITypeConverter<DateTime, long>, ITypeConverter<long, DateTime>
    {
        public long Convert(DateTime source, long destination, ResolutionContext context)
        {
            return source.Ticks;
        }

        public DateTime Convert(long source, DateTime destination, ResolutionContext context)
        {
            return new DateTime(source, DateTimeKind.Utc);
        }
    }
}
