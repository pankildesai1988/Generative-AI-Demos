// Services/PromptTemplateService.cs
using _2_OpenAIChatDemo.Data;
using _2_OpenAIChatDemo.DTOs;
using _2_OpenAIChatDemo.Models;
using _2_OpenAIChatDemo.Models.OpenAIChatDemo.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace _2_OpenAIChatDemo.Services
{
    public class PromptTemplateService : IPromptTemplateService
    {
        private readonly ChatDbContext _context;

        public PromptTemplateService(ChatDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PromptTemplateDto>> GetAllAsync()
        {
            return await _context.PromptTemplates
                .Where(t => t.IsActive)
                .Include(t => t.Parameters)
                .Select(t => new PromptTemplateDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    KeyName = t.KeyName,
                    TemplateText = t.TemplateText,
                    Version = t.Version,
                    IsActive = t.IsActive,
                    Parameters = t.Parameters.Select(p => new PromptTemplateParameterDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        KeyName = p.KeyName,
                        Options = p.Options,
                        DefaultValue = p.DefaultValue
                    })
                }).ToListAsync();
        }

        public async Task<PromptTemplateDto> GetByIdAsync(int id)
        {
            var t = await _context.PromptTemplates
                .Include(t => t.Parameters)
                .FirstOrDefaultAsync(t => t.Id == id && t.IsActive);

            if (t == null) return null;

            return new PromptTemplateDto
            {
                Id = t.Id,
                Name = t.Name,
                KeyName = t.KeyName,
                TemplateText = t.TemplateText,
                Version = t.Version,
                IsActive = t.IsActive,
                Parameters = t.Parameters.Select(p => new PromptTemplateParameterDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    KeyName = p.KeyName,
                    Options = p.Options,
                    DefaultValue = p.DefaultValue
                })
            };
        }

        public async Task<PromptTemplateDto> CreateAsync(PromptTemplateCreateDto dto)
        {
            var template = new PromptTemplate
            {
                Name = dto.Name,
                KeyName = dto.KeyName,
                TemplateText = dto.TemplateText,
                Version = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                Parameters = dto.Parameters?.Select(p => new PromptTemplateParameter
                {
                    Name = p.Name,
                    KeyName = p.KeyName,
                    Options = p.Options,
                    DefaultValue = p.DefaultValue
                }).ToList()
            };

            _context.PromptTemplates.Add(template);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(template.Id);
        }

        public async Task<PromptTemplateDto> UpdateAsync(int id, PromptTemplateUpdateDto dto)
        {
            var template = await _context.PromptTemplates
                .Include(t => t.Parameters)
                .FirstOrDefaultAsync(t => t.Id == id && t.IsActive);

            if (template == null) return null;

            // Save snapshot before updating
            var snapshot = new PromptTemplateVersion
            {
                TemplateId = template.Id,
                Version = template.Version,
                Name = template.Name,
                KeyName = template.KeyName,
                TemplateText = template.TemplateText,
                ParametersJson = JsonSerializer.Serialize(
                    template.Parameters.Select(p => new
                    {
                        p.Name,
                        p.KeyName,
                        p.Options,
                        p.DefaultValue
                    })
                ),
                CreatedAt = DateTime.UtcNow
            };
            _context.PromptTemplateVersions.Add(snapshot);

            // Update template
            template.Version += 1;
            template.Name = dto.Name;
            template.TemplateText = dto.TemplateText;
            template.UpdatedAt = DateTime.UtcNow;

            // Replace parameters
            _context.PromptTemplateParameters.RemoveRange(template.Parameters);
            template.Parameters = dto.Parameters?.Select(p => new PromptTemplateParameter
            {
                Name = p.Name,
                KeyName = p.KeyName,
                Options = p.Options,
                DefaultValue = p.DefaultValue
            }).ToList();

            await _context.SaveChangesAsync();
            return await GetByIdAsync(template.Id);
        }

        public async Task<IEnumerable<PromptTemplateDto>> GetVersionsAsync(int templateId)
        {
            var versions = await _context.PromptTemplateVersions
                .Where(v => v.TemplateId == templateId)
                .OrderByDescending(v => v.Version)
                .ToListAsync();

            return versions.Select(v => new PromptTemplateDto
            {
                Id = v.TemplateId,
                Name = v.Name,
                KeyName = v.KeyName,
                TemplateText = v.TemplateText,
                Version = v.Version,
                IsActive = true,
                Parameters = JsonSerializer.Deserialize<IEnumerable<PromptTemplateParameterDto>>(v.ParametersJson)
            });
        }

        public async Task<PromptTemplateDto> RollbackAsync(int templateId, int version)
        {
            var snapshot = await _context.PromptTemplateVersions
                .FirstOrDefaultAsync(v => v.TemplateId == templateId && v.Version == version);

            if (snapshot == null) return null;

            var template = await _context.PromptTemplates
                .Include(t => t.Parameters)
                .FirstOrDefaultAsync(t => t.Id == templateId);

            if (template == null) return null;

            // Save current as a new snapshot before rollback
            var rollbackSnapshot = new PromptTemplateVersion
            {
                TemplateId = template.Id,
                Version = template.Version,
                Name = template.Name,
                KeyName = template.KeyName,
                TemplateText = template.TemplateText,
                ParametersJson = JsonSerializer.Serialize(
                    template.Parameters.Select(p => new
                    {
                        p.Name,
                        p.KeyName,
                        p.Options,
                        p.DefaultValue
                    })
                ),
                CreatedAt = DateTime.UtcNow
            };
            _context.PromptTemplateVersions.Add(rollbackSnapshot);

            // Restore old data
            template.Version = snapshot.Version + 1; // bump version after rollback
            template.Name = snapshot.Name;
            template.KeyName = snapshot.KeyName;
            template.TemplateText = snapshot.TemplateText;
            template.UpdatedAt = DateTime.UtcNow;

            _context.PromptTemplateParameters.RemoveRange(template.Parameters);
            if (!string.IsNullOrEmpty(snapshot.ParametersJson))
            {
                var restoredParams = JsonSerializer.Deserialize<IEnumerable<PromptTemplateParameterDto>>(snapshot.ParametersJson);
                template.Parameters = restoredParams.Select(p => new PromptTemplateParameter
                {
                    Name = p.Name,
                    KeyName = p.KeyName,
                    Options = p.Options,
                    DefaultValue = p.DefaultValue
                }).ToList();
            }

            await _context.SaveChangesAsync();
            return await GetByIdAsync(template.Id);
        }

        public async Task<PromptPreviewResultDto> PreviewAsync(int templateId, PromptPreviewDto dto)
        {
            var template = await _context.PromptTemplates
                .Include(t => t.Parameters)
                .FirstOrDefaultAsync(t => t.Id == templateId && t.IsActive);

            if (template == null) return null;

            string rendered = template.TemplateText;

            foreach (var param in template.Parameters)
            {
                string key = $"{{{param.KeyName}}}";
                string value = dto.Parameters.ContainsKey(param.KeyName)
                    ? dto.Parameters[param.KeyName]
                    : param.DefaultValue ?? $"[{param.KeyName}]";

                // ✅ If Options exist, validate the provided value
                if (!string.IsNullOrEmpty(param.Options))
                {
                    var allowed = param.Options.Split(',', StringSplitOptions.TrimEntries);
                    if (!allowed.Contains(value))
                    {
                        value = param.DefaultValue ?? allowed.First();
                    }
                }

                rendered = rendered.Replace(key, value);
            }

            return new PromptPreviewResultDto { RenderedPrompt = rendered };
        }


        public async Task<bool> SoftDeleteAsync(int id)
        {
            var template = await _context.PromptTemplates.FindAsync(id);
            if (template == null) return false;

            template.IsActive = false;
            template.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
