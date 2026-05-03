using System;
using Volo.Abp.Application.Services;

namespace NurseryHub.Nurseries;

public interface IGradeCategoryAppService
    : ICrudAppService<
        GradeCategoryDto,
        Guid,
        GetGradeCategoriesInput,
        CreateUpdateGradeCategoryDto,
        CreateUpdateGradeCategoryDto>
{
}
