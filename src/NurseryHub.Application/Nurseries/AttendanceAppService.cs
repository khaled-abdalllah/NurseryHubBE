using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using NurseryHub.Security;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.TenantManagement;

namespace NurseryHub.Nurseries;

[Authorize(Roles =
    $"{NurseryHubRoles.Admin},{NurseryHubRoles.NurseryAdmin},{NurseryHubRoles.BranchManager},{NurseryHubRoles.Teacher}")]
public class AttendanceAppService : ApplicationService, IAttendanceAppService
{
    private readonly IRepository<Attendance, Guid> _attendanceRepository;
    private readonly IRepository<Student, Guid> _studentRepository;
    private readonly IRepository<NurseryBranch, Guid> _nurseryBranchRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly NurseryMediaOptions _mediaOptions;

    public AttendanceAppService(
        IRepository<Attendance, Guid> attendanceRepository,
        IRepository<Student, Guid> studentRepository,
        IRepository<NurseryBranch, Guid> nurseryBranchRepository,
        ITenantRepository tenantRepository,
        IOptions<NurseryMediaOptions> mediaOptions)
    {
        _attendanceRepository = attendanceRepository;
        _studentRepository = studentRepository;
        _nurseryBranchRepository = nurseryBranchRepository;
        _tenantRepository = tenantRepository;
        _mediaOptions = mediaOptions.Value;
    }

    public virtual async Task<AttendanceDayRowDto> MarkAsync(MarkAttendanceInput input)
    {
        var student = await ValidateStudentInBranchAsync(input.StudentId, input.NurseryBranchId);
        var (checkedInAt, checkedOutAt) = NormalizeCheckTimes(input.Status, input.CheckedInAt, input.CheckedOutAt);

        var existing = await _attendanceRepository.FirstOrDefaultAsync(a =>
            a.StudentId == input.StudentId && a.Date == input.Date);

        if (existing == null)
        {
            var entity = new Attendance(
                GuidGenerator.Create(),
                CurrentTenant.Id,
                input.StudentId,
                input.Date,
                input.Status);
            entity.SetCheckTimes(checkedInAt, checkedOutAt);
            await _attendanceRepository.InsertAsync(entity, autoSave: true);
            existing = entity;
        }
        else
        {
            existing.SetStatus(input.Status);
            existing.SetCheckTimes(checkedInAt, checkedOutAt);
            await _attendanceRepository.UpdateAsync(existing, autoSave: true);
        }

        var tenantName = await GetTenantNameAsync(student.TenantId);
        return MapToDayRowDto(student, existing, input.Date, tenantName);
    }

    public virtual async Task<IReadOnlyList<AttendanceDayRowDto>> BulkMarkAsync(BulkMarkAttendanceInput input)
    {
        if (input.Items == null || input.Items.Count == 0)
        {
            return Array.Empty<AttendanceDayRowDto>();
        }

        var results = new List<AttendanceDayRowDto>();
        foreach (var item in input.Items)
        {
            var row = await MarkAsync(new MarkAttendanceInput
            {
                NurseryBranchId = input.NurseryBranchId,
                StudentId = item.StudentId,
                Date = input.Date,
                Status = item.Status,
                CheckedInAt = item.CheckedInAt,
                CheckedOutAt = item.CheckedOutAt,
            });
            results.Add(row);
        }

        return results;
    }

    public virtual async Task<IReadOnlyList<AttendanceDayRowDto>> GetByDateAsync(GetAttendanceByDateInput input)
    {
        await EnsureBranchInTenantAsync(input.NurseryBranchId);

        var students = await _studentRepository.GetQueryableAsync();
        var attendances = await _attendanceRepository.GetQueryableAsync();

        var studentsQuery = students
            .Where(s => s.NurseryBranchId == input.NurseryBranchId
                        && s.IsActive
                        && (!input.NurseryClassId.HasValue || s.NurseryClassId == input.NurseryClassId))
            .OrderBy(s => s.FullName);

        var studentsList = await AsyncExecuter.ToListAsync(studentsQuery);
        if (studentsList.Count == 0)
        {
            return Array.Empty<AttendanceDayRowDto>();
        }

        var studentIds = studentsList.Select(s => s.Id).ToList();
        var attendanceQuery = attendances.Where(a => studentIds.Contains(a.StudentId) && a.Date == input.Date);
        var attendanceList = await AsyncExecuter.ToListAsync(attendanceQuery);
        var attendanceByStudent = attendanceList.ToDictionary(a => a.StudentId);

        var tenantName = await GetTenantNameAsync(studentsList[0].TenantId);
        return studentsList
            .Select(s =>
            {
                attendanceByStudent.TryGetValue(s.Id, out var att);
                return MapToDayRowDto(s, att, input.Date, tenantName);
            })
            .ToList();
    }

