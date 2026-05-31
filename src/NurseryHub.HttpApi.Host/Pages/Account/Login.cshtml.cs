using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Volo.Abp.Account.Web;
using Volo.Abp.Account.Web.Pages.Account;
using Volo.Abp.Identity;
using Volo.Abp.TenantManagement;
using IdentityUser = Volo.Abp.Identity.IdentityUser;

namespace NurseryHub.Pages.Account;

public class NurseryHubLoginModel : LoginModel
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IConfiguration _configuration;

    public string HomePageUrl =>
        (_configuration["App:AngularUrl"] ?? "/").TrimEnd('/');

    public NurseryHubLoginModel(
        IAuthenticationSchemeProvider schemeProvider,
        IOptions<AbpAccountOptions> accountOptions,
        IOptions<IdentityOptions> identityOptions,
        IdentityDynamicClaimsPrincipalContributorCache contributorCache,
        IWebHostEnvironment webHostEnvironment,
        ITenantRepository tenantRepository,
        IConfiguration configuration)
        : base(schemeProvider, accountOptions, identityOptions, contributorCache, webHostEnvironment)
    {
        _tenantRepository = tenantRepository;
        _configuration = configuration;
    }

    public override async Task<IActionResult> OnPostAsync(string action)
    {
        if (string.Equals(action, "Login", StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrWhiteSpace(LoginInput?.UserNameOrEmailAddress))
        {
            var user = await FindUserAsync(LoginInput.UserNameOrEmailAddress);
            using (CurrentTenant.Change(user?.TenantId))
            {
                return await base.OnPostAsync(action);
            }
        }

        return await base.OnPostAsync(action);
    }

    protected virtual async Task<IdentityUser?> FindUserAsync(string userNameOrEmailAddress)
    {
        IdentityUser? user;

        using (CurrentTenant.Change(null))
        {
            user = await FindUserInCurrentTenantAsync(userNameOrEmailAddress);
            if (user != null)
            {
                return user;
            }
        }

        foreach (var tenant in await _tenantRepository.GetListAsync())
        {
            using (CurrentTenant.Change(tenant.Id))
            {
                user = await FindUserInCurrentTenantAsync(userNameOrEmailAddress);
                if (user != null)
                {
                    return user;
                }
            }
        }

        return null;
    }

    private async Task<IdentityUser?> FindUserInCurrentTenantAsync(string userNameOrEmailAddress)
    {
        return await UserManager.FindByNameAsync(userNameOrEmailAddress) ??
               await UserManager.FindByEmailAsync(userNameOrEmailAddress);
    }
}
