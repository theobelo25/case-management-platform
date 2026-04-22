using CaseManagement.Api.Tests.Infrastructure;

namespace CaseManagement.Api.Tests.Http;

[CollectionDefinition("HttpApi")]
public sealed class HttpApiCollection : ICollectionFixture<ApiHttpFixture>
{
}
