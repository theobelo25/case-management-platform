using System.Collections.ObjectModel;

namespace CaseManagement.Api.Exceptions.Mapping;

public sealed class ExceptionToProblemDetailsMapperRegistry : IExceptionToProblemDetailsMapperRegistry
{
    private readonly IReadOnlyDictionary<Type, IExceptionToProblemDetailsMapper> _mappersByExactType;

    public ExceptionToProblemDetailsMapperRegistry(IEnumerable<IExceptionToProblemDetailsMapper> mappers)
    {
        var map = new Dictionary<Type, IExceptionToProblemDetailsMapper>();
        foreach (var mapper in mappers)
        {
            if (!map.TryAdd(mapper.ExceptionType, mapper))
                throw new InvalidOperationException(
                    $"Duplicate {nameof(IExceptionToProblemDetailsMapper)} registration for {mapper.ExceptionType.FullName}.");
        }

        _mappersByExactType = new ReadOnlyDictionary<Type, IExceptionToProblemDetailsMapper>(map);
    }

    public IExceptionToProblemDetailsMapper? GetMapperFor(Exception exception)
    {
        for (var type = exception.GetType();
             type is not null && type != typeof(object);
             type = type.BaseType)
        {
            if (_mappersByExactType.TryGetValue(type, out var mapper))
                return mapper;
        }

        return null;
    }
}
