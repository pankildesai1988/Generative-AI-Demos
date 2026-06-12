using System.Text.RegularExpressions;
using ArNir.Platform.Configuration;
using ArNir.RAG.Interfaces;
using ArNir.RAG.Models;
using Microsoft.Extensions.Options;

namespace ArNir.RAG.Chunking;

/// <summary>
/// Splits a <see cref="RagDocument"/> into chunks that respect sentence boundaries.
/// Sentences are packed greedily into a chunk until adding the next sentence would exceed the
/// target chunk size; the chunk is then flushed and a new one started. Chunks therefore never
/// split mid-sentence, which improves embedding quality over the fixed-size
/// <see cref="SlidingWindowChunker"/>.
/// <para>
/// Overlap is sentence-based: each new chunk is seeded with the trailing sentences of the
/// previous chunk whose combined length fits within the configured overlap character budget,
/// preserving context across chunk boundaries.
/// </para>
/// <para>
/// A single sentence longer than the chunk size is hard-split at the chunk-size boundary as a
/// last resort so no content is dropped.
/// </para>
/// <para>
/// Chunk size/overlap default to the <c>Rag</c> appsettings section (<see cref="RagSettings"/>);
/// explicit method arguments override. When <see cref="RagDocument.Pages"/> is populated the
/// chunker runs per-page so each chunk carries the originating <see cref="RagChunk.PageNumber"/>.
/// </para>
/// </summary>
public sealed class SentenceAwareChunker : IDocumentChunker
{
    private readonly RagSettings _settings;

    /// <summary>
    /// Matches sentence terminators (<c>.</c>, <c>!</c>, <c>?</c>, optionally followed by closing
    /// quotes/brackets) trailed by whitespace, and paragraph breaks — the points where the text
    /// may be split into sentences.
    /// </summary>
    private static readonly Regex SentenceBoundary = new(
        @"(?<=[.!?][""')\]]?)\s+|\n{2,}",
        RegexOptions.Compiled);

    /// <summary>Initialises the chunker with appsettings-bound <see cref="RagSettings"/>.</summary>
    /// <param name="settings">The bound RAG options; when unconfigured, property defaults apply.</param>
    public SentenceAwareChunker(IOptions<RagSettings> settings)
        => _settings = settings.Value;

    /// <inheritdoc />
    /// <remarks>
    /// Each chunk's <see cref="RagChunk.Metadata"/> contains <c>DocumentName</c>, <c>ChunkIndex</c>,
    /// and <c>PageNumber</c>. Whitespace-only chunks are skipped.
    /// </remarks>
    public IReadOnlyList<RagChunk> Chunk(RagDocument document, int? chunkSize = null, int? overlap = null)
    {
        var effectiveChunkSize = chunkSize ?? _settings.ChunkSize;
        var effectiveOverlap   = overlap   ?? _settings.ChunkOverlap;

        if (effectiveChunkSize <= 0) throw new ArgumentOutOfRangeException(nameof(chunkSize), "chunkSize must be > 0.");
        if (effectiveOverlap < 0)    throw new ArgumentOutOfRangeException(nameof(overlap),    "overlap must be >= 0.");
        if (effectiveOverlap >= effectiveChunkSize) throw new ArgumentOutOfRangeException(nameof(overlap), "overlap must be < chunkSize.");

        var chunks = new List<RagChunk>();
        var index  = 0;

        var pages = document.Pages is { Count: > 0 }
            ? document.Pages
            : new List<RagPageContent> { new() { PageNumber = 1, Text = document.Content ?? string.Empty } };

        foreach (var page in pages)
        {
            var sentences = SplitIntoSentences(page.Text ?? string.Empty, effectiveChunkSize);
            if (sentences.Count == 0)
                continue;

            var current = new List<string>();
            var currentLength = 0;

            foreach (var sentence in sentences)
            {
                // +1 accounts for the joining space between sentences.
                var addedLength = currentLength == 0 ? sentence.Length : sentence.Length + 1;

                if (currentLength + addedLength > effectiveChunkSize && current.Count > 0)
                {
                    FlushChunk(chunks, current, document, page.PageNumber, ref index);

                    var carried = TakeTrailingSentences(current, effectiveOverlap);
                    current = carried;
                    currentLength = carried.Sum(s => s.Length) + Math.Max(0, carried.Count - 1);

                    addedLength = currentLength == 0 ? sentence.Length : sentence.Length + 1;
                }

                current.Add(sentence);
                currentLength += addedLength;
            }

            if (current.Count > 0)
                FlushChunk(chunks, current, document, page.PageNumber, ref index);
        }

        return chunks.AsReadOnly();
    }

