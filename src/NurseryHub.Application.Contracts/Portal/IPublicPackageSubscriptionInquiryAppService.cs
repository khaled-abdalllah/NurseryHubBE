using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace NurseryHub.Portal;

public interface IPublicPackageSubscriptionInquiryAppService : IApplicationService
{
    Task CreateAsync(CreatePackageSubscriptionInquiryDto input);
}
