using System.Text;
using ArNir.Core.Config;
using ArNir.Core.DTOs.Documents;
using ArNir.Core.DTOs.Embeddings;
using ArNir.Core.Interfaces;
using ArNir.Core.Models.Chunking;
using ArNir.Data;
using ArNir.Platform.Configuration;
using ArNir.RAG.Chunking;
using ArNir.RAG.Extraction;
using ArNir.RAG.Interfaces;
using ArNir.RAG.Models;
using ArNir.RAG.Parsing;
using ArNir.RAG.Pipeline;
using ArNir.Services;
using ArNir.Services.Interfaces;
using ArNir.Services.Mapping;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace ArNir.Tests.T3;

/// <summary>
/// The critical dual-path alignment tests: <c>DocumentService</c> (SQL path) and
/// <c>IngestionPipeline</c> (vector path) must produce identical chunk sequences from the same
/// bytes, so PostgreSQL embeddings keyed <c>"sql:{docId}:{index}"</c> always FK to the
/// <c>DocumentChunk</c> row holding the same text.
/// </summary>
public class DocumentServiceUnifiedPathTests
{
    private const string SampleText =
        "Alpha sentence one here. Bravo sentence two here. Charlie sentence three here. " +
        "Delta sentence four here. Echo sentence five here.";

    private static DbContextOptions<ArNirDbContext> CreateDbOptions()
        => new DbContextOptionsBuilder<ArNirDbContext>()
            .UseInMemoryDatabase("DocSvcUnified_" + Guid.NewGuid())
            .Options;

