using AutoMapper;

namespace Lib.CrossService.Converters;

public class TimeSpanTicksConverter : ITypeConverter<TimeSpan, long>, ITypeConverter<long, TimeSpan>
{
    public long Convert(TimeSpan source, long destination, ResolutionContext context)
    {
        return source.Ticks;
    }

    public TimeSpan Convert(long source, TimeSpan destination, ResolutionContext context)
    {
        return TimeSpan.FromTicks(source);
    }
}
