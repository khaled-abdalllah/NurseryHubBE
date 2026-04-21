using NurseryHub.Samples;
using Xunit;

namespace NurseryHub.EntityFrameworkCore.Domains;

[Collection(NurseryHubTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<NurseryHubEntityFrameworkCoreTestModule>
{

}
