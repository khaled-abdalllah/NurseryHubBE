using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using NurseryHub.Locations;
using NurseryHub.Permissions;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace NurseryHub.Nurseries;


public class NurseryStatisticsAppService : ApplicationService, INurseryStatisticsAppService
{
    private readonly IRepository<Nursery, Guid> _nurseryRepository;
    private readonly IRepository<NurseryBranch, Guid> _branchRepository;
    private readonly IRepository<NurseryClass, Guid> _classRepository;
    private readonly IRepository<Governorate, Guid> _governorateRepository;

    public NurseryStatisticsAppService(
        IRepository<Nursery, Guid> nurseryRepository,
        IRepository<NurseryBranch, Guid> branchRepository,
        IRepository<NurseryClass, Guid> classRepository,
        IRepository<Governorate, Guid> governorateRepository)
    {
        _nurseryRepository = nurseryRepository;
        _branchRepository = branchRepository;
        _classRepository = classRepository;
        _governorateRepository = governorateRepository;
    }

    public virtual async Task<NurseryStatisticsDto> GetStatisticsAsync()
    {
        var nurseryQuery = await _nurseryRepository.GetQueryableAsync();
        var branchQuery = await _branchRepository.GetQueryableAsync();
        var classQuery = await _classRepository.GetQueryableAsync();
        var governorateQuery = await _governorateRepository.GetQueryableAsync();

        var totalNurseries = await AsyncExecuter.CountAsync(nurseryQuery);
        var activeNurseries = await AsyncExecuter.CountAsync(nurseryQuery.Where(n => n.IsActive));
        var inactiveNurseries = totalNurseries - activeNurseries;

        var totalBranches = await AsyncExecuter.CountAsync(branchQuery);
        var totalClasses = await AsyncExecuter.CountAsync(classQuery);

        var newByMonth = await BuildMonthlyNurseryCreationsAsync(nurseryQuery);
        var byGov = await BuildBranchesByGovernorateAsync(branchQuery, governorateQuery);

        return new NurseryStatisticsDto
        {
            TotalNurseries = totalNurseries,
            ActiveNurseries = activeNurseries,
            InactiveNurseries = inactiveNurseries,
            TotalBranches = totalBranches,
            TotalClasses = totalClasses,
            NewNurseriesByMonth = newByMonth,
            BranchesByGovernorate = byGov,
        };
    }

    private async Task<List<NurseryMonthlySeriesItemDto>> BuildMonthlyNurseryCreationsAsync(
        IQueryable<Nursery> nurseryQuery)
    {
        var list = new List<NurseryMonthlySeriesItemDto>();
        var now = Clock.Now;
        for (var i = 5; i >= 0; i--)
        {
            var d = now.AddMonths(-i);
            var y = d.Year;
            var m = d.Month;
            var start = new DateTime(y, m, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = start.AddMonths(1);

            var count = await AsyncExecuter.CountAsync(
                nurseryQuery.Where(n => n.CreationTime >= start && n.CreationTime < end));

            list.Add(new NurseryMonthlySeriesItemDto { Year = y, Month = m, Count = count });
        }

        return list;
    }

    private async Task<List<GovernorateBranchCountDto>> BuildBranchesByGovernorateAsync(
        IQueryable<NurseryBranch> branchQuery,
        IQueryable<Governorate> governorateQuery)
    {
        var grouped = from b in branchQuery
            join g in governorateQuery on b.GovernorateId equals g.Id
            group b by new { g.Id, g.NameEn, g.NameAr } into grp
            select new
            {
                grp.Key.Id,
                grp.Key.NameEn,
                grp.Key.NameAr,
                Count = grp.Count(),
            };

        var ordered = await AsyncExecuter.ToListAsync(
            grouped.OrderByDescending(x => x.Count).Take(12));

        var useAr = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.Equals(
            "ar",
            StringComparison.OrdinalIgnoreCase);

        return ordered
            .Select(x => new GovernorateBranchCountDto
            {
                GovernorateId = x.Id,
                DisplayName = useAr ? x.NameAr : x.NameEn,
                BranchCount = x.Count,
            })
            .ToList();
    }
}
