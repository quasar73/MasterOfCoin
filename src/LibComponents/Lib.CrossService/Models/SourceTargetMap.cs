using AutoMapper.Internal;
using Lib.CrossService.Converters;
using System.Reflection;
using GT = Google.Protobuf.WellKnownTypes;

namespace Lib.CrossService.Models
{
    internal class SourceTargetMap
    {
        public SourceTargetMap(Type source, Type target, Type? convertor = null)
        {
            SourceType = source;
            TargetType = target;
            Convertor = convertor;
        }

        public Type SourceType { get; set; }
        public Type TargetType { get; set; }
        public Type? Convertor { get; set; }

        public static SourceTargetMap From((Type First, Type Second) tuple)
            => new SourceTargetMap(tuple.First, tuple.Second);

        public static IEnumerable<SourceTargetMap?> From(PropertyInfo? source, PropertyInfo? target)
        {
            if (source is null || target is null)
                return Array.Empty<SourceTargetMap>();

            if (source.PropertyType.IsCollection() && target.PropertyType.IsCollection())
            {
                return source.PropertyType.GetGenericArguments()
                        .Zip(target.PropertyType.GetGenericArguments())
                        .Select(p => From(p.First, p.Second));
            }

            if (!source.CanWrite || !target.CanWrite)
                return Array.Empty<SourceTargetMap>();

            return new List<SourceTargetMap?> { From(source.PropertyType, target.PropertyType) };
        }

        private static SourceTargetMap? From(Type? source, Type? target)
        {
            if (source is null || target is null)
                return null;

            //AutoMapper can take care of strings better
            if (source == typeof(string) || target == typeof(string))
                return null;

            if (source == target)
                return null;

            return new SourceTargetMap(source, target);
        }

        public static List<SourceTargetMap> GetDefaultGrpcToStandartMap()
        {
            return new List<SourceTargetMap>
            {
                new (typeof(long), typeof(DateTime), typeof(DateTimeUtcTicksConverter)),
                new (typeof(long), typeof(TimeSpan), typeof(TimeSpanTicksConverter)),
                new (typeof(GT.StringValue), typeof(string), typeof(StringValueStringConverter)),
                new (typeof(GT.Any), typeof(decimal?), typeof(DecimalValueAnyConverter)),
                new (typeof(GT.Any), typeof(decimal), typeof(DecimalAnyConverter))
            };
        }

        public static List<SourceTargetMap> GetDefaultStandartToGrpcMap()
        {
            return new List<SourceTargetMap>
            {
                new(typeof(DateTime), typeof(long), typeof(DateTimeUtcTicksConverter)),
                new(typeof(TimeSpan), typeof(long), typeof(TimeSpanTicksConverter)),
                new(typeof(decimal?), typeof(GT.Any), typeof(DecimalValueAnyConverter)),
                new(typeof(decimal), typeof(GT.Any), typeof(DecimalAnyConverter)),
                new(typeof(string), typeof(GT.StringValue), typeof(StringValueStringConverter)),
                new(typeof(Task), typeof(Task<GT.Empty>), typeof(TaskToTaskEmptyConverter))
            };
        }

        public override bool Equals(object? obj)
        {
            if (obj is SourceTargetMap stType)
            {
                return SourceType == stType.SourceType
                    && TargetType == stType.TargetType;
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
            => HashCode.Combine(SourceType, TargetType);

        public override string ToString()
        {
            return Convertor == null ?
                $"{SourceType.Name} to {TargetType.Name}" :
                $"{SourceType.Name} to {TargetType.Name} using {Convertor.Name}";
        }
    }
}
