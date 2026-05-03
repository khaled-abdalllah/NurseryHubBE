using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.SettingManagement;

namespace NurseryHub;

/// <summary>
/// Disables public self-registration (login-only). Angular and MVC account UIs read
/// <c>Abp.Account.IsSelfRegistrationEnabled</c> from application configuration.
/// </summary>
public class AccountSettingsDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly ISettingManager _settingManager;

    public AccountSettingsDataSeedContributor(ISettingManager settingManager)
    {
        _settingManager = settingManager;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        await _settingManager.SetGlobalAsync(
            "Abp.Account.IsSelfRegistrationEnabled",
            "false");
    }
}