    public virtual async Task<IReadOnlyList<AttendanceHistoryItemDto>> GetHistoryAsync(GetAttendanceHistoryInput input)
    {
        await ValidateStudentInBranchAsync(input.StudentId, input.NurseryBranchId);

        var attendances = await _attendanceRepository.GetQueryableAsync();

        var start = input.StartDate ?? DateOnly.MinValue;
        var end = input.EndDate ?? DateOnly.MaxValue;

        var query = attendances
            .Where(a => a.StudentId == input.StudentId && a.Date >= start && a.Date <= end)
            .OrderByDescending(a => a.Date);

        var items = await AsyncExecuter.ToListAsync(query);
        return items
            .Select(a => new AttendanceHistoryItemDto
            {
                Id = a.Id,
                Date = a.Date,
                Status = a.Status,
                CreationTime = a.CreationTime,
                LastModificationTime = a.LastModificationTime,
                CheckedInAt = a.CheckedInAt,
                CheckedOutAt = a.CheckedOutAt,
            })
            .ToList();
    }

    private async Task EnsureBranchInTenantAsync(Guid nurseryBranchId)
    {
        var branch = await _nurseryBranchRepository.FindAsync(nurseryBranchId);
        if (branch == null)
        {
            throw new BusinessException("NurseryHub:Attendance:BranchNotFound");
        }
    }

    private async Task<Student> ValidateStudentInBranchAsync(Guid studentId, Guid nurseryBranchId)
    {
        await EnsureBranchInTenantAsync(nurseryBranchId);

        var student = await _studentRepository.FindAsync(studentId);
        if (student == null || student.NurseryBranchId != nurseryBranchId)
        {
            throw new BusinessException("NurseryHub:Attendance:StudentNotInBranch");
        }

        if (!student.IsActive)
        {
            throw new BusinessException("NurseryHub:Student:Inactive");
        }

        return student;
    }

    private async Task<string?> GetTenantNameAsync(Guid? tenantId)
    {
        if (!tenantId.HasValue)
        {
            return CurrentTenant.Name;
        }

        var tenant = await _tenantRepository.FindAsync(tenantId.Value);
        return tenant?.Name ?? CurrentTenant.Name;
    }

    private AttendanceDayRowDto MapToDayRowDto(Student student, Attendance? attendance, DateOnly asOfDate, string? tenantName)
    {
        return new AttendanceDayRowDto
        {
            StudentId = student.Id,
            FullName = student.FullName,
            BirthDate = student.BirthDate,
            AgeYearsApprox = ApproximateAgeYears(student.BirthDate, asOfDate),
            ProfileImageUrl = ResolveStudentImageDisplayUrl(student.ProfileImageFileName, tenantName),
            Status = attendance?.Status,
            AttendanceId = attendance?.Id,
            CheckedInAt = attendance?.CheckedInAt,
            CheckedOutAt = attendance?.CheckedOutAt,
        };
    }

    private static (DateTime? CheckedInAt, DateTime? CheckedOutAt) NormalizeCheckTimes(
        AttendanceStatus status,
        DateTime? checkedInAt,
        DateTime? checkedOutAt)
    {
        if (status == AttendanceStatus.Absent)
        {
            return (null, null);
        }

        return (checkedInAt, checkedOutAt);
    }

    private static decimal ApproximateAgeYears(DateOnly birthDate, DateOnly today)
    {
        var days = today.DayNumber - birthDate.DayNumber;
        if (days <= 0)
        {
            return 0;
        }

        return Math.Round(days / 365.25m, 1, MidpointRounding.AwayFromZero);
    }

    private string? ResolveStudentImageDisplayUrl(string? fileName, string? tenantName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return null;
        }

        var safeTenantName = string.IsNullOrWhiteSpace(tenantName) ? "host" : tenantName;
        var baseUrl = _mediaOptions.PublicBaseUrl.TrimEnd('/');
        return $"{baseUrl}/{safeTenantName}/students/{fileName}";
    }
}
