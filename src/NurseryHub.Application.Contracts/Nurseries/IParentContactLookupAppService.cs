using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace NurseryHub.Nurseries;

/// <summary>
/// Dedicated API for parent contact lookup — must not live on StudentAppService routes
/// (paths like <c>student/find-parent-contacts-by-phone</c> collide with <c>student/{id}</c>).
/// </summary>
public interface IParentContactLookupAppService : IApplicationService
{
    Task<List<ParentContactLookupDto>> FindParentContactsByPhoneAsync(string phoneNumber);
}
