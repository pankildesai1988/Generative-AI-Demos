using _2_OpenAIChatDemo.DTOs;
using _2_OpenAIChatDemo.Models;

namespace _2_OpenAIChatDemo.Services
{
    public interface IPromptTemplateService
    {
        Task<IEnumerable<PromptTemplateDto>> GetAllAsync();
        Task<PromptTemplateDto> GetByIdAsync(int id);
        Task<PromptTemplateDto> CreateAsync(PromptTemplateCreateDto dto);
        Task<PromptTemplateDto> UpdateAsync(int id, PromptTemplateUpdateDto dto);
        Task<bool> SoftDeleteAsync(int id);
        Task<IEnumerable<PromptTemplateVersionDto>> GetVersionsAsync(int templateId);
        Task<PromptTemplateDto> RollbackAsync(int templateId, int version);
        Task<PromptPreviewResultDto> PreviewAsync(int templateId, PromptPreviewDto dto);
    }
}
