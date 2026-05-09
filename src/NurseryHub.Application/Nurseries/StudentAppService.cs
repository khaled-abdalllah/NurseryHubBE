using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using NurseryHub.Permissions;
using NurseryHub.Security;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.TenantManagement;
using Volo.Abp.Identity;
using Volo.Abp.Threading;

namespace NurseryHub.Nurseries;

[Authorize(Roles = $"{NurseryHubRoles.Admin},{NurseryHubRoles.NurseryAdmin}")]
public class StudentAppService
    : CrudAppService<
            Student,
            StudentDto,
            Guid,
            GetStudentsInput,
            CreateUpdateStudentDto,
            CreateUpdateStudentDto>,
        IStudentAppService
{
    private readonly IRepository<NurseryBranch, Guid> _nurseryBranchRepository;
    private readonly IRepository<NurseryClass, Guid> _nurseryClassRepository;
    private readonly IRepository<ParentContact, Guid> _parentContactRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly NurseryMediaOptions _mediaOptions;
    private readonly IRepository<ParentStudent, Guid> _parentStudentRepository;
    private readonly IIdentityUserRepository _identityUserRepository;
    private readonly IIdentityRoleRepository _identityRoleRepository;
    private readonly IdentityRoleManager _identityRoleManager;
    private readonly IdentityUserManager _identityUserManager;

    public StudentAppService(
        IRepository<Student, Guid> repository,
        IRepository<NurseryBranch, Guid> nurseryBranchRepository,
        IRepository<NurseryClass, Guid> nurseryClassRepository,
        IRepository<ParentContact, Guid> parentContactRepository,
        IRepository<ParentStudent, Guid> parentStudentRepository,
        IIdentityUserRepository identityUserRepository,
        IIdentityRoleRepository identityRoleRepository,
        IdentityRoleManager identityRoleManager,
        IdentityUserManager identityUserManager,
        ITenantRepository tenantRepository,
        IOptions<NurseryMediaOptions> mediaOptions) : base(repository)
    {
        _nurseryBranchRepository = nurseryBranchRepository;
        _nurseryClassRepository = nurseryClassRepository;
        _parentContactRepository = parentContactRepository;
        _parentStudentRepository = parentStudentRepository;
        _identityUserRepository = identityUserRepository;
        _identityRoleRepository = identityRoleRepository;
        _identityRoleManager = identityRoleManager;
        _identityUserManager = identityUserManager;
        _tenantRepository = tenantRepository;
        _mediaOptions = mediaOptions.Value;
    }

    public override async Task<PagedResultDto<StudentDto>> GetListAsync(GetStudentsInput input)
    {
        var students = await Repository.GetQueryableAsync();
        var classes = await _nurseryClassRepository.GetQueryableAsync();
        var parents = await _parentContactRepository.GetQueryableAsync();
        var tenantName = CurrentTenant.Name;

        var filtered = from student in students
            join parent in parents on student.ParentId equals parent.Id
            select new { Student = student, Parent = parent };

        filtered = filtered
            .WhereIf(input.NurseryBranchId.HasValue, x => x.Student.NurseryBranchId == input.NurseryBranchId)
            .WhereIf(input.NurseryClassId.HasValue, x => x.Student.NurseryClassId == input.NurseryClassId)
            .WhereIf(
                !input.Filter.IsNullOrWhiteSpace(),
                x =>
                    x.Student.FullName.Contains(input.Filter!) ||
                    x.Parent.FatherName.Contains(input.Filter!) ||
                    x.Parent.MotherName.Contains(input.Filter!) ||
                    x.Parent.FatherPhoneNumber.Contains(input.Filter!) ||
                    x.Parent.MotherPhoneNumber.Contains(input.Filter!));

        var query =
            from row in filtered
            join nurseryClass in classes on row.Student.NurseryClassId equals nurseryClass.Id into classJoin
            from nurseryClass in classJoin.DefaultIfEmpty()
            let student = row.Student
            let parent = row.Parent
            select new
            {
                student.Id,
                student.NurseryBranchId,
                student.NurseryClassId,
                student.ParentId,
                NurseryClassName = nurseryClass != null ? nurseryClass.Name : null,
                student.FullName,
                student.BirthDate,
                student.Gender,
                student.BloodType,
                student.Religion,
                student.HomeAddress,
                student.WeightKg,
                parent.FatherName,
                parent.FatherIdentityNumber,
                parent.FatherPhoneNumber,
                parent.MotherName,
                parent.MotherIdentityNumber,
                parent.MotherPhoneNumber,
                student.EmergencyContactNumber,
                student.EnrollmentDate,
                student.HealthNotes,
                student.DietaryRestrictions,
                student.ToiletTrainingStatus,
                student.AttendsSunday,
                student.AttendsMonday,
                student.AttendsTuesday,
                student.AttendsWednesday,
                student.AttendsThursday,
                student.AttendsFriday,
                student.AttendsSaturday,
                student.MedicalNotes,
                student.AllergyNotes,
                student.ProfileImageFileName,
                student.IsActive,
                student.CreationTime,
                student.CreatorId,
                student.LastModificationTime,
                student.LastModifierId,
                student.IsDeleted,
                student.DeleterId,
                student.DeletionTime,
            };

        var totalCount = await AsyncExecuter.CountAsync(query);
        var rawItems = await AsyncExecuter.ToListAsync(
            query.OrderBy(x => x.FullName).Skip(input.SkipCount).Take(input.MaxResultCount));

        var items = rawItems.Select(x => new StudentDto
        {
            Id = x.Id,
            NurseryBranchId = x.NurseryBranchId,
            NurseryClassId = x.NurseryClassId,
            NurseryClassName = x.NurseryClassName,
            ParentId = x.ParentId,
            FullName = x.FullName,
            BirthDate = x.BirthDate,
            Gender = x.Gender,
            BloodType = x.BloodType,
            Religion = x.Religion,
            HomeAddress = x.HomeAddress,
            WeightKg = x.WeightKg,
            FatherName = x.FatherName,
            FatherIdentityNumber = x.FatherIdentityNumber,
            FatherPhoneNumber = x.FatherPhoneNumber,
            MotherName = x.MotherName,
            MotherIdentityNumber = x.MotherIdentityNumber,
            MotherPhoneNumber = x.MotherPhoneNumber,
            EmergencyContactNumber = x.EmergencyContactNumber,
            EnrollmentDate = x.EnrollmentDate,
            HealthNotes = x.HealthNotes,
            DietaryRestrictions = x.DietaryRestrictions,
            ToiletTrainingStatus = x.ToiletTrainingStatus,
            AttendsSunday = x.AttendsSunday,
            AttendsMonday = x.AttendsMonday,
            AttendsTuesday = x.AttendsTuesday,
            AttendsWednesday = x.AttendsWednesday,
            AttendsThursday = x.AttendsThursday,
            AttendsFriday = x.AttendsFriday,
            AttendsSaturday = x.AttendsSaturday,
            MedicalNotes = x.MedicalNotes,
            AllergyNotes = x.AllergyNotes,
            ProfileImageFileName = x.ProfileImageFileName,
            ProfileImageUrl = ResolveStudentImageDisplayUrl(x.ProfileImageFileName, tenantName),
            IsActive = x.IsActive,
            CreationTime = x.CreationTime,
            CreatorId = x.CreatorId,
            LastModificationTime = x.LastModificationTime,
            LastModifierId = x.LastModifierId,
            IsDeleted = x.IsDeleted,
            DeleterId = x.DeleterId,
            DeletionTime = x.DeletionTime,
        }).ToList();

        return new PagedResultDto<StudentDto>(totalCount, items);
    }

    public override async Task<StudentDto> CreateAsync(CreateUpdateStudentDto input)
    {
        await CheckCreatePolicyAsync();
        TrimCreateUpdateStudentDto(input);
        var tenantId = await ResolveTenantIdForCreationAsync(input.NurseryBranchId);
        await ValidateParentContactNoConflictsAsync(input, tenantId, input.ExistingParentContactId);
        ValidateParentPortalRequest(input);
        var entity = await MapToEntityAsync(input);
        entity.SetIsActive(true);
        await Repository.InsertAsync(entity, autoSave: true);
        await CreateParentPortalAccountIfNeededAsync(input, entity);
        var parent = await _parentContactRepository.GetAsync(entity.ParentId);
        var dto = MapToGetOutputDto(entity, parent);
        dto.ProfileImageUrl = ResolveStudentImageDisplayUrl(dto.ProfileImageFileName, CurrentTenant.Name);
        return dto;
    }

    public override async Task<StudentDto> UpdateAsync(Guid id, CreateUpdateStudentDto input)
    {
        await CheckUpdatePolicyAsync();
        var entity = await GetEntityByIdAsync(id);
        if (!entity.IsActive && !input.IsActive)
        {
            throw new BusinessException("NurseryHub:Student:Inactive");
        }

        TrimCreateUpdateStudentDto(input);
        var tenantId = entity.TenantId ?? await ResolveTenantIdForCreationAsync(input.NurseryBranchId);
        await ValidateParentContactNoConflictsAsync(input, tenantId, entity.ParentId);
        ValidateParentPortalRequest(input);
        await MapToEntityAsync(input, entity);
        await Repository.UpdateAsync(entity, autoSave: true);
        await CreateParentPortalAccountIfNeededAsync(input, entity);
        var parent = await _parentContactRepository.GetAsync(entity.ParentId);
        var dto = MapToGetOutputDto(entity, parent);
        dto.ProfileImageUrl = ResolveStudentImageDisplayUrl(dto.ProfileImageFileName, CurrentTenant.Name);
        return dto;
    }

    public override async Task<StudentDto> GetAsync(Guid id)
    {
        var entity = await Repository.GetAsync(id);
        var parent = await _parentContactRepository.GetAsync(entity.ParentId);
        var nurseryClass = entity.NurseryClassId.HasValue
            ? await _nurseryClassRepository.FindAsync(entity.NurseryClassId.Value)
            : null;

        var dto = MapToGetOutputDto(entity, parent);
        dto.NurseryClassName = nurseryClass?.Name;
        dto.ProfileImageUrl = ResolveStudentImageDisplayUrl(dto.ProfileImageFileName, CurrentTenant.Name);
        return dto;
    }

    public virtual async Task<StudentDto> UploadImageAsync(Guid id, UploadStudentImageInput input)
    {
        var entity = await GetEntityByIdAsync(id);
        EnsureStudentIsActive(entity);
        var file = input.File;
        var ext = Path.GetExtension(file.FileName)!.ToLowerInvariant();
        var tenantName = await GetTenantNameAsync(entity.TenantId);
        var directory = GetStudentDirectoryPath(tenantName);
        Directory.CreateDirectory(directory);

        DeleteStoredStudentImageIfExists(tenantName, entity.ProfileImageFileName);

        var fileName = $"{GuidGenerator.Create()}{ext}";
        var physicalPath = Path.Combine(directory, fileName);
        await using (var stream = File.Create(physicalPath))
        {
            await file.CopyToAsync(stream);
        }

        entity.SetProfileImageFileName(fileName);
        await Repository.UpdateAsync(entity, autoSave: true);

        var parent = await _parentContactRepository.GetAsync(entity.ParentId);
        var dto = MapToGetOutputDto(entity, parent);
        dto.ProfileImageUrl = ResolveStudentImageDisplayUrl(entity.ProfileImageFileName, tenantName);
        return dto;
    }

    public override async Task DeleteAsync(Guid id)
    {
        var entity = await Repository.GetAsync(id);
        EnsureStudentIsActive(entity);
        var parentId = entity.ParentId;
        var portalLinks = await _parentStudentRepository.GetListAsync(x => x.StudentId == id);
        if (portalLinks.Count > 0)
        {
            await _parentStudentRepository.DeleteManyAsync(portalLinks, autoSave: true);
        }

        await base.DeleteAsync(id);
        var otherStudentsWithSameParent = await Repository.CountAsync(s => s.ParentId == parentId && s.Id != id);
        if (otherStudentsWithSameParent == 0)
        {
            await _parentContactRepository.DeleteAsync(parentId);
        }
    }

    protected override async Task<Student> MapToEntityAsync(CreateUpdateStudentDto createInput)
    {
        await ValidateReferencesAsync(createInput.NurseryBranchId, createInput.NurseryClassId);
        var tenantId = await ResolveTenantIdForCreationAsync(createInput.NurseryBranchId);

        Guid parentId;
        if (createInput.ExistingParentContactId.HasValue)
        {
            parentId = createInput.ExistingParentContactId.Value;
            var existingParent = await _parentContactRepository.GetAsync(parentId);
            if (!TenantMatchesParent(tenantId, existingParent.TenantId))
            {
                throw new BusinessException("NurseryHub:Student:ParentContactTenantMismatch");
            }

            existingParent.SetFatherInfo(
                createInput.FatherName,
                createInput.FatherIdentityNumber,
                createInput.FatherPhoneNumber);
            existingParent.SetMotherInfo(
                createInput.MotherName,
                createInput.MotherIdentityNumber,
                createInput.MotherPhoneNumber);
            await _parentContactRepository.UpdateAsync(existingParent, autoSave: false);
        }
        else
        {
            parentId = GuidGenerator.Create();
            var parent = new ParentContact(
                parentId,
                tenantId,
                createInput.FatherName,
                createInput.FatherIdentityNumber ?? string.Empty,
                createInput.FatherPhoneNumber,
                createInput.MotherName ?? string.Empty,
                createInput.MotherIdentityNumber ?? string.Empty,
                createInput.MotherPhoneNumber ?? string.Empty);
            await _parentContactRepository.InsertAsync(parent, autoSave: false);
        }

        return new Student(
            GuidGenerator.Create(),
            tenantId,
            createInput.NurseryBranchId,
            parentId,
            createInput.FullName,
            createInput.BirthDate,
            createInput.Gender,
            createInput.BloodType,
            createInput.Religion,
            createInput.HomeAddress,
            createInput.EmergencyContactNumber,
            createInput.EnrollmentDate,
            createInput.HealthNotes,
            createInput.DietaryRestrictions,
            createInput.ToiletTrainingStatus,
            createInput.AttendsSunday,
            createInput.AttendsMonday,
            createInput.AttendsTuesday,
            createInput.AttendsWednesday,
            createInput.AttendsThursday,
            createInput.AttendsFriday,
            createInput.AttendsSaturday,
            createInput.MedicalNotes,
            createInput.AllergyNotes,
            createInput.WeightKg,
            profileImageFileName: null,
            createInput.IsActive,
            createInput.NurseryClassId);
    }

    protected override async Task MapToEntityAsync(CreateUpdateStudentDto updateInput, Student entity)
    {
        await ValidateReferencesAsync(updateInput.NurseryBranchId, updateInput.NurseryClassId);

        var parent = await _parentContactRepository.GetAsync(entity.ParentId);
        parent.SetFatherInfo(updateInput.FatherName, updateInput.FatherIdentityNumber, updateInput.FatherPhoneNumber);
        parent.SetMotherInfo(updateInput.MotherName, updateInput.MotherIdentityNumber, updateInput.MotherPhoneNumber);
        await _parentContactRepository.UpdateAsync(parent, autoSave: false);

        entity.SetClass(updateInput.NurseryClassId);
        entity.SetFullName(updateInput.FullName);
        entity.SetBirthDate(updateInput.BirthDate);
        entity.SetGender(updateInput.Gender);
        entity.SetBloodType(updateInput.BloodType);
        entity.SetReligion(updateInput.Religion);
        entity.SetHomeAddress(updateInput.HomeAddress);
        entity.SetEmergencyContactNumber(updateInput.EmergencyContactNumber);
        entity.SetEnrollmentDate(updateInput.EnrollmentDate);
        entity.SetHealthNotes(updateInput.HealthNotes);
        entity.SetDietaryRestrictions(updateInput.DietaryRestrictions);
        entity.SetToiletTrainingStatus(updateInput.ToiletTrainingStatus);
        entity.SetAttendingDays(
            updateInput.AttendsSunday,
            updateInput.AttendsMonday,
            updateInput.AttendsTuesday,
            updateInput.AttendsWednesday,
            updateInput.AttendsThursday,
            updateInput.AttendsFriday,
            updateInput.AttendsSaturday);
        entity.SetMedicalNotes(updateInput.MedicalNotes);
        entity.SetAllergyNotes(updateInput.AllergyNotes);
        entity.SetWeightKg(updateInput.WeightKg);
        entity.SetIsActive(updateInput.IsActive);

        await Task.CompletedTask;
    }

    private async Task ValidateReferencesAsync(Guid nurseryBranchId, Guid? nurseryClassId)
    {
        var branch = await _nurseryBranchRepository.FindAsync(nurseryBranchId);
        if (branch == null)
        {
            throw new BusinessException("NurseryHub:Student:BranchNotFound");
        }

        if (CurrentTenant.Id.HasValue && branch.TenantId != CurrentTenant.Id)
        {
            throw new BusinessException("NurseryHub:Student:BranchNotFound");
        }

        if (!nurseryClassId.HasValue)
        {
            return;
        }

        var nurseryClass = await _nurseryClassRepository.FindAsync(nurseryClassId.Value);
        if (nurseryClass == null || nurseryClass.NurseryBranchId != nurseryBranchId)
        {
            throw new BusinessException("NurseryHub:Student:ClassInvalidForBranch");
        }
    }

    private async Task<Guid> ResolveTenantIdForCreationAsync(Guid nurseryBranchId)
    {
        if (CurrentTenant.Id.HasValue)
        {
            return CurrentTenant.Id.Value;
        }

        var branch = await _nurseryBranchRepository.FindAsync(nurseryBranchId);
        if (branch?.TenantId.HasValue == true)
        {
            return branch.TenantId.Value;
        }

        throw new BusinessException("NurseryHub:Student:TenantIdRequired");
    }

    protected override StudentDto MapToGetOutputDto(Student entity)
    {
        return AsyncHelper.RunSync(async () =>
        {
            var parent = await _parentContactRepository.GetAsync(entity.ParentId);
            return MapToGetOutputDto(entity, parent);
        });
    }

    protected StudentDto MapToGetOutputDto(Student entity, ParentContact parent)
    {
        return new StudentDto
        {
            Id = entity.Id,
            NurseryBranchId = entity.NurseryBranchId,
            NurseryClassId = entity.NurseryClassId,
            ParentId = entity.ParentId,
            FullName = entity.FullName,
            BirthDate = entity.BirthDate,
            Gender = entity.Gender,
            BloodType = entity.BloodType,
            Religion = entity.Religion,
            HomeAddress = entity.HomeAddress,
            WeightKg = entity.WeightKg,
            FatherName = parent.FatherName,
            FatherIdentityNumber = parent.FatherIdentityNumber,
            FatherPhoneNumber = parent.FatherPhoneNumber,
            MotherName = parent.MotherName,
            MotherIdentityNumber = parent.MotherIdentityNumber,
            MotherPhoneNumber = parent.MotherPhoneNumber,
            EmergencyContactNumber = entity.EmergencyContactNumber,
            EnrollmentDate = entity.EnrollmentDate,
            HealthNotes = entity.HealthNotes,
            DietaryRestrictions = entity.DietaryRestrictions,
            ToiletTrainingStatus = entity.ToiletTrainingStatus,
            AttendsSunday = entity.AttendsSunday,
            AttendsMonday = entity.AttendsMonday,
            AttendsTuesday = entity.AttendsTuesday,
            AttendsWednesday = entity.AttendsWednesday,
            AttendsThursday = entity.AttendsThursday,
            AttendsFriday = entity.AttendsFriday,
            AttendsSaturday = entity.AttendsSaturday,
            MedicalNotes = entity.MedicalNotes,
            AllergyNotes = entity.AllergyNotes,
            ProfileImageFileName = entity.ProfileImageFileName,
            ProfileImageUrl = ResolveStudentImageDisplayUrl(entity.ProfileImageFileName, CurrentTenant.Name),
            IsActive = entity.IsActive,
            CreationTime = entity.CreationTime,
            CreatorId = entity.CreatorId,
            LastModificationTime = entity.LastModificationTime,
            LastModifierId = entity.LastModifierId,
            IsDeleted = entity.IsDeleted,
            DeleterId = entity.DeleterId,
            DeletionTime = entity.DeletionTime,
        };
    }

    private string GetStudentDirectoryPath(string? tenantName)
    {
        var safeTenantName = string.IsNullOrWhiteSpace(tenantName) ? "host" : tenantName;
        return Path.Combine(_mediaOptions.MediaRootPath, safeTenantName, "students");
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

    private void DeleteStoredStudentImageIfExists(string? tenantName, string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return;
        }

        var directory = GetStudentDirectoryPath(tenantName);
        var path = Path.Combine(directory, Path.GetFileName(fileName));
        if (File.Exists(path))
        {
            File.Delete(path);
        }
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

    private static void EnsureStudentIsActive(Student student)
    {
        if (!student.IsActive)
        {
            throw new BusinessException("NurseryHub:Student:Inactive");
        }
    }

    private static bool TenantMatchesParent(Guid expectedTenantId, Guid? parentTenantId)
    {
        return parentTenantId == expectedTenantId;
    }

    /// <summary>Trims all string fields and normalizes empty optional values to null.</summary>
    private static void TrimCreateUpdateStudentDto(CreateUpdateStudentDto input)
    {
        input.FullName = input.FullName.Trim();
        input.Gender = input.Gender.Trim();
        input.BloodType = NullIfWhiteSpace(input.BloodType);
        input.Religion = NullIfWhiteSpace(input.Religion);
        input.HomeAddress = NullIfWhiteSpace(input.HomeAddress);
        input.FatherName = input.FatherName.Trim();
        input.FatherIdentityNumber = NullIfWhiteSpace(input.FatherIdentityNumber);
        input.FatherPhoneNumber = input.FatherPhoneNumber.Trim();
        input.MotherName = NullIfWhiteSpace(input.MotherName);
        input.MotherIdentityNumber = NullIfWhiteSpace(input.MotherIdentityNumber);
        input.MotherPhoneNumber = NullIfWhiteSpace(input.MotherPhoneNumber);
        input.EmergencyContactNumber = input.EmergencyContactNumber.Trim();
        input.HealthNotes = NullIfWhiteSpace(input.HealthNotes);
        input.DietaryRestrictions = NullIfWhiteSpace(input.DietaryRestrictions);
        input.ToiletTrainingStatus = NullIfWhiteSpace(input.ToiletTrainingStatus);
        input.MedicalNotes = NullIfWhiteSpace(input.MedicalNotes);
        input.AllergyNotes = NullIfWhiteSpace(input.AllergyNotes);
    }

    private static string? NullIfWhiteSpace(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    /// <summary>
    /// Prevents reusing another household's phone or national id in the same tenant when creating/updating a parent row.
    /// </summary>
    private async Task ValidateParentContactNoConflictsAsync(
        CreateUpdateStudentDto input,
        Guid tenantId,
        Guid? excludeParentContactId)
    {
        var parents = await _parentContactRepository.GetQueryableAsync();

        var fatherPhone = input.FatherPhoneNumber ?? string.Empty;
        if (fatherPhone.Length > 0)
        {
            var q = parents.Where(p => p.TenantId == tenantId && p.FatherPhoneNumber == fatherPhone);
            if (excludeParentContactId.HasValue)
            {
                q = q.Where(p => p.Id != excludeParentContactId.Value);
            }

            if (await AsyncExecuter.AnyAsync(q))
            {
                throw new UserFriendlyException("رقم هاتف الأب مسجّل بالفعل لملف ولي آخر في النظام.");
            }
        }

        var motherPhone = input.MotherPhoneNumber ?? string.Empty;
        if (motherPhone.Length > 0)
        {
            var q = parents.Where(p => p.TenantId == tenantId && p.MotherPhoneNumber == motherPhone);
            if (excludeParentContactId.HasValue)
            {
                q = q.Where(p => p.Id != excludeParentContactId.Value);
            }

            if (await AsyncExecuter.AnyAsync(q))
            {
                throw new UserFriendlyException("رقم هاتف الأم مسجّل بالفعل لملف ولي آخر في النظام.");
            }
        }

        var fatherId = input.FatherIdentityNumber ?? string.Empty;
        if (fatherId.Length == 14)
        {
            var q = parents.Where(p => p.TenantId == tenantId && p.FatherIdentityNumber == fatherId);
            if (excludeParentContactId.HasValue)
            {
                q = q.Where(p => p.Id != excludeParentContactId.Value);
            }

            if (await AsyncExecuter.AnyAsync(q))
            {
                throw new UserFriendlyException("الرقم القومي للأب مسجّل بالفعل لولي آخر.");
            }
        }

        var motherId = input.MotherIdentityNumber ?? string.Empty;
        if (motherId.Length == 14)
        {
            var q = parents.Where(p => p.TenantId == tenantId && p.MotherIdentityNumber == motherId);
            if (excludeParentContactId.HasValue)
            {
                q = q.Where(p => p.Id != excludeParentContactId.Value);
            }

            if (await AsyncExecuter.AnyAsync(q))
            {
                throw new UserFriendlyException("الرقم القومي للأم مسجّل بالفعل لولي آخر.");
            }
        }
    }

    private static void ValidateParentPortalRequest(CreateUpdateStudentDto input)
    {
        if (!input.CreateParentPortalAccount)
        {
            return;
        }

        if (!input.ParentLoginUsernameSource.HasValue)
        {
            throw new BusinessException("NurseryHub:Student:ParentLoginSourceRequired");
        }

        var phone = ResolveLoginPhoneRaw(input, input.ParentLoginUsernameSource.Value);
        if (phone.IsNullOrWhiteSpace())
        {
            throw new BusinessException("NurseryHub:Student:ParentLoginPhoneRequired");
        }
    }

    private async Task CreateParentPortalAccountIfNeededAsync(CreateUpdateStudentDto input, Student entity)
    {
        if (!input.CreateParentPortalAccount || !input.ParentLoginUsernameSource.HasValue)
        {
            return;
        }

        var tenantId = entity.TenantId;
        if (!tenantId.HasValue)
        {
            throw new BusinessException("NurseryHub:Student:TenantIdRequiredForParentPortal");
        }

        var source = input.ParentLoginUsernameSource.Value;
        var userName = ResolveLoginPhoneRaw(input, source)?.Trim();
        if (userName.IsNullOrWhiteSpace())
        {
            throw new BusinessException("NurseryHub:Student:ParentLoginPhoneRequired");
        }

        var displayName = source == ParentPortalLoginUsernameSource.FatherPhone
            ? input.FatherName
            : input.MotherName;

        using (CurrentTenant.Change(tenantId.Value))
        {
            await EnsureParentRoleExistsAsync();
            var normalizedUserName = _identityUserManager.NormalizeName(userName);
            var existing = await _identityUserRepository.FindByNormalizedUserNameAsync(normalizedUserName);
            if (existing != null)
            {
                if (existing.TenantId != tenantId)
                {
                    throw new BusinessException("NurseryHub:Student:ParentLoginPhoneRequired");
                }

                await EnsureUserHasParentRoleAsync(existing);
                await EnsureParentStudentLinkAsync(existing.Id, entity.Id, tenantId);
                return;
            }

            var userId = GuidGenerator.Create();
            var (firstName, lastName) = SplitName(displayName);
            var email = BuildSyntheticParentEmail(userId);
            var user = new Volo.Abp.Identity.IdentityUser(userId, userName, email, tenantId)
            {
                Name = firstName,
                Surname = lastName,
            };
            user.SetIsActive(true);
            var createResult = await _identityUserManager.CreateAsync(user, password: userName);
            ThrowIfFailed(createResult);
            var phoneResult = await _identityUserManager.SetPhoneNumberAsync(user, userName);
            ThrowIfFailed(phoneResult);
            var addRoleResult = await _identityUserManager.AddToRoleAsync(user, NurseryHubRoles.Parent);
            ThrowIfFailed(addRoleResult);
            await EnsureParentStudentLinkAsync(user.Id, entity.Id, tenantId);
        }
    }

    private static string? ResolveLoginPhoneRaw(CreateUpdateStudentDto input, ParentPortalLoginUsernameSource source)
    {
        return source == ParentPortalLoginUsernameSource.FatherPhone
            ? input.FatherPhoneNumber
            : input.MotherPhoneNumber;
    }

    private async Task EnsureParentRoleExistsAsync()
    {
        var normalizedName = NurseryHubRoles.Parent.ToUpperInvariant();
        var role = await _identityRoleRepository.FindByNormalizedNameAsync(normalizedName);
        if (role != null)
        {
            return;
        }

        role = new IdentityRole(GuidGenerator.Create(), NurseryHubRoles.Parent, CurrentTenant.Id);
        var createRoleResult = await _identityRoleManager.CreateAsync(role);
        ThrowIfFailed(createRoleResult);
    }

    private async Task EnsureUserHasParentRoleAsync(Volo.Abp.Identity.IdentityUser user)
    {
        var roles = await _identityUserManager.GetRolesAsync(user);
        if (roles.Any(r => r.Equals(NurseryHubRoles.Parent, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        await EnsureParentRoleExistsAsync();
        var add = await _identityUserManager.AddToRoleAsync(user, NurseryHubRoles.Parent);
        ThrowIfFailed(add);
    }

    private async Task EnsureParentStudentLinkAsync(Guid parentUserId, Guid studentId, Guid? tenantId)
    {
        var exists = await _parentStudentRepository.AnyAsync(x =>
            x.ParentUserId == parentUserId && x.StudentId == studentId);
        if (exists)
        {
            return;
        }

        await _parentStudentRepository.InsertAsync(
            new ParentStudent(GuidGenerator.Create(), tenantId, parentUserId, studentId),
            autoSave: true);
    }

    private static string BuildSyntheticParentEmail(Guid userId)
    {
        return $"{userId:N}@parents.nurseryhub.local";
    }

    private static (string Name, string Surname) SplitName(string fullName)
    {
        var clean = fullName.Trim();
        if (clean.IsNullOrWhiteSpace())
        {
            return (string.Empty, string.Empty);
        }

        var parts = clean.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1)
        {
            return (parts[0], string.Empty);
        }

        return (parts[0], string.Join(' ', parts.Skip(1)));
    }

    private static void ThrowIfFailed(Microsoft.AspNetCore.Identity.IdentityResult result)
    {
        if (result.Succeeded)
        {
            return;
        }

        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
        throw new UserFriendlyException(errors);
    }
}
