using _2_OpenAIChatDemo.Data;
using _2_OpenAIChatDemo.Models;
using Microsoft.EntityFrameworkCore;

namespace _2_OpenAIChatDemo.Services
{
    public class TemplateService : ITemplateService
    {
        private readonly ChatDbContext _db;

        public TemplateService(ChatDbContext db)
        {
            _db = db;
        }

        public async Task<List<PromptTemplate>> GetTemplatesAsync()
        {
            return await _db.PromptTemplates
                .Include(t => t.Parameters)
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        public async Task<PromptTemplate?> GetTemplateAsync(int id)
        {
            return await _db.PromptTemplates
                .Include(t => t.Parameters)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<PromptTemplate> AddTemplateAsync(PromptTemplate template)
        {
            _db.PromptTemplates.Add(template);
            await _db.SaveChangesAsync();
            return template;
        }

        public async Task<PromptTemplate?> UpdateTemplateAsync(PromptTemplate template)
        {
            var existing = await _db.PromptTemplates
                .Include(t => t.Parameters)
                .FirstOrDefaultAsync(t => t.Id == template.Id);

            if (existing == null) return null;

            existing.Name = template.Name;
            existing.KeyName = template.KeyName;
            existing.TemplateText = template.TemplateText;

            // Replace parameters
            _db.PromptTemplateParameters.RemoveRange(existing.Parameters);
            existing.Parameters = template.Parameters;

            await _db.SaveChangesAsync();
            return existing;
        }

        public async Task DeleteTemplateAsync(int id)
        {
            var template = await _db.PromptTemplates.FindAsync(id);
            if (template != null)
            {
                _db.PromptTemplates.Remove(template);
                await _db.SaveChangesAsync();
            }
        }
    }
}
