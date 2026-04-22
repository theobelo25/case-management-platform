namespace CaseManagement.Api.Exceptions.Mapping;

public interface IExceptionToProblemDetailsMapperRegistry
{
    /// <summary>
    /// Returns the mapper for the most specific type in <paramref name="exception"/>'s inheritance chain, if any.
    /// </summary>
    IExceptionToProblemDetailsMapper? GetMapperFor(Exception exception);
}