    private static IMapper CreateMapper()
        => new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>()).CreateMapper();

    private static UnifiedChunkExtractor CreateExtractor(RagSettings settings)
    {
        var options = Options.Create(settings);
        return new UnifiedChunkExtractor(
            new IDocumentParser[] { new PlainTextDocumentParser() },
            new SlidingWindowChunker(options),
            new SentenceAwareChunker(options),
            options,
            NullLogger<UnifiedChunkExtractor>.Instance);
    }

    private static DocumentService CreateService(ArNirDbContext context, IUnifiedChunkExtractor? extractor)
    {
        var fileSettings = Options.Create(new FileUploadSettings
        {
            AllowedTypes = new[] { "text/plain", "application/pdf" },
            MaxFileSize = 10_000_000
        });

        var embedding = new Mock<IEmbeddingService>();
        embedding
            .Setup(e => e.RebuildEmbeddingsForDocumentAsync(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(new List<EmbeddingResultDto>());

        return new DocumentService(
            context, CreateMapper(), fileSettings, embedding.Object,
            Options.Create(new RagSettings()), extractor);
    }

    private static IFormFile CreateFile(string content, string fileName = "doc.txt", string contentType = "text/plain")
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        var file = new Mock<IFormFile>();
        file.Setup(f => f.OpenReadStream()).Returns(() => new MemoryStream(bytes));
        file.Setup(f => f.FileName).Returns(fileName);
        file.Setup(f => f.ContentType).Returns(contentType);
        file.Setup(f => f.Length).Returns(bytes.Length);
        return file.Object;
    }

    [Fact]
    public async Task Upload_WithExtractor_PersistsExtractorSequence()
    {
        var settings = new RagSettings { ChunkSize = 60, ChunkOverlap = 0, MinChunkSize = 0, ChunkingStrategy = "sentence" };
        var extractor = CreateExtractor(settings);
        var dbOptions = CreateDbOptions();

        using (var context = new ArNirDbContext(dbOptions))
        {
            var service = CreateService(context, extractor);
            await service.UploadDocumentAsync(new DocumentUploadDto { File = CreateFile(SampleText) });
        }

        var expected = await extractor.ExtractAsync(
            new MemoryStream(Encoding.UTF8.GetBytes(SampleText)), "doc.txt", "text/plain");

        using (var context = new ArNirDbContext(dbOptions))
        {
            var persisted = await context.DocumentChunks.OrderBy(c => c.ChunkOrder).ToListAsync();

            Assert.Equal(expected.Chunks.Count, persisted.Count);
            for (var i = 0; i < persisted.Count; i++)
            {
                Assert.Equal(expected.Chunks[i].Index,      persisted[i].ChunkOrder);
                Assert.Equal(expected.Chunks[i].Text,       persisted[i].Text);
                Assert.Equal(expected.Chunks[i].PageNumber, persisted[i].PageNumber);
                Assert.Equal(expected.Chunks[i].ChunkType,  persisted[i].ChunkType);
            }
        }
    }

    [Fact]
    public async Task Upload_ThenPipeline_ChunkIdsAlignWithPersistedChunkOrder()
    {
        var settings = new RagSettings { ChunkSize = 60, ChunkOverlap = 0, MinChunkSize = 0, ChunkingStrategy = "sentence" };
        var extractor = CreateExtractor(settings);
        var dbOptions = CreateDbOptions();

        int documentId;
        using (var context = new ArNirDbContext(dbOptions))
        {
            var service = CreateService(context, extractor);
            var dto = await service.UploadDocumentAsync(new DocumentUploadDto { File = CreateFile(SampleText) });
            documentId = dto.Id;
        }

        // Vector path: same extractor, same bytes, mocked embedder + capturing vector store.
        var embedder = new Mock<IDocumentEmbedder>();
        embedder
            .Setup(e => e.GenerateBatchAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<string>()))
            .ReturnsAsync((IEnumerable<string> texts, string _) =>
                texts.Select(_ => new float[] { 0.1f }).ToList());

        var capturedIds = new List<string>();
        var vectorStore = new Mock<IDocumentVectorStore>();
        vectorStore
            .Setup(v => v.StoreBatchAsync(It.IsAny<IEnumerable<(string chunkId, float[] vector)>>()))
            .Callback((IEnumerable<(string chunkId, float[] vector)> items) =>
                capturedIds.AddRange(items.Select(i => i.chunkId)))
            .Returns(Task.CompletedTask);

        var pipeline = new IngestionPipeline(
            extractor, embedder.Object, vectorStore.Object, NullLogger<IngestionPipeline>.Instance);

        var result = await pipeline.IngestAsync(new IngestionRequest
        {
            FileStream          = new MemoryStream(Encoding.UTF8.GetBytes(SampleText)),
            FileName            = "doc.txt",
            ContentType         = "text/plain",
            LegacySqlDocumentId = documentId
        });

        Assert.True(result.Success);

        using (var context = new ArNirDbContext(dbOptions))
        {
            var persisted = await context.DocumentChunks.OrderBy(c => c.ChunkOrder).ToListAsync();

            Assert.Equal(persisted.Count, capturedIds.Count);
            for (var i = 0; i < persisted.Count; i++)
            {
                Assert.Equal($"sql:{documentId}:{persisted[i].ChunkOrder}", capturedIds[i]);
            }
        }
    }

    [Fact]
    public async Task Pipeline_SkipsImageStubsFromEmbedding_ButKeepsIndexAlignment()
    {
        // Extractor yields text(0) + table(1) + image(2). The pipeline must embed only the text
        // and table chunks; the image stub gets no embedding row, and the surviving chunkIds keep
        // their original indices (0 and 1) so SQL ChunkOrder ↔ embedding FK alignment holds.
        var extractor = new Mock<IUnifiedChunkExtractor>();
        extractor
            .Setup(e => e.ExtractAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChunkExtractionResult
            {
                DocumentId = Guid.NewGuid(),
                PageCount  = 1,
                Chunks     = new[]
                {
                    new ExtractedChunk { Index = 0, Text = "Plain prose chunk.",        ChunkType = ChunkTypes.Text },
                    new ExtractedChunk { Index = 1, Text = "The N9020B has RBW 1Hz.",   ChunkType = ChunkTypes.Table },
                    new ExtractedChunk { Index = 2, Text = "[Image: page 1, image 1]", ChunkType = ChunkTypes.Image }
                }
            });

        var embeddedTexts = new List<string>();
        var embedder = new Mock<IDocumentEmbedder>();
        embedder
            .Setup(e => e.GenerateBatchAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<string>()))
            .ReturnsAsync((IEnumerable<string> texts, string _) =>
            {
                var list = texts.ToList();
                embeddedTexts.AddRange(list);
                return list.Select(_ => new float[] { 0.1f }).ToList();
            });

        var capturedIds = new List<string>();
        var vectorStore = new Mock<IDocumentVectorStore>();
        vectorStore
            .Setup(v => v.StoreBatchAsync(It.IsAny<IEnumerable<(string chunkId, float[] vector)>>()))
            .Callback((IEnumerable<(string chunkId, float[] vector)> items) =>
                capturedIds.AddRange(items.Select(i => i.chunkId)))
            .Returns(Task.CompletedTask);

        var pipeline = new IngestionPipeline(
            extractor.Object, embedder.Object, vectorStore.Object, NullLogger<IngestionPipeline>.Instance);

        var result = await pipeline.IngestAsync(new IngestionRequest
        {
            FileStream          = new MemoryStream(new byte[] { 1 }),
            FileName            = "spec.pdf",
            ContentType         = "application/pdf",
            LegacySqlDocumentId = 7
        });

        Assert.True(result.Success);
        Assert.Equal(3, result.ChunksCreated);     // all chunks reported
        Assert.Equal(2, result.EmbeddingsCreated); // image stub not embedded

        Assert.DoesNotContain(embeddedTexts, t => t.StartsWith("[Image:"));
        Assert.Equal(new[] { "sql:7:0", "sql:7:1" }, capturedIds); // indices preserved, image (2) absent
    }

    [Fact]
    public async Task Upload_WithoutExtractor_FallsBackToLegacyChunking()
    {
        var dbOptions = CreateDbOptions();

        using (var context = new ArNirDbContext(dbOptions))
        {
            var service = CreateService(context, extractor: null);
            await service.UploadDocumentAsync(new DocumentUploadDto { File = CreateFile(SampleText) });
        }

        using (var context = new ArNirDbContext(dbOptions))
        {
            var persisted = await context.DocumentChunks.ToListAsync();

            // Legacy ChunkText path: chunks exist but carry no page/type provenance.
            Assert.NotEmpty(persisted);
            Assert.All(persisted, c =>
            {
                Assert.Null(c.PageNumber);
                Assert.Null(c.ChunkType);
            });
        }
    }

    [Fact]
    public async Task Upload_TableAndImageChunks_NullBboxPersistsWithoutThrow()
    {
        var dbOptions = CreateDbOptions();

        var extractor = new Mock<IUnifiedChunkExtractor>();
        extractor
            .Setup(e => e.ExtractAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChunkExtractionResult
            {
                DocumentId = Guid.NewGuid(),
                PageCount  = 1,
                Chunks     = new[]
                {
                    new ExtractedChunk
                    {
                        Index = 0, Text = "The N9020B has frequency range 10Hz-26.5GHz.",
                        PageNumber = 3, ChunkType = ChunkTypes.Table,
                        BboxX1 = 10f, BboxY1 = 20f, BboxX2 = 300f, BboxY2 = 90f
                    },
                    new ExtractedChunk
                    {
                        Index = 1, Text = "[Image: page 3, image 1]",
                        PageNumber = 3, ChunkType = ChunkTypes.Image
                        // bbox intentionally null — pending vision-captioning decision
                    }
                }
            });

        using (var context = new ArNirDbContext(dbOptions))
        {
            var service = CreateService(context, extractor.Object);
            await service.UploadDocumentAsync(new DocumentUploadDto { File = CreateFile(SampleText) });
        }

        using (var context = new ArNirDbContext(dbOptions))
        {
            var persisted = await context.DocumentChunks.OrderBy(c => c.ChunkOrder).ToListAsync();

            Assert.Equal(2, persisted.Count);
            Assert.Equal("table", persisted[0].ChunkType);
            Assert.Equal(10f, persisted[0].BboxX1);
            Assert.Equal("image", persisted[1].ChunkType);
            Assert.Null(persisted[1].BboxX1);
            Assert.Null(persisted[1].BboxY2);
            Assert.Equal(3, persisted[1].PageNumber);
        }
    }
}
