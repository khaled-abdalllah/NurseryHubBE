using System;
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
public class ExpenseAppService : ApplicationService, IExpenseAppService
{
    private readonly IRepository<Expense, Guid> _expenseRepository;
    private readonly IRepository<NurseryBranch, Guid> _branchRepository;
    private readonly IRepository<UserBranch, Guid> _userBranchRepository;

    public ExpenseAppService(
        IRepository<Expense, Guid> expenseRepository,
        IRepository<NurseryBranch, Guid> branchRepository,
        IRepository<UserBranch, Guid> userBranchRepository)
    {
        _expenseRepository = expenseRepository;
        _branchRepository = branchRepository;
        _userBranchRepository = userBranchRepository;
    }

    public virtual async Task<PagedResultDto<ExpenseListDto>> GetListAsync(ExpenseFilterDto input)
    {
        var accessibleBranchIds = await GetAccessibleBranchIdsAsync(input.NurseryBranchId);
        var queryable = await _expenseRepository.GetQueryableAsync();
        var query = queryable
            .Where(x => accessibleBranchIds.Contains(x.NurseryBranchId))
            .WhereIf(input.ExpenseCategory.HasValue, x => x.ExpenseCategory == input.ExpenseCategory)
            .WhereIf(!input.CustomExpenseCategory.IsNullOrWhiteSpace(), x => x.CustomExpenseCategory != null && x.CustomExpenseCategory.Contains(input.CustomExpenseCategory!))
            .WhereIf(input.PaymentMethod.HasValue, x => x.PaymentMethod == input.PaymentMethod)
            .WhereIf(input.PaymentStatus.HasValue, x => x.PaymentStatus == input.PaymentStatus)
            .WhereIf(!input.VendorName.IsNullOrWhiteSpace(), x => x.VendorName != null && x.VendorName.Contains(input.VendorName!))
            .WhereIf(input.DateFrom.HasValue, x => x.ExpenseDate >= input.DateFrom!.Value.Date)
            .WhereIf(input.DateTo.HasValue, x => x.ExpenseDate <= input.DateTo!.Value.Date.AddDays(1).AddTicks(-1))
            .WhereIf(input.MinAmount.HasValue, x => x.Amount >= input.MinAmount)
            .WhereIf(input.MaxAmount.HasValue, x => x.Amount <= input.MaxAmount)
            .WhereIf(!input.Filter.IsNullOrWhiteSpace(),
                x => x.Title.Contains(input.Filter!) ||
                     x.ExpenseNumber.Contains(input.Filter!) ||
                     (x.VendorName != null && x.VendorName.Contains(input.Filter!)));

        var projected = query.Select(x => new ExpenseListDto
        {
            Id = x.Id,
            NurseryBranchId = x.NurseryBranchId,
            ExpenseNumber = x.ExpenseNumber,
            Title = x.Title,
            ExpenseCategory = x.ExpenseCategory,
            CustomExpenseCategory = x.CustomExpenseCategory,
            VendorName = x.VendorName,
            PaymentMethod = x.PaymentMethod,
            PaymentStatus = x.PaymentStatus,
            Amount = x.Amount,
            TotalAmount = x.TotalAmount,
            ExpenseDate = x.ExpenseDate,
        });

        var totalCount = await AsyncExecuter.CountAsync(projected);
        var items = await AsyncExecuter.ToListAsync(
            ApplySorting(projected, input.Sorting)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount));

