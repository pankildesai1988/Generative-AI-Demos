using _2_OpenAIChatDemo.Models;

namespace _2_OpenAIChatDemo.Services
{
    public interface ITemplateService
    {
        Task<List<PromptTemplate>> GetTemplatesAsync();
        Task<PromptTemplate?> GetTemplateAsync(int id);
        Task<PromptTemplate> AddTemplateAsync(PromptTemplate template);
        Task<PromptTemplate?> UpdateTemplateAsync(PromptTemplate template);
        Task DeleteTemplateAsync(int id);
    }
}
