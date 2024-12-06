using AutoMapper;
using GT = Google.Protobuf.WellKnownTypes;

namespace Lib.CrossService.Converters
{
    public class TaskToTaskEmptyConverter : ITypeConverter<Task, Task<GT.Empty>>
    {
        public Task<GT.Empty> Convert(Task source, Task<GT.Empty> destination, ResolutionContext context)
        {
            return source
                .ContinueWith(t => Task.FromResult(new GT.Empty()))
                .Unwrap();
        }
    }
}
