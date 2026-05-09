using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using NurseryHub.Security;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace NurseryHub.Nurseries;

[Authorize(Roles = $"{NurseryHubRoles.Admin},{NurseryHubRoles.NurseryAdmin},{NurseryHubRoles.Accountant}")]
public class PaymentAppService : ApplicationService, IPaymentAppService
{
    private readonly IRepository<Payment, Guid> _paymentRepository;
    private readonly IRepository<Student, Guid> _studentRepository;
    private readonly IRepository<ParentContact, Guid> _parentContactRepository;
    private readonly IRepository<NurseryClass, Guid> _classRepository;
    private readonly IRepository<GradeCategory, Guid> _gradeRepository;
    private readonly IRepository<UserBranch, Guid> _userBranchRepository;

    public PaymentAppService(
        IRepository<Payment, Guid> paymentRepository,
        IRepository<Student, Guid> studentRepository,
        IRepository<ParentContact, Guid> parentContactRepository,
        IRepository<NurseryClass, Guid> classRepository,
        IRepository<GradeCategory, Guid> gradeRepository,
        IRepository<UserBranch, Guid> userBranchRepository)
    {
        _paymentRepository = paymentRepository;
        _studentRepository = studentRepository;
        _parentContactRepository = parentContactRepository;
        _classRepository = classRepository;
        _gradeRepository = gradeRepository;
        _userBranchRepository = userBranchRepository;
    }

    public virtual async Task<PagedResultDto<PaymentListDto>> GetListAsync(PaymentFilterDto input)
    {
        var accessibleBranchIds = await GetAccessibleBranchIdsAsync(input.NurseryBranchId);
        var payments = await _paymentRepository.GetQueryableAsync();
        var students = await _studentRepository.GetQueryableAsync();
        var parents = await _parentContactRepository.GetQueryableAsync();
        var classes = await _classRepository.GetQueryableAsync();
        var grades = await _gradeRepository.GetQueryableAsync();

        var filtered = payments
            .WhereIf(input.GradeId.HasValue, x => x.GradeId == input.GradeId)
            .WhereIf(input.ClassId.HasValue, x => x.ClassId == input.ClassId)
            .WhereIf(input.StudentId.HasValue, x => x.StudentId == input.StudentId)
            .WhereIf(input.PaymentMethod.HasValue, x => x.PaymentMethod == input.PaymentMethod)
            .WhereIf(input.PaymentPurpose.HasValue, x => x.PaymentPurpose == input.PaymentPurpose)
            .WhereIf(input.DateFrom.HasValue, x => x.PaymentDate >= input.DateFrom!.Value.Date)
            .WhereIf(input.DateTo.HasValue, x => x.PaymentDate <= input.DateTo!.Value.Date.AddDays(1).AddTicks(-1));

        filtered =
            from payment in filtered
            join nurseryClass in classes on payment.ClassId equals nurseryClass.Id
            where accessibleBranchIds.Contains(nurseryClass.NurseryBranchId)
            select payment;

        var query =
            from payment in filtered
            join student in students on payment.StudentId equals student.Id
            join parent in parents on student.ParentId equals parent.Id
            join nurseryClass in classes on payment.ClassId equals nurseryClass.Id into classJoin
            from nurseryClass in classJoin.DefaultIfEmpty()
            join grade in grades on payment.GradeId equals grade.Id into gradeJoin
            from grade in gradeJoin.DefaultIfEmpty()
            select new PaymentListDto
            {
                Id = payment.Id,
                PaymentNumber = payment.PaymentNumber,
                StudentId = payment.StudentId,
                StudentName = BuildStudentDisplayName(student.FullName, parent.FatherName),
                GradeId = payment.GradeId,
                GradeName = grade != null ? grade.Name : null,
                ClassId = payment.ClassId,
                ClassName = nurseryClass != null ? nurseryClass.Name : null,
                PaymentMethod = payment.PaymentMethod,
                PaymentPurpose = payment.PaymentPurpose,
                CustomPaymentPurpose = payment.CustomPaymentPurpose,
                Amount = payment.Amount,
                PaymentDate = payment.PaymentDate,
            };

        query = query.WhereIf(
            !input.Filter.IsNullOrWhiteSpace(),
            x => x.StudentName.Contains(input.Filter!) ||
                 x.PaymentNumber.Contains(input.Filter!) ||
                 (x.CustomPaymentPurpose != null && x.CustomPaymentPurpose.Contains(input.Filter!)));

        var totalCount = await AsyncExecuter.CountAsync(query);
        var items = await AsyncExecuter.ToListAsync(
            ApplySorting(query, input.Sorting).Skip(input.SkipCount).Take(input.MaxResultCount));

        return new PagedResultDto<PaymentListDto>(totalCount, items);
    }

