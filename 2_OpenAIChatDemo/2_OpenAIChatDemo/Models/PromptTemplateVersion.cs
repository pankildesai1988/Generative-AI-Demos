namespace _2_OpenAIChatDemo.Models
{
    // Models/PromptTemplateVersion.cs
    using System;

    namespace OpenAIChatDemo.Models
    {
        public class PromptTemplateVersion
        {
            public int Id { get; set; }
            public int TemplateId { get; set; }
            public int Version { get; set; }
            public string Name { get; set; }
            public string KeyName { get; set; }
            public string TemplateText { get; set; }
            public string ParametersJson { get; set; } // store parameters snapshot as JSON
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

            public PromptTemplate Template { get; set; }
        }
    }
}