    /// <summary>
    /// Splits page text into sentences; any sentence exceeding <paramref name="maxLength"/> is
    /// hard-split into <paramref name="maxLength"/>-sized pieces so it can still be packed.
    /// </summary>
    /// <param name="text">The page text to split.</param>
    /// <param name="maxLength">The maximum length any returned sentence may have.</param>
    /// <returns>The ordered list of non-empty sentences.</returns>
    private static List<string> SplitIntoSentences(string text, int maxLength)
    {
        var result = new List<string>();

        foreach (var raw in SentenceBoundary.Split(text))
        {
            var sentence = raw.Trim();
            if (sentence.Length == 0)
                continue;

            if (sentence.Length <= maxLength)
            {
                result.Add(sentence);
                continue;
            }

            for (var start = 0; start < sentence.Length; start += maxLength)
            {
                var piece = sentence.Substring(start, Math.Min(maxLength, sentence.Length - start)).Trim();
                if (piece.Length > 0)
                    result.Add(piece);
            }
        }

        return result;
    }

    /// <summary>
    /// Returns the trailing sentences of <paramref name="sentences"/> whose combined length
    /// (including joining spaces) fits within the <paramref name="overlapBudget"/> character budget.
    /// </summary>
    /// <param name="sentences">The sentences of the chunk that was just flushed.</param>
    /// <param name="overlapBudget">The maximum characters the carried-over sentences may total.</param>
    /// <returns>The sentences to seed the next chunk with, in original order.</returns>
    private static List<string> TakeTrailingSentences(List<string> sentences, int overlapBudget)
    {
        var carried = new List<string>();
        var total = 0;

        for (var i = sentences.Count - 1; i >= 0; i--)
        {
            var added = carried.Count == 0 ? sentences[i].Length : sentences[i].Length + 1;
            if (total + added > overlapBudget)
                break;

            carried.Insert(0, sentences[i]);
            total += added;
        }

        return carried;
    }

    /// <summary>
    /// Joins the accumulated sentences into a <see cref="RagChunk"/> and appends it to
    /// <paramref name="chunks"/>, advancing the running chunk <paramref name="index"/>.
    /// </summary>
    /// <param name="chunks">The output chunk list.</param>
    /// <param name="sentences">The sentences forming the chunk body.</param>
    /// <param name="document">The source document.</param>
    /// <param name="pageNumber">The 1-based page the sentences came from.</param>
    /// <param name="index">The running zero-based chunk index, incremented on append.</param>
    private static void FlushChunk(List<RagChunk> chunks, List<string> sentences, RagDocument document, int pageNumber, ref int index)
    {
        var text = string.Join(" ", sentences);
        if (string.IsNullOrWhiteSpace(text))
            return;

        chunks.Add(new RagChunk
        {
            DocumentId = document.Id,
            ChunkIndex = index,
            Text       = text,
            TokenCount = (int)Math.Ceiling(text.Length / 4.0),
            PageNumber = pageNumber,
            Metadata   = new Dictionary<string, string>
            {
                ["DocumentName"] = document.FileName,
                ["ChunkIndex"]   = index.ToString(),
                ["PageNumber"]   = pageNumber.ToString()
            }
        });

        index++;
    }
}
