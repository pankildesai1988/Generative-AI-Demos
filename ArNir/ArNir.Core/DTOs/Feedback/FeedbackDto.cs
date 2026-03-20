using System.Text.Json.Serialization;

namespace ArNir.Core.DTOs.Feedback
{
    public class FeedbackDto
    {
        public int HistoryId { get; set; }      // Link to RagComparisonHistory
        public int Rating { get; set; }         // 1–5 stars
        public string? Comments { get; set; }

        /// <summary>
        /// Alias for <see cref="Comments"/> — accepts "comment" (singular) from React demo frontends
        /// while keeping backward compatibility with Admin controllers that use "Comments" (plural).
        /// </summary>
        [JsonPropertyName("comment")]
        public string? Comment { get => Comments; set => Comments = value; }
    }
}
