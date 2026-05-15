using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using NurseryHub;
using Volo.Abp.Domain.Repositories;

namespace NurseryHub.Portal;

[AllowAnonymous]
public class PublicPackageSubscriptionInquiryAppService : NurseryHubAppService, IPublicPackageSubscriptionInquiryAppService
{
    private readonly IRepository<PackageSubscriptionInquiry, Guid> _repository;

    public PublicPackageSubscriptionInquiryAppService(IRepository<PackageSubscriptionInquiry, Guid> repository)
    {
        _repository = repository;
    }

    public virtual async Task CreateAsync(CreatePackageSubscriptionInquiryDto input)
    {
        var entity = new PackageSubscriptionInquiry(
            GuidGenerator.Create(),
            input.PackageTier,
            input.NurseryName.Trim(),
            input.ContactName.Trim(),
            input.PhoneNumber.Trim(),
            input.Email.Trim(),
            string.IsNullOrWhiteSpace(input.Message) ? null : input.Message.Trim());

        await _repository.InsertAsync(entity, autoSave: true);
    }
}
