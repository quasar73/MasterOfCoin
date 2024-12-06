using AutoMapper;
using Lib.CrossService.Models;

namespace Lib.CrossService.Extensions
{
    internal static class SourceTargetMapExtensions
    {
        public static MapperConfiguration ToMapperConfiguration(this IEnumerable<SourceTargetMap> maps)
        {
            if (maps == null)
            {
                throw new ArgumentNullException(nameof(maps));
            }

            var config = new MapperConfiguration(cfg =>
            {
                foreach (var type in maps.PopulateInnerTypes())
                {
                    if (type.Convertor == null)
                    {
                        cfg.CreateMap(type.SourceType, type.TargetType);
                    }
                    else
                    {
                        cfg.CreateMap(type.SourceType, type.TargetType).ConvertUsing(type.Convertor);
                    }
                }
            });

            config.AssertConfigurationIsValid();

            return config;
        }

        private static IEnumerable<SourceTargetMap> PopulateInnerTypes(this IEnumerable<SourceTargetMap> maps)
        {
            var hash = maps.Distinct().ToHashSet();

            hash
                .ToList()
                .ForEach(m => m.PopulateInnerTypes(hash));

            return hash;
        }

        private static void PopulateInnerTypes(this SourceTargetMap map, HashSet<SourceTargetMap> hash)
        {
            var targetMap = map.TargetType
                .GetProperties()
                .ToDictionary(p => p.Name);

            map.SourceType
                .GetProperties()
                .Select(p => new
                {
                    Source = p,
                    Target = targetMap.ContainsKey(p.Name) ? targetMap[p.Name] : null
                })
                .SelectMany(st => SourceTargetMap.From(st.Source, st.Target))
                .Where(m => m != null && hash.Add(m))
                .ToList()
                .ForEach(st => st!.PopulateInnerTypes(hash));
        }
    }
}