    public virtual async Task<PaymentDetailsDto> GetAsync(Guid id)
    {
        var payment = await _paymentRepository.GetAsync(id);
        var student = await _studentRepository.GetAsync(payment.StudentId);
        var parent = await _parentContactRepository.GetAsync(student.ParentId);
        var nurseryClass = await _classRepository.FindAsync(payment.ClassId);
        await EnsureCanAccessBranchAsync(nurseryClass?.NurseryBranchId);
        var grade = await _gradeRepository.FindAsync(payment.GradeId);

        return new PaymentDetailsDto
        {
            Id = payment.Id,
            PaymentNumber = payment.PaymentNumber,
            StudentId = payment.StudentId,
            StudentName = BuildStudentDisplayName(student.FullName, parent.FatherName),
            GradeId = payment.GradeId,
            GradeName = grade?.Name,
            ClassId = payment.ClassId,
            ClassName = nurseryClass?.Name,
            PaymentMethod = payment.PaymentMethod,
            PaymentPurpose = payment.PaymentPurpose,
            CustomPaymentPurpose = payment.CustomPaymentPurpose,
            Amount = payment.Amount,
            Notes = payment.Notes,
            PaymentDate = payment.PaymentDate,
            CreationTime = payment.CreationTime,
            CreatorId = payment.CreatorId,
            LastModificationTime = payment.LastModificationTime,
            LastModifierId = payment.LastModifierId,
            IsDeleted = payment.IsDeleted,
            DeletionTime = payment.DeletionTime,
            DeleterId = payment.DeleterId,
        };
    }

    public virtual async Task<PaymentDto> CreateAsync(CreateUpdatePaymentDto input)
    {
        await ValidateRelationsAsync(input);

        var payment = new Payment(
            GuidGenerator.Create(),
            CurrentTenant.Id,
            await GeneratePaymentNumberAsync(),
            input.StudentId,
            input.GradeId,
            input.ClassId,
            input.PaymentMethod,
            input.PaymentPurpose,
            input.CustomPaymentPurpose,
            input.Amount,
            input.Notes,
            input.PaymentDate);

        await _paymentRepository.InsertAsync(payment, autoSave: true);
        return await MapToPaymentDtoAsync(payment);
    }

    public virtual async Task<PaymentDto> UpdateAsync(Guid id, CreateUpdatePaymentDto input)
    {
        await ValidateRelationsAsync(input);
        var payment = await _paymentRepository.GetAsync(id);
        var existingClass = await _classRepository.FindAsync(payment.ClassId);
        await EnsureCanAccessBranchAsync(existingClass?.NurseryBranchId);

        payment.SetStudentStudentAndClass(input.StudentId, input.GradeId, input.ClassId);
        payment.SetMethodAndPurpose(input.PaymentMethod, input.PaymentPurpose, input.CustomPaymentPurpose);
        payment.SetAmount(input.Amount);
        payment.SetNotes(input.Notes);
        payment.SetPaymentDate(input.PaymentDate);

        await _paymentRepository.UpdateAsync(payment, autoSave: true);
        return await MapToPaymentDtoAsync(payment);
    }

    public virtual async Task DeleteAsync(Guid id)
    {
        var payment = await _paymentRepository.GetAsync(id);
        var nurseryClass = await _classRepository.FindAsync(payment.ClassId);
        await EnsureCanAccessBranchAsync(nurseryClass?.NurseryBranchId);
        await _paymentRepository.DeleteAsync(payment);
    }

    public virtual async Task<IReadOnlyList<PaymentStudentLookupDto>> GetStudentsByClassAsync(GetStudentsByClassInput input)
    {
        var nurseryClass = await _classRepository.FindAsync(input.ClassId);
        await EnsureCanAccessBranchAsync(nurseryClass?.NurseryBranchId);

        var students = await _studentRepository.GetQueryableAsync();
        var parents = await _parentContactRepository.GetQueryableAsync();
        var query =
            from student in students.Where(x => x.IsActive && x.NurseryClassId == input.ClassId)
            join parent in parents on student.ParentId equals parent.Id
            select new { student, parent };

        query = query.WhereIf(
            !input.Filter.IsNullOrWhiteSpace(),
            x => x.student.FullName.Contains(input.Filter!));

        var result = await AsyncExecuter.ToListAsync(
            query.OrderBy(x => x.student.FullName).Take(50));

        return result.Select(x => new PaymentStudentLookupDto
        {
            Id = x.student.Id,
            FullName = BuildStudentDisplayName(x.student.FullName, x.parent.FatherName),
        }).ToList();
    }

