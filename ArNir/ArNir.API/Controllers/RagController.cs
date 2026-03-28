using ArNir.Core.DTOs.RAG;
using ArNir.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ArNir.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RagController : ControllerBase
    {
        private readonly IRagService _ragService;

        public RagController(IRagService ragService) => _ragService = ragService;

        [HttpPost("run")]
        public async Task<IActionResult> Run([FromBody] RagRequestDto dto)
        {
            var result = await _ragService.RunRagAsync(
                dto.Query,
                dto.TopK,
                dto.UseHybrid,
                dto.PromptStyle,
                dto.SaveAsNew,
                dto.Provider,
                dto.Model,
                dto.DocumentIds);

            return Ok(result);
        }

        [HttpGet("stream")]
        [Produces("text/event-stream")]
        public async Task Stream([FromQuery] RagRequestDto dto, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(dto.Query))
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
                await Response.WriteAsJsonAsync(new { message = "Query is required." }, cancellationToken);
                return;
            }

            Response.StatusCode = StatusCodes.Status200OK;
            Response.ContentType = "text/event-stream";
            Response.Headers.Append("Cache-Control", "no-cache");
            Response.Headers.Append("Connection", "keep-alive");

            try
            {
                var result = await _ragService.RunRagAsync(
                    dto.Query,
                    dto.TopK,
                    dto.UseHybrid,
                    dto.PromptStyle,
                    dto.SaveAsNew,
                    dto.Provider,
                    dto.Model,
                    dto.DocumentIds);

                foreach (var token in ChunkAnswer(result.RagAnswer))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await WriteSseEventAsync("token", new { text = token }, cancellationToken);
                    await Task.Delay(15, cancellationToken);
                }

                await WriteSseEventAsync(
                    "metadata",
                    new
                    {
                        historyId = result.HistoryId,
                        retrievedChunks = result.RetrievedChunks,
                    },
                    cancellationToken);

                await WriteSseEventAsync("complete", new { done = true }, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // Client disconnected or aborted the request.
            }
            catch (Exception ex)
            {
                await WriteSseEventAsync("error", new { message = ex.Message }, CancellationToken.None);
            }
        }

        private static readonly JsonSerializerOptions _camelCaseOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        private async Task WriteSseEventAsync(string eventName, object payload, CancellationToken cancellationToken)
        {
            var json = JsonSerializer.Serialize(payload, _camelCaseOptions);
            await Response.WriteAsync($"event: {eventName}\n", cancellationToken);
            await Response.WriteAsync($"data: {json}\n\n", cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
        }

        private static IEnumerable<string> ChunkAnswer(string answer)
        {
            if (string.IsNullOrWhiteSpace(answer))
            {
                yield break;
            }

            var matches = Regex.Matches(answer, @"\S+\s*");
            foreach (Match match in matches)
            {
                if (!string.IsNullOrEmpty(match.Value))
                {
                    yield return match.Value;
                }
            }
        }
    }
}
