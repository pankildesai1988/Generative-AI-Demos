namespace _2_OpenAIChatFrontend.Areas.Admin.Services
{
    public interface IPromptTemplateService
    {
        Task<IEnumerable<PromptTemplateDto>> GetAllAsync();
        Task<PromptTemplateDto> GetByIdAsync(int id);
        Task<PromptTemplateDto> CreateAsync(PromptTemplateCreateDto dto);
        Task<PromptTemplateDto> UpdateAsync(int id, PromptTemplateUpdateDto dto);
        Task<bool> SoftDeleteAsync(int id);

        // Version history
        Task<IEnumerable<PromptTemplateDto>> GetVersionsAsync(int templateId);
        Task<PromptTemplateDto> RollbackAsync(int templateId, int version);
    }
}