    private async Task<PaymentDto> MapToPaymentDtoAsync(Payment payment)
    {
        var student = await _studentRepository.GetAsync(payment.StudentId);
        var parent = await _parentContactRepository.GetAsync(student.ParentId);
        var nurseryClass = await _classRepository.FindAsync(payment.ClassId);
        var grade = await _gradeRepository.FindAsync(payment.GradeId);

        return new PaymentDto
        {
            Id = payment.Id,
            PaymentNumber = payment.PaymentNumber,
            StudentId = payment.StudentId,
            StudentName = BuildStudentDisplayName(student.FullName, parent.FatherName),
            GradeId = payment.GradeId,
            GradeName = grade?.Name,
            ClassId = payment.ClassId,
            ClassName = nurseryClass?.Name,
            PaymentMethod = payment.PaymentMethod,
            PaymentPurpose = payment.PaymentPurpose,
            CustomPaymentPurpose = payment.CustomPaymentPurpose,
            Amount = payment.Amount,
            Notes = payment.Notes,
            PaymentDate = payment.PaymentDate,
            CreationTime = payment.CreationTime,
            CreatorId = payment.CreatorId,
            LastModificationTime = payment.LastModificationTime,
            LastModifierId = payment.LastModifierId,
            IsDeleted = payment.IsDeleted,
            DeletionTime = payment.DeletionTime,
            DeleterId = payment.DeleterId,
        };
    }

    private async Task ValidateRelationsAsync(CreateUpdatePaymentDto input)
    {
        var nurseryClass = await _classRepository.FindAsync(input.ClassId);
        if (nurseryClass == null)
        {
            throw new BusinessException("NurseryHub:Payments:ClassNotFound");
        }

        await EnsureCanAccessBranchAsync(nurseryClass.NurseryBranchId);

        var grade = await _gradeRepository.FindAsync(input.GradeId);
        if (grade == null)
        {
            throw new BusinessException("NurseryHub:Payments:GradeNotFound");
        }

        var student = await _studentRepository.FindAsync(input.StudentId);
        if (student == null || !student.IsActive || student.NurseryClassId != input.ClassId)
        {
            throw new BusinessException("NurseryHub:Payments:StudentNotFoundInClass");
        }

        if (student.NurseryBranchId != nurseryClass.NurseryBranchId)
        {
            throw new BusinessException("NurseryHub:Payments:BranchMismatch");
        }

        if (nurseryClass.GradeCategoryId != input.GradeId)
        {
            throw new BusinessException("NurseryHub:Payments:GradeClassMismatch");
        }
    }

    private async Task EnsureCanAccessBranchAsync(Guid? nurseryBranchId)
    {
        if (!nurseryBranchId.HasValue || !CurrentUser.Id.HasValue || !CurrentTenant.Id.HasValue)
        {
            throw new AbpAuthorizationException("Current user is not allowed to access this nursery branch.");
        }

        var canAccess = await _userBranchRepository.AnyAsync(x =>
            x.TenantId == CurrentTenant.Id &&
            x.UserId == CurrentUser.Id.Value &&
            x.NurseryBranchId == nurseryBranchId.Value);
        if (!canAccess)
        {
            throw new AbpAuthorizationException("Current user is not allowed to access this nursery branch.");
        }
    }

    private async Task<Guid[]> GetAccessibleBranchIdsAsync(Guid? requestedBranchId)
    {
        if (requestedBranchId.HasValue)
        {
            await EnsureCanAccessBranchAsync(requestedBranchId.Value);
            return new[] { requestedBranchId.Value };
        }

        if (!CurrentUser.Id.HasValue || !CurrentTenant.Id.HasValue)
        {
            return Array.Empty<Guid>();
        }

        var userBranches = await _userBranchRepository.GetListAsync(x =>
            x.TenantId == CurrentTenant.Id &&
            x.UserId == CurrentUser.Id.Value);
        return userBranches.Select(x => x.NurseryBranchId).Distinct().ToArray();
    }

    private async Task<string> GeneratePaymentNumberAsync()
    {
        var year = Clock.Now.Year;
        var prefix = $"PAY-{year}-";
        var existing = await _paymentRepository.GetQueryableAsync();
        var count = await AsyncExecuter.CountAsync(existing.Where(x => x.PaymentNumber.StartsWith(prefix)));
        return $"{prefix}{(count + 1):D6}";
    }

    private static IQueryable<PaymentListDto> ApplySorting(IQueryable<PaymentListDto> query, string? sorting)
    {
        if (sorting.IsNullOrWhiteSpace())
        {
            return query.OrderByDescending(x => x.PaymentDate);
        }

        if (sorting.Contains("amount", StringComparison.OrdinalIgnoreCase))
        {
            return sorting.Contains("desc", StringComparison.OrdinalIgnoreCase)
                ? query.OrderByDescending(x => x.Amount)
                : query.OrderBy(x => x.Amount);
        }

        if (sorting.Contains("studentName", StringComparison.OrdinalIgnoreCase))
        {
            return sorting.Contains("desc", StringComparison.OrdinalIgnoreCase)
                ? query.OrderByDescending(x => x.StudentName)
                : query.OrderBy(x => x.StudentName);
        }

        return sorting.Contains("asc", StringComparison.OrdinalIgnoreCase)
            ? query.OrderBy(x => x.PaymentDate)
            : query.OrderByDescending(x => x.PaymentDate);
    }

    private static string BuildStudentDisplayName(string studentName, string fatherName)
    {
        if (fatherName.IsNullOrWhiteSpace())
        {
            return studentName;
        }

        return $"{studentName} - {fatherName}";
    }
}
