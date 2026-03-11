using ArNir.Core.DTOs.Chat;
using System.Threading.Tasks;

namespace ArNir.Services.Interfaces
{
    public interface IChatInsightService
    {
        /// <summary>
        /// Processes a user query, performing semantic recall, insight generation, 
        /// chart building, and contextual action extraction.
        /// </summary>
        Task<ChatResponseDto> ProcessUserQueryAsync(ChatQueryDto query);

        /// <summary>
        /// Retrieves previous session context and chat messages.
        /// </summary>
        Task<object> GetSessionContextAsync(string sessionId);

        /// <summary>
        /// Executes contextual backend actions triggered by AI suggestions.
        /// </summary>
        Task<object?> ExecuteActionAsync(string action);
    }
}
