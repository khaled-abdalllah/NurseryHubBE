using NurseryHub.Samples;
using Xunit;

namespace NurseryHub.EntityFrameworkCore.Applications;

[Collection(NurseryHubTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<NurseryHubEntityFrameworkCoreTestModule>
{

}
