using Xunit;

namespace NurseryHub.EntityFrameworkCore;

[CollectionDefinition(NurseryHubTestConsts.CollectionDefinitionName)]
public class NurseryHubEntityFrameworkCoreCollection : ICollectionFixture<NurseryHubEntityFrameworkCoreFixture>
{

}
