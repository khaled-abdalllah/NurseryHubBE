using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using NurseryHub.Security;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace NurseryHub.Nurseries;

/// <summary>
/// HTTP route is implemented explicitly in <c>ParentContactLookupController</c> so the URL is stable.
/// </summary>
[RemoteService(IsEnabled = false)]
[Authorize(Roles = $"{NurseryHubRoles.Admin},{NurseryHubRoles.NurseryAdmin}")]
public class ParentContactLookupAppService : ApplicationService, IParentContactLookupAppService
{
    private readonly IRepository<ParentContact, Guid> _parentContactRepository;

    public ParentContactLookupAppService(IRepository<ParentContact, Guid> parentContactRepository)
    {
        _parentContactRepository = parentContactRepository;
    }

    public virtual async Task<List<ParentContactLookupDto>> FindParentContactsByPhoneAsync(string phoneNumber)
    {
        if (phoneNumber.IsNullOrWhiteSpace())
        {
            return new List<ParentContactLookupDto>();
        }

        var normalized = phoneNumber.Trim();
        var query = await _parentContactRepository.GetQueryableAsync();
        query = query.Where(p =>
            p.FatherPhoneNumber == normalized || p.MotherPhoneNumber == normalized);

        if (CurrentTenant.Id.HasValue)
        {
            query = query.Where(p => p.TenantId == CurrentTenant.Id);
        }
        else
        {
            query = query.Where(p => p.TenantId == null);
        }

        var projected = query
            .OrderBy(p => p.FatherName)
            .ThenBy(p => p.Id)
            .Take(20)
            .Select(p => new ParentContactLookupDto
            {
                Id = p.Id,
                FatherName = p.FatherName,
                FatherIdentityNumber = p.FatherIdentityNumber,
                FatherPhoneNumber = p.FatherPhoneNumber,
                MotherName = p.MotherName,
                MotherIdentityNumber = p.MotherIdentityNumber,
                MotherPhoneNumber = p.MotherPhoneNumber,
            });

        return await AsyncExecuter.ToListAsync(projected);
    }
}
