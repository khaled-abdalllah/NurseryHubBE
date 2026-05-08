using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace NurseryHub.Nurseries;

public interface IDailyFollowupBookAppService : IApplicationService
{
    Task<DailyFollowupBookDto> GetAsync(GetDailyFollowupBookInput input);

    Task<DailyFollowupBookDto> SaveDraftAsync(SaveDailyFollowupBookInput input);

    Task<DailyFollowupBookDto> SendToParentAsync(SaveDailyFollowupBookInput input);
}
