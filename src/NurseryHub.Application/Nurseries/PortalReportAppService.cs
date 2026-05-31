using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using NurseryHub.Security;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;

namespace NurseryHub.Nurseries;

[Authorize(Roles =
    $"{NurseryHubRoles.Admin},{NurseryHubRoles.NurseryAdmin},{NurseryHubRoles.BranchManager}")]
public class PortalReportAppService : ApplicationService, IPortalReportAppService
{
    private const int MaxReportRows = 5000;

    private static readonly PortalReportType[] NurseryAdminOnlyReports =
    {
        PortalReportType.Payments,
        PortalReportType.Expenses,
        PortalReportType.Staff,
    };

    private readonly IRepository<NurseryBranch, Guid> _branchRepository;
    private readonly IRepository<UserBranch, Guid> _userBranchRepository;
    private readonly IRepository<Student, Guid> _studentRepository;
    private readonly IRepository<ParentContact, Guid> _parentContactRepository;
    private readonly IRepository<NurseryClass, Guid> _classRepository;
    private readonly IRepository<GradeCategory, Guid> _gradeRepository;
    private readonly IRepository<StudentApplication, Guid> _applicationRepository;
    private readonly IRepository<Attendance, Guid> _attendanceRepository;
    private readonly IRepository<Payment, Guid> _paymentRepository;
    private readonly IRepository<Expense, Guid> _expenseRepository;
    private readonly IRepository<Notification, Guid> _notificationRepository;
    private readonly IRepository<NotificationRecipient, Guid> _notificationRecipientRepository;
    private readonly IIdentityUserRepository _identityUserRepository;
    private readonly IdentityUserManager _identityUserManager;

    public PortalReportAppService(
        IRepository<NurseryBranch, Guid> branchRepository,
        IRepository<UserBranch, Guid> userBranchRepository,
        IRepository<Student, Guid> studentRepository,
        IRepository<ParentContact, Guid> parentContactRepository,
        IRepository<NurseryClass, Guid> classRepository,
        IRepository<GradeCategory, Guid> gradeRepository,
        IRepository<StudentApplication, Guid> applicationRepository,
        IRepository<Attendance, Guid> attendanceRepository,
        IRepository<Payment, Guid> paymentRepository,
        IRepository<Expense, Guid> expenseRepository,
        IRepository<Notification, Guid> notificationRepository,
        IRepository<NotificationRecipient, Guid> notificationRecipientRepository,
        IIdentityUserRepository identityUserRepository,
        IdentityUserManager identityUserManager)
    {
        _branchRepository = branchRepository;
        _userBranchRepository = userBranchRepository;
        _studentRepository = studentRepository;
        _parentContactRepository = parentContactRepository;
        _classRepository = classRepository;
        _gradeRepository = gradeRepository;
        _applicationRepository = applicationRepository;
        _attendanceRepository = attendanceRepository;
        _paymentRepository = paymentRepository;
        _expenseRepository = expenseRepository;
        _notificationRepository = notificationRepository;
        _notificationRecipientRepository = notificationRecipientRepository;
        _identityUserRepository = identityUserRepository;
        _identityUserManager = identityUserManager;
    }

    public virtual async Task<PortalReportResultDto> GenerateAsync(PortalReportInput input)
    {
        await EnsureCanAccessBranchAsync(input.NurseryBranchId);
        EnsureReportTypeAllowed(input.ReportType);

        var branch = await _branchRepository.GetAsync(input.NurseryBranchId);
        var result = input.ReportType switch
        {
            PortalReportType.Students => await GenerateStudentsReportAsync(input, branch.Name),
            PortalReportType.Applications => await GenerateApplicationsReportAsync(input, branch.Name),
            PortalReportType.Attendance => await GenerateAttendanceReportAsync(input, branch.Name),
            PortalReportType.Classes => await GenerateClassesReportAsync(input, branch.Name),
            PortalReportType.Grades => await GenerateGradesReportAsync(input, branch.Name),
            PortalReportType.Payments => await GeneratePaymentsReportAsync(input, branch.Name),
            PortalReportType.Expenses => await GenerateExpensesReportAsync(input, branch.Name),
            PortalReportType.Staff => await GenerateStaffReportAsync(input, branch.Name),
            PortalReportType.Notifications => await GenerateNotificationsReportAsync(input, branch.Name),
            _ => throw new BusinessException("NurseryHub:Reports:UnknownType"),
        };

        result.GeneratedAt = Clock.Now;
        return result;
    }

