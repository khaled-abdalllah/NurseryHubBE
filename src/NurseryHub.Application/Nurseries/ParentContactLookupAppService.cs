using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using NurseryHub.Security;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;

namespace NurseryHub.Nurseries;

/// <summary>
/// HTTP route is implemented explicitly in <c>ParentContactLookupController</c> so the URL is stable.
/// </summary>
[RemoteService(IsEnabled = false)]
[Authorize(Roles = $"{NurseryHubRoles.Admin},{NurseryHubRoles.NurseryAdmin}")]
public class ParentContactLookupAppService : ApplicationService, IParentContactLookupAppService
{
    private readonly IRepository<ParentContact, Guid> _parentContactRepository;
    private readonly IIdentityUserRepository _identityUserRepository;
    private readonly IdentityUserManager _identityUserManager;

    public ParentContactLookupAppService(
        IRepository<ParentContact, Guid> parentContactRepository,
        IIdentityUserRepository identityUserRepository,
        IdentityUserManager identityUserManager)
    {
        _parentContactRepository = parentContactRepository;
        _identityUserRepository = identityUserRepository;
        _identityUserManager = identityUserManager;
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

        var rows = await AsyncExecuter.ToListAsync(projected);
        await EnrichHasParentPortalAccountAsync(rows);
        return rows;
    }

    private async Task EnrichHasParentPortalAccountAsync(List<ParentContactLookupDto> rows)
    {
        if (rows.Count == 0)
        {
            return;
        }

        var phones = new HashSet<string>(StringComparer.Ordinal);
        foreach (var r in rows)
        {
            if (!r.FatherPhoneNumber.IsNullOrWhiteSpace())
            {
                phones.Add(r.FatherPhoneNumber.Trim());
            }

            if (!r.MotherPhoneNumber.IsNullOrWhiteSpace())
            {
                phones.Add(r.MotherPhoneNumber.Trim());
            }
        }

        var normalizedUserNames = phones
            .Select(p => _identityUserManager.NormalizeName(p))
            .Where(n => !n.IsNullOrWhiteSpace())
            .Distinct()
            .ToList();

        if (normalizedUserNames.Count == 0)
        {
            return;
        }

        var existingSet = new HashSet<string>(StringComparer.Ordinal);
        foreach (var norm in normalizedUserNames)
        {
            var user = await _identityUserRepository.FindByNormalizedUserNameAsync(norm);
            if (user == null || !TenantMatchesCurrentUserTenant(user.TenantId))
            {
                continue;
            }

            existingSet.Add(norm);
        }

        foreach (var r in rows)
        {
            var fatherN = r.FatherPhoneNumber.IsNullOrWhiteSpace()
                ? null
                : _identityUserManager.NormalizeName(r.FatherPhoneNumber.Trim());
            var motherN = r.MotherPhoneNumber.IsNullOrWhiteSpace()
                ? null
                : _identityUserManager.NormalizeName(r.MotherPhoneNumber.Trim());

            r.HasParentPortalAccount =
                (fatherN != null && existingSet.Contains(fatherN)) ||
                (motherN != null && existingSet.Contains(motherN));
        }
    }

    private bool TenantMatchesCurrentUserTenant(Guid? userTenantId)
    {
        if (CurrentTenant.Id.HasValue)
        {
            return userTenantId == CurrentTenant.Id;
        }

        return !userTenantId.HasValue;
    }
}