        return new PagedResultDto<ExpenseListDto>(totalCount, items);
    }

    public virtual async Task<ExpenseDetailsDto> GetAsync(Guid id)
    {
        var expense = await _expenseRepository.GetAsync(id);
        await EnsureCanAccessBranchAsync(expense.NurseryBranchId);
        return MapToExpenseDetailsDto(expense);
    }

    public virtual async Task<ExpenseDto> CreateAsync(CreateUpdateExpenseDto input)
    {
        await EnsureValidAccessibleBranchAsync(input.NurseryBranchId);

        var expense = new Expense(
            GuidGenerator.Create(),
            CurrentTenant.Id,
            input.NurseryBranchId,
            await GenerateExpenseNumberAsync(),
            input.Title,
            input.ExpenseCategory,
            input.CustomExpenseCategory,
            input.VendorName,
            input.PaymentMethod,
            ExpensePaymentStatus.Paid,
            input.Amount,
            0,
            input.ExpenseDate,
            input.ReceiptNumber,
            input.Notes,
            input.ReceiptFileId);

        await _expenseRepository.InsertAsync(expense, autoSave: true);
        return MapToExpenseDto(expense);
    }

    public virtual async Task<ExpenseDto> UpdateAsync(Guid id, CreateUpdateExpenseDto input)
    {
        var expense = await _expenseRepository.GetAsync(id);
        await EnsureCanAccessBranchAsync(expense.NurseryBranchId);
        await EnsureValidAccessibleBranchAsync(input.NurseryBranchId);
        expense.SetNurseryBranch(input.NurseryBranchId);
        expense.SetInformation(input.Title, input.ExpenseCategory, input.CustomExpenseCategory, input.VendorName);
        expense.SetPayment(input.PaymentMethod, ExpensePaymentStatus.Paid);
        expense.SetAmounts(input.Amount, 0);
        expense.SetExpenseDate(input.ExpenseDate);
        expense.SetReceipt(input.ReceiptNumber, input.ReceiptFileId);
        expense.SetNotes(input.Notes);

        await _expenseRepository.UpdateAsync(expense, autoSave: true);
        return MapToExpenseDto(expense);
    }

    public virtual async Task DeleteAsync(Guid id)
    {
        var expense = await _expenseRepository.GetAsync(id);
        await EnsureCanAccessBranchAsync(expense.NurseryBranchId);
        await _expenseRepository.DeleteAsync(expense);
    }

    private async Task<string> GenerateExpenseNumberAsync()
    {
        var year = Clock.Now.Year;
        var prefix = $"EXP-{year}-";
        var existing = await _expenseRepository.GetQueryableAsync();
        var count = await AsyncExecuter.CountAsync(existing.Where(x => x.ExpenseNumber.StartsWith(prefix)));
        return $"{prefix}{(count + 1):D6}";
    }

    private async Task EnsureValidAccessibleBranchAsync(Guid nurseryBranchId)
    {
        if (nurseryBranchId == Guid.Empty)
        {
            throw new BusinessException("NurseryHub:Expenses:BranchRequired");
        }

        var branch = await _branchRepository.FindAsync(nurseryBranchId);
        if (branch == null || branch.TenantId != CurrentTenant.Id)
        {
            throw new BusinessException("NurseryHub:Expenses:InvalidBranch");
        }

        await EnsureCanAccessBranchAsync(nurseryBranchId);
    }

    private async Task EnsureCanAccessBranchAsync(Guid nurseryBranchId)
    {
        if (!CurrentUser.Id.HasValue || !CurrentTenant.Id.HasValue)
        {
            throw new AbpAuthorizationException("Current user is not allowed to access this nursery branch.");
        }

        var canAccess = await _userBranchRepository.AnyAsync(x =>
            x.TenantId == CurrentTenant.Id &&
            x.UserId == CurrentUser.Id.Value &&
            x.NurseryBranchId == nurseryBranchId);
        if (!canAccess)
        {
            throw new AbpAuthorizationException("Current user is not allowed to access this nursery branch.");
        }
    }

    private async Task<Guid[]> GetAccessibleBranchIdsAsync(Guid? requestedBranchId)
    {
        if (requestedBranchId.HasValue)
        {
            await EnsureValidAccessibleBranchAsync(requestedBranchId.Value);
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

    private static IQueryable<ExpenseListDto> ApplySorting(IQueryable<ExpenseListDto> query, string? sorting)
    {
        if (sorting.IsNullOrWhiteSpace())
        {
            return query.OrderByDescending(x => x.ExpenseDate);
        }

        if (sorting.Contains("amount", StringComparison.OrdinalIgnoreCase))
        {
            return sorting.Contains("desc", StringComparison.OrdinalIgnoreCase)
                ? query.OrderByDescending(x => x.Amount)
                : query.OrderBy(x => x.Amount);
        }

        if (sorting.Contains("title", StringComparison.OrdinalIgnoreCase))
        {
            return sorting.Contains("desc", StringComparison.OrdinalIgnoreCase)
                ? query.OrderByDescending(x => x.Title)
                : query.OrderBy(x => x.Title);
        }

        return sorting.Contains("asc", StringComparison.OrdinalIgnoreCase)
            ? query.OrderBy(x => x.ExpenseDate)
            : query.OrderByDescending(x => x.ExpenseDate);
    }

    private static ExpenseDto MapToExpenseDto(Expense expense)
    {
        return new ExpenseDto
        {
            Id = expense.Id,
            NurseryBranchId = expense.NurseryBranchId,
            ExpenseNumber = expense.ExpenseNumber,
            Title = expense.Title,
            ExpenseCategory = expense.ExpenseCategory,
            CustomExpenseCategory = expense.CustomExpenseCategory,
            VendorName = expense.VendorName,
            PaymentMethod = expense.PaymentMethod,
            PaymentStatus = expense.PaymentStatus,
            Amount = expense.Amount,
            TaxAmount = expense.TaxAmount,
            TotalAmount = expense.TotalAmount,
            ExpenseDate = expense.ExpenseDate,
            ReceiptNumber = expense.ReceiptNumber,
            Notes = expense.Notes,
            ReceiptFileId = expense.ReceiptFileId,
            CreationTime = expense.CreationTime,
            CreatorId = expense.CreatorId,
            LastModificationTime = expense.LastModificationTime,
            LastModifierId = expense.LastModifierId,
            IsDeleted = expense.IsDeleted,
            DeletionTime = expense.DeletionTime,
            DeleterId = expense.DeleterId,
        };
    }

    private static ExpenseDetailsDto MapToExpenseDetailsDto(Expense expense)
    {
        return new ExpenseDetailsDto
        {
            Id = expense.Id,
            NurseryBranchId = expense.NurseryBranchId,
            ExpenseNumber = expense.ExpenseNumber,
            Title = expense.Title,
            ExpenseCategory = expense.ExpenseCategory,
            CustomExpenseCategory = expense.CustomExpenseCategory,
            VendorName = expense.VendorName,
            PaymentMethod = expense.PaymentMethod,
            PaymentStatus = expense.PaymentStatus,
            Amount = expense.Amount,
            TaxAmount = expense.TaxAmount,
            TotalAmount = expense.TotalAmount,
            ExpenseDate = expense.ExpenseDate,
            ReceiptNumber = expense.ReceiptNumber,
            Notes = expense.Notes,
            ReceiptFileId = expense.ReceiptFileId,
            CreationTime = expense.CreationTime,
            CreatorId = expense.CreatorId,
            LastModificationTime = expense.LastModificationTime,
            LastModifierId = expense.LastModifierId,
            IsDeleted = expense.IsDeleted,
            DeletionTime = expense.DeletionTime,
            DeleterId = expense.DeleterId,
        };
    }
}