    private void EnsureReportTypeAllowed(PortalReportType reportType)
    {
        if (!NurseryAdminOnlyReports.Contains(reportType))
        {
            return;
        }

        if (CurrentUser.Roles.Any(r =>
                string.Equals(r, NurseryHubRoles.Admin, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(r, NurseryHubRoles.NurseryAdmin, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(r, NurseryHubRoles.BranchManager, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        throw new AbpAuthorizationException("Current user is not allowed to generate this report.");
    }

    private async Task EnsureCanAccessBranchAsync(Guid branchId)
    {
        if (!CurrentUser.Id.HasValue || !CurrentTenant.Id.HasValue)
        {
            throw new AbpAuthorizationException();
        }

        var canAccess = await _userBranchRepository.AnyAsync(x =>
            x.TenantId == CurrentTenant.Id &&
            x.UserId == CurrentUser.Id.Value &&
            x.NurseryBranchId == branchId);
        if (!canAccess)
        {
            throw new AbpAuthorizationException("Current user is not allowed to access this nursery branch.");
        }
    }

    private async Task<PortalReportResultDto> GenerateStudentsReportAsync(PortalReportInput input, string branchName)
    {
        var students = await _studentRepository.GetQueryableAsync();
        var classes = await _classRepository.GetQueryableAsync();
        var grades = await _gradeRepository.GetQueryableAsync();
        var parents = await _parentContactRepository.GetQueryableAsync();

        var query =
            from student in students
            join parent in parents on student.ParentId equals parent.Id
            join cls in classes on student.NurseryClassId equals cls.Id into classJoin
            from cls in classJoin.DefaultIfEmpty()
            join grade in grades on cls.GradeCategoryId equals grade.Id into gradeJoin
            from grade in gradeJoin.DefaultIfEmpty()
            where student.NurseryBranchId == input.NurseryBranchId
            select new { student, parent, cls, grade };

        query = query
            .WhereIf(input.NurseryClassId.HasValue, x => x.student.NurseryClassId == input.NurseryClassId)
            .WhereIf(input.GradeCategoryId.HasValue, x => x.cls != null && x.cls.GradeCategoryId == input.GradeCategoryId)
            .WhereIf(input.IsActive.HasValue, x => x.student.IsActive == input.IsActive!.Value)
            .WhereIf(input.DateFrom.HasValue, x => x.student.EnrollmentDate >= DateOnly.FromDateTime(input.DateFrom!.Value.Date))
            .WhereIf(input.DateTo.HasValue, x => x.student.EnrollmentDate <= DateOnly.FromDateTime(input.DateTo!.Value.Date));

        var items = await AsyncExecuter.ToListAsync(
            query.OrderBy(x => x.student.FullName).Take(MaxReportRows));

        var columns = Columns(
            ("fullName", "Portal:Reports:Col:StudentName"),
            ("className", "Portal:Reports:Col:Class"),
            ("gradeName", "Portal:Reports:Col:Grade"),
            ("gender", "Portal:Reports:Col:Gender"),
            ("birthDate", "Portal:Reports:Col:BirthDate"),
            ("enrollmentDate", "Portal:Reports:Col:EnrollmentDate"),
            ("fatherPhone", "Portal:Reports:Col:FatherPhone"),
            ("isActive", "Portal:Reports:Col:Status"));

        var rows = items.Select(x => new List<string>
        {
            x.student.FullName,
            x.cls?.Name ?? "-",
            x.grade?.Name ?? "-",
            x.student.Gender,
            FormatDateOnly(x.student.BirthDate),
            FormatDateOnly(x.student.EnrollmentDate),
            x.parent.FatherPhoneNumber,
            x.student.IsActive ? "Active" : "Inactive",
        }).ToList();

        return BuildResult(
            PortalReportType.Students,
            "Portal:Reports:Type:Students",
            branchName,
            input,
            columns,
            rows);
    }

    private async Task<PortalReportResultDto> GenerateApplicationsReportAsync(PortalReportInput input, string branchName)
    {
        var apps = await _applicationRepository.GetQueryableAsync();
        var grades = await _gradeRepository.GetQueryableAsync();

        var query =
            from app in apps
            join grade in grades on app.RequestedGradeCategoryId equals grade.Id into gradeJoin
            from grade in gradeJoin.DefaultIfEmpty()
            where app.NurseryBranchId == input.NurseryBranchId
            select new { app, grade };

        query = query
            .WhereIf(input.ApplicationStatus.HasValue, x => x.app.Status == input.ApplicationStatus)
            .WhereIf(input.GradeCategoryId.HasValue, x => x.app.RequestedGradeCategoryId == input.GradeCategoryId)
            .WhereIf(input.DateFrom.HasValue, x => x.app.CreationTime >= input.DateFrom!.Value.Date)
            .WhereIf(input.DateTo.HasValue, x => x.app.CreationTime <= input.DateTo!.Value.Date.AddDays(1).AddTicks(-1));

        var items = await AsyncExecuter.ToListAsync(
            query.OrderByDescending(x => x.app.CreationTime).Take(MaxReportRows));

        var columns = Columns(
            ("childName", "Portal:Reports:Col:ChildName"),
            ("parentName", "Portal:Reports:Col:ParentName"),
            ("phone", "Portal:Reports:Col:Phone"),
            ("grade", "Portal:Reports:Col:Grade"),
            ("status", "Portal:Reports:Col:Status"),
            ("submitted", "Portal:Reports:Col:SubmittedDate"));

        var rows = items.Select(x => new List<string>
        {
            x.app.ChildFullName,
            x.app.ParentFullName,
            x.app.ParentPhoneNumber,
            x.grade?.Name ?? "-",
            x.app.Status.ToString(),
            FormatDateTime(x.app.CreationTime),
        }).ToList();

        return BuildResult(
            PortalReportType.Applications,
            "Portal:Reports:Type:Applications",
            branchName,
            input,
            columns,
            rows);
    }

    private async Task<PortalReportResultDto> GenerateAttendanceReportAsync(PortalReportInput input, string branchName)
    {
        var dateFrom = input.DateFrom.HasValue
            ? DateOnly.FromDateTime(input.DateFrom.Value.Date)
            : DateOnly.FromDateTime(Clock.Now.Date.AddDays(-30));
        var dateTo = input.DateTo.HasValue
            ? DateOnly.FromDateTime(input.DateTo.Value.Date)
            : DateOnly.FromDateTime(Clock.Now.Date);

        if (dateTo < dateFrom)
        {
            throw new BusinessException("NurseryHub:Reports:InvalidDateRange");
        }

        var students = await _studentRepository.GetQueryableAsync();
        var classes = await _classRepository.GetQueryableAsync();
        var attendances = await _attendanceRepository.GetQueryableAsync();

        var studentQuery = students
            .Where(s => s.NurseryBranchId == input.NurseryBranchId && s.IsActive)
            .WhereIf(input.NurseryClassId.HasValue, s => s.NurseryClassId == input.NurseryClassId);

        var studentList = await AsyncExecuter.ToListAsync(studentQuery);
        if (studentList.Count == 0)
        {
            return BuildResult(
                PortalReportType.Attendance,
                "Portal:Reports:Type:Attendance",
                branchName,
                input,
                Columns(
                    ("date", "Portal:Reports:Col:Date"),
                    ("student", "Portal:Reports:Col:StudentName"),
                    ("className", "Portal:Reports:Col:Class"),
                    ("status", "Portal:Reports:Col:Status"),
                    ("checkIn", "Portal:Reports:Col:CheckIn"),
                    ("checkOut", "Portal:Reports:Col:CheckOut")),
                new List<List<string>>());
        }

        var studentIds = studentList.Select(s => s.Id).ToList();
        var classById = (await AsyncExecuter.ToListAsync(classes.Where(c => c.NurseryBranchId == input.NurseryBranchId)))
            .ToDictionary(c => c.Id, c => c.Name);

        var attendanceList = await AsyncExecuter.ToListAsync(
            attendances.Where(a =>
                studentIds.Contains(a.StudentId) &&
                a.Date >= dateFrom &&
                a.Date <= dateTo));

        if (input.AttendanceStatus.HasValue)
        {
            attendanceList = attendanceList
                .Where(a => a.Status == input.AttendanceStatus.Value)
                .ToList();
        }

        var studentById = studentList.ToDictionary(s => s.Id);
        var columns = Columns(
            ("date", "Portal:Reports:Col:Date"),
            ("student", "Portal:Reports:Col:StudentName"),
            ("className", "Portal:Reports:Col:Class"),
            ("status", "Portal:Reports:Col:Status"),
            ("checkIn", "Portal:Reports:Col:CheckIn"),
            ("checkOut", "Portal:Reports:Col:CheckOut"));

        var rows = attendanceList
            .OrderByDescending(a => a.Date)
            .ThenBy(a => studentById.TryGetValue(a.StudentId, out var s) ? s.FullName : string.Empty)
            .Take(MaxReportRows)
            .Select(a =>
            {
                studentById.TryGetValue(a.StudentId, out var student);
                var className = student?.NurseryClassId.HasValue == true &&
                                classById.TryGetValue(student.NurseryClassId.Value, out var cn)
                    ? cn
                    : "-";
                return new List<string>
                {
                    FormatDateOnly(a.Date),
                    student?.FullName ?? "-",
                    className,
                    a.Status.ToString(),
                    a.CheckedInAt.HasValue ? FormatDateTime(a.CheckedInAt.Value) : "-",
                    a.CheckedOutAt.HasValue ? FormatDateTime(a.CheckedOutAt.Value) : "-",
                };
            })
            .ToList();

        return BuildResult(
            PortalReportType.Attendance,
            "Portal:Reports:Type:Attendance",
            branchName,
            input,
            columns,
            rows);
    }

    private async Task<PortalReportResultDto> GenerateClassesReportAsync(PortalReportInput input, string branchName)
    {
        var classes = await _classRepository.GetQueryableAsync();
        var grades = await _gradeRepository.GetQueryableAsync();
        var students = await _studentRepository.GetQueryableAsync();

        var query =
            from cls in classes
            join grade in grades on cls.GradeCategoryId equals grade.Id into gradeJoin
            from grade in gradeJoin.DefaultIfEmpty()
            where cls.NurseryBranchId == input.NurseryBranchId
            select new { cls, grade };

        query = query
            .WhereIf(input.GradeCategoryId.HasValue, x => x.cls.GradeCategoryId == input.GradeCategoryId)
            .WhereIf(input.IsActive.HasValue, x => x.cls.IsActive == input.IsActive!.Value);

        var items = await AsyncExecuter.ToListAsync(query.OrderBy(x => x.cls.Name).Take(MaxReportRows));
        var studentCounts = await AsyncExecuter.ToListAsync(
            students
                .Where(s => s.NurseryBranchId == input.NurseryBranchId && s.IsActive && s.NurseryClassId.HasValue)
                .GroupBy(s => s.NurseryClassId!.Value)
                .Select(g => new { ClassId = g.Key, Count = g.Count() }));

        var countByClass = studentCounts.ToDictionary(x => x.ClassId, x => x.Count);

        var columns = Columns(
            ("name", "Portal:Reports:Col:ClassName"),
            ("grade", "Portal:Reports:Col:Grade"),
            ("capacity", "Portal:Reports:Col:Capacity"),
            ("enrolled", "Portal:Reports:Col:Enrolled"),
            ("status", "Portal:Reports:Col:Status"));

        var rows = items.Select(x =>
        {
            countByClass.TryGetValue(x.cls.Id, out var enrolled);
            return new List<string>
            {
                x.cls.Name,
                x.grade?.Name ?? "-",
                x.cls.Capacity.ToString(),
                enrolled.ToString(),
                x.cls.IsActive ? "Active" : "Inactive",
            };
        }).ToList();

        return BuildResult(
            PortalReportType.Classes,
            "Portal:Reports:Type:Classes",
            branchName,
            input,
            columns,
            rows);
    }

    private async Task<PortalReportResultDto> GenerateGradesReportAsync(PortalReportInput input, string branchName)
    {
        var grades = await _gradeRepository.GetQueryableAsync();
        var classes = await _classRepository.GetQueryableAsync();

        var query = grades
            .Where(g => g.NurseryBranchId == input.NurseryBranchId)
            .WhereIf(input.IsActive.HasValue, g => g.IsActive == input.IsActive!.Value);

        var items = await AsyncExecuter.ToListAsync(query.OrderBy(g => g.Name).Take(MaxReportRows));
        var classCounts = await AsyncExecuter.ToListAsync(
            classes
                .Where(c => c.NurseryBranchId == input.NurseryBranchId && c.GradeCategoryId.HasValue)
                .GroupBy(c => c.GradeCategoryId!.Value)
                .Select(g => new { GradeId = g.Key, Count = g.Count() }));

        var countByGrade = classCounts.ToDictionary(x => x.GradeId, x => x.Count);

        var columns = Columns(
            ("name", "Portal:Reports:Col:GradeName"),
            ("classes", "Portal:Reports:Col:ClassCount"),
            ("status", "Portal:Reports:Col:Status"),
            ("description", "Portal:Reports:Col:Description"));

        var rows = items.Select(x =>
        {
            countByGrade.TryGetValue(x.Id, out var classCount);
            return new List<string>
            {
                x.Name,
                classCount.ToString(),
                x.IsActive ? "Active" : "Inactive",
                string.IsNullOrWhiteSpace(x.Description) ? "-" : x.Description,
            };
        }).ToList();

        return BuildResult(
            PortalReportType.Grades,
            "Portal:Reports:Type:Grades",
            branchName,
            input,
            columns,
            rows);
    }

    private async Task<PortalReportResultDto> GeneratePaymentsReportAsync(PortalReportInput input, string branchName)
    {
        var payments = await _paymentRepository.GetQueryableAsync();
        var students = await _studentRepository.GetQueryableAsync();
        var parents = await _parentContactRepository.GetQueryableAsync();
        var classes = await _classRepository.GetQueryableAsync();
        var grades = await _gradeRepository.GetQueryableAsync();

        var branchClassIds = await AsyncExecuter.ToListAsync(
            classes.Where(c => c.NurseryBranchId == input.NurseryBranchId).Select(c => c.Id));

        var filtered = payments
            .Where(p => branchClassIds.Contains(p.ClassId))
            .WhereIf(input.PaymentMethod.HasValue, p => p.PaymentMethod == input.PaymentMethod)
            .WhereIf(input.DateFrom.HasValue, p => p.PaymentDate >= input.DateFrom!.Value.Date)
            .WhereIf(input.DateTo.HasValue, p => p.PaymentDate <= input.DateTo!.Value.Date.AddDays(1).AddTicks(-1));

        var query =
            from payment in filtered
            join student in students on payment.StudentId equals student.Id
            join parent in parents on student.ParentId equals parent.Id
            join cls in classes on payment.ClassId equals cls.Id into classJoin
            from cls in classJoin.DefaultIfEmpty()
            join grade in grades on payment.GradeId equals grade.Id into gradeJoin
            from grade in gradeJoin.DefaultIfEmpty()
            orderby payment.PaymentDate descending
            select new { payment, student, parent, cls, grade };

        var items = await AsyncExecuter.ToListAsync(query.Take(MaxReportRows));

        var columns = Columns(
            ("number", "Portal:Reports:Col:PaymentNumber"),
            ("student", "Portal:Reports:Col:StudentName"),
            ("grade", "Portal:Reports:Col:Grade"),
            ("className", "Portal:Reports:Col:Class"),
            ("amount", "Portal:Reports:Col:Amount"),
            ("method", "Portal:Reports:Col:PaymentMethod"),
            ("purpose", "Portal:Reports:Col:Purpose"),
            ("date", "Portal:Reports:Col:PaymentDate"));

        var rows = items.Select(x => new List<string>
        {
            x.payment.PaymentNumber,
            x.student.FullName,
            x.grade?.Name ?? "-",
            x.cls?.Name ?? "-",
            x.payment.Amount.ToString("N2"),
            x.payment.PaymentMethod.ToString(),
            string.IsNullOrWhiteSpace(x.payment.CustomPaymentPurpose)
                ? x.payment.PaymentPurpose.ToString()
                : x.payment.CustomPaymentPurpose,
            FormatDateTime(x.payment.PaymentDate),
        }).ToList();

        return BuildResult(
            PortalReportType.Payments,
            "Portal:Reports:Type:Payments",
            branchName,
            input,
            columns,
            rows);
    }

    private async Task<PortalReportResultDto> GenerateExpensesReportAsync(PortalReportInput input, string branchName)
    {
        var expenses = await _expenseRepository.GetQueryableAsync();

        var query = expenses
            .Where(x => x.NurseryBranchId == input.NurseryBranchId)
            .WhereIf(input.ExpenseCategory.HasValue, x => x.ExpenseCategory == input.ExpenseCategory)
            .WhereIf(input.DateFrom.HasValue, x => x.ExpenseDate >= input.DateFrom!.Value.Date)
            .WhereIf(input.DateTo.HasValue, x => x.ExpenseDate <= input.DateTo!.Value.Date.AddDays(1).AddTicks(-1))
            .OrderByDescending(x => x.ExpenseDate);

        var items = await AsyncExecuter.ToListAsync(query.Take(MaxReportRows));

        var columns = Columns(
            ("number", "Portal:Reports:Col:ExpenseNumber"),
            ("title", "Portal:Reports:Col:Title"),
            ("category", "Portal:Reports:Col:Category"),
            ("vendor", "Portal:Reports:Col:Vendor"),
            ("amount", "Portal:Reports:Col:Amount"),
            ("status", "Portal:Reports:Col:PaymentStatus"),
            ("date", "Portal:Reports:Col:ExpenseDate"));

        var rows = items.Select(x => new List<string>
        {
            x.ExpenseNumber,
            x.Title,
            string.IsNullOrWhiteSpace(x.CustomExpenseCategory)
                ? x.ExpenseCategory.ToString()
                : x.CustomExpenseCategory,
            x.VendorName ?? "-",
            x.Amount.ToString("N2"),
            x.PaymentStatus.ToString(),
            FormatDateTime(x.ExpenseDate),
        }).ToList();

        return BuildResult(
            PortalReportType.Expenses,
            "Portal:Reports:Type:Expenses",
            branchName,
            input,
            columns,
            rows);
    }

    private async Task<PortalReportResultDto> GenerateStaffReportAsync(PortalReportInput input, string branchName)
    {
        var users = (await _identityUserRepository.GetListAsync())
            .Where(x => x.TenantId == CurrentTenant.Id)
            .ToList();
        var branches = await _branchRepository.GetListAsync(x => x.TenantId == CurrentTenant.Id);
        var userBranches = (await _userBranchRepository.GetListAsync())
            .Where(x => x.TenantId == CurrentTenant.Id)
            .ToList();
        var branchById = branches.ToDictionary(x => x.Id, x => x.Name);
        var branchIdsByUser = userBranches
            .GroupBy(x => x.UserId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.NurseryBranchId).Distinct().ToList());

        var columns = Columns(
            ("name", "Portal:Reports:Col:EmployeeName"),
            ("email", "Portal:Reports:Col:Email"),
            ("phone", "Portal:Reports:Col:Phone"),
            ("role", "Portal:Reports:Col:Role"),
            ("branches", "Portal:Reports:Col:Branches"),
            ("status", "Portal:Reports:Col:Status"),
            ("joined", "Portal:Reports:Col:JoinedDate"));

        var rows = new List<List<string>>();
        foreach (var user in users.OrderBy(u => u.Name))
        {
            if (rows.Count >= MaxReportRows)
            {
                break;
            }

            if (!branchIdsByUser.TryGetValue(user.Id, out var assignedIds) ||
                !assignedIds.Contains(input.NurseryBranchId))
            {
                continue;
            }

            var role = await ResolveEmployeeRoleAsync(user);
            var status = user.IsActive ? EmployeeStatus.Active : EmployeeStatus.Inactive;
            if (input.EmployeeStatus.HasValue && status != input.EmployeeStatus.Value)
            {
                continue;
            }

            var branchNames = assignedIds
                .Where(branchById.ContainsKey)
                .Select(id => branchById[id])
                .OrderBy(n => n)
                .ToList();

            rows.Add(new List<string>
            {
                $"{user.Name} {user.Surname}".Trim(),
                user.Email ?? "-",
                user.PhoneNumber ?? "-",
                role.ToString(),
                string.Join(", ", branchNames),
                status.ToString(),
                FormatDateTime(user.CreationTime),
            });
        }

        return BuildResult(
            PortalReportType.Staff,
            "Portal:Reports:Type:Staff",
            branchName,
            input,
            columns,
            rows);
    }

    private async Task<PortalReportResultDto> GenerateNotificationsReportAsync(PortalReportInput input, string branchName)
    {
        var notifications = await _notificationRepository.GetQueryableAsync();
        var recipients = await _notificationRecipientRepository.GetQueryableAsync();

        var query =
            from n in notifications
            where n.BranchId == input.NurseryBranchId
            where n.AudienceType != NotificationAudienceType.ParentToNursery
            select n;

        query = query
            .WhereIf(input.NotificationType.HasValue, n => n.NotificationType == input.NotificationType)
            .WhereIf(input.DateFrom.HasValue, n => n.SentDate >= input.DateFrom!.Value.Date)
            .WhereIf(input.DateTo.HasValue, n => n.SentDate <= input.DateTo!.Value.Date.AddDays(1).AddTicks(-1));

        var notificationList = await AsyncExecuter.ToListAsync(
            query.OrderByDescending(n => n.SentDate).Take(MaxReportRows));

        var notificationIds = notificationList.Select(n => n.Id).ToList();
        var recipientStats = notificationIds.Count == 0
            ? new List<(Guid NotificationId, int Delivered)>()
            : (await AsyncExecuter.ToListAsync(
                recipients
                    .Where(r => notificationIds.Contains(r.NotificationId))
                    .GroupBy(r => r.NotificationId)
                    .Select(g => new
                    {
                        NotificationId = g.Key,
                        Delivered = g.Count(x => x.DeliveryStatus == NotificationDeliveryStatus.Delivered),
                    })))
                .Select(x => (x.NotificationId, x.Delivered))
                .ToList();

        var deliveredByNotification = recipientStats.ToDictionary(x => x.NotificationId, x => x.Delivered);
        var items = notificationList;

        var columns = Columns(
            ("title", "Portal:Reports:Col:Title"),
            ("type", "Portal:Reports:Col:Type"),
            ("audience", "Portal:Reports:Col:Audience"),
            ("status", "Portal:Reports:Col:Status"),
            ("recipients", "Portal:Reports:Col:Recipients"),
            ("delivered", "Portal:Reports:Col:Delivered"),
            ("sentDate", "Portal:Reports:Col:SentDate"));

        var rows = items.Select(x =>
        {
            deliveredByNotification.TryGetValue(x.Id, out var delivered);
            return new List<string>
            {
                x.Title,
                x.NotificationType.ToString(),
                x.AudienceType.ToString(),
                x.Status.ToString(),
                x.TotalRecipients.ToString(),
                delivered.ToString(),
                x.SentDate.HasValue ? FormatDateTime(x.SentDate.Value) : "-",
            };
        }).ToList();

        return BuildResult(
            PortalReportType.Notifications,
            "Portal:Reports:Type:Notifications",
            branchName,
            input,
            columns,
            rows);
    }

    private async Task<EmployeeRole> ResolveEmployeeRoleAsync(IdentityUser user)
    {
        var roles = await _identityUserManager.GetRolesAsync(user);
        if (roles.Contains(NurseryHubRoles.Accountant))
        {
            return EmployeeRole.Accountant;
        }

        if (roles.Contains(NurseryHubRoles.BranchManager))
        {
            return EmployeeRole.BranchManager;
        }

        return EmployeeRole.Teacher;
    }

    private static List<PortalReportColumnDto> Columns(params (string key, string labelKey)[] defs)
    {
        return defs
            .Select(d => new PortalReportColumnDto { Key = d.key, LabelKey = d.labelKey })
            .ToList();
    }

    private PortalReportResultDto BuildResult(
        PortalReportType reportType,
        string titleKey,
        string branchName,
        PortalReportInput input,
        List<PortalReportColumnDto> columns,
        List<List<string>> rows)
    {
        return new PortalReportResultDto
        {
            ReportType = reportType,
            TitleKey = titleKey,
            BranchName = branchName,
            Criteria = BuildCriteria(input, branchName),
            Columns = columns,
            Rows = rows,
            TotalRows = rows.Count,
        };
    }

    private static List<PortalReportCriteriaItemDto> BuildCriteria(PortalReportInput input, string branchName)
    {
        var criteria = new List<PortalReportCriteriaItemDto>
        {
            new()
            {
                LabelKey = "Portal:Reports:Criteria:Branch",
                Value = branchName,
            },
        };

        if (input.NurseryClassId.HasValue)
        {
            criteria.Add(new PortalReportCriteriaItemDto
            {
                LabelKey = "Portal:Reports:Criteria:Class",
                Value = input.NurseryClassId.Value.ToString(),
            });
        }

        if (input.GradeCategoryId.HasValue)
        {
            criteria.Add(new PortalReportCriteriaItemDto
            {
                LabelKey = "Portal:Reports:Criteria:Grade",
                Value = input.GradeCategoryId.Value.ToString(),
            });
        }

        if (input.IsActive.HasValue)
        {
            criteria.Add(new PortalReportCriteriaItemDto
            {
                LabelKey = "Portal:Reports:Criteria:Status",
                Value = input.IsActive.Value ? "Active" : "Inactive",
            });
        }

        if (input.DateFrom.HasValue)
        {
            criteria.Add(new PortalReportCriteriaItemDto
            {
                LabelKey = "Portal:Reports:Criteria:DateFrom",
                Value = FormatDateTime(input.DateFrom.Value.Date),
            });
        }

        if (input.DateTo.HasValue)
        {
            criteria.Add(new PortalReportCriteriaItemDto
            {
                LabelKey = "Portal:Reports:Criteria:DateTo",
                Value = FormatDateTime(input.DateTo.Value.Date),
            });
        }

        if (input.ApplicationStatus.HasValue)
        {
            criteria.Add(new PortalReportCriteriaItemDto
            {
                LabelKey = "Portal:Reports:Criteria:ApplicationStatus",
                Value = input.ApplicationStatus.Value.ToString(),
            });
        }

        if (input.AttendanceStatus.HasValue)
        {
            criteria.Add(new PortalReportCriteriaItemDto
            {
                LabelKey = "Portal:Reports:Criteria:AttendanceStatus",
                Value = input.AttendanceStatus.Value.ToString(),
            });
        }

        if (input.PaymentMethod.HasValue)
        {
            criteria.Add(new PortalReportCriteriaItemDto
            {
                LabelKey = "Portal:Reports:Criteria:PaymentMethod",
                Value = input.PaymentMethod.Value.ToString(),
            });
        }

        if (input.ExpenseCategory.HasValue)
        {
            criteria.Add(new PortalReportCriteriaItemDto
            {
                LabelKey = "Portal:Reports:Criteria:ExpenseCategory",
                Value = input.ExpenseCategory.Value.ToString(),
            });
        }

        if (input.NotificationType.HasValue)
        {
            criteria.Add(new PortalReportCriteriaItemDto
            {
                LabelKey = "Portal:Reports:Criteria:NotificationType",
                Value = input.NotificationType.Value.ToString(),
            });
        }

        if (input.EmployeeStatus.HasValue)
        {
            criteria.Add(new PortalReportCriteriaItemDto
            {
                LabelKey = "Portal:Reports:Criteria:EmployeeStatus",
                Value = input.EmployeeStatus.Value.ToString(),
            });
        }

        return criteria;
    }

    private static string FormatDateOnly(DateOnly date) => date.ToString("yyyy-MM-dd");

    private static string FormatDateTime(DateTime dateTime) => dateTime.ToString("yyyy-MM-dd HH:mm");
}
