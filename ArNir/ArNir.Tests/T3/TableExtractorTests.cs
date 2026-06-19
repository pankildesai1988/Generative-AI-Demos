using ArNir.RAG.Extraction;
using Xunit;

namespace ArNir.Tests.T3;

/// <summary>
/// Unit tests for the heuristic <see cref="TableExtractor"/> over synthetic <see cref="WordBox"/>
/// grids (PDF point coordinates, origin bottom-left — higher Y = higher on the page).
/// </summary>
public class TableExtractorTests
{
    /// <summary>Builds one row of single-word cells at the given baseline and column X starts.</summary>
    private static IEnumerable<WordBox> Row(double baseline, params (double x, string text)[] cells)
        => cells.Select(c => new WordBox(c.text, c.x, baseline, c.x + 40, baseline + 10));

    [Fact]
    public void DetectTables_AlignedGrid_ReturnsTableWithHeaderAndRows()
    {
        var words = new List<WordBox>();
        words.AddRange(Row(700, (50, "Model"),  (200, "Frequency"), (350, "RBW")));
        words.AddRange(Row(680, (50, "N9020B"), (200, "26.5GHz"),   (350, "1Hz")));
        words.AddRange(Row(660, (50, "N9030B"), (200, "50GHz"),     (350, "1Hz")));

        var tables = new TableExtractor().DetectTables(words);

        Assert.Single(tables);
        var table = tables[0];
        Assert.Equal(new[] { "Model", "Frequency", "RBW" }, table.Headers);
        Assert.Equal(2, table.Rows.Count);
        Assert.Equal(new[] { "N9020B", "26.5GHz", "1Hz" }, table.Rows[0]);
        Assert.Equal(new[] { "N9030B", "50GHz", "1Hz" }, table.Rows[1]);
    }

    [Fact]
    public void DetectTables_BboxIsUnionOfMemberWords()
    {
        var words = new List<WordBox>();
        words.AddRange(Row(700, (50, "A"), (200, "B")));
        words.AddRange(Row(680, (50, "C"), (200, "D")));
        words.AddRange(Row(660, (50, "E"), (200, "F")));

        var table = Assert.Single(new TableExtractor().DetectTables(words));

        Assert.Equal(50, table.X1);
        Assert.Equal(660, table.Y1);
        Assert.Equal(240, table.X2); // 200 + word width 40
        Assert.Equal(710, table.Y2); // 700 + word height 10
        Assert.Equal(6, table.ConsumedWords.Count);
    }

    [Fact]
    public void DetectTables_OnlyTwoRows_ReturnsNoTable()
    {
        var words = new List<WordBox>();
        words.AddRange(Row(700, (50, "Model"),  (200, "Frequency")));
        words.AddRange(Row(680, (50, "N9020B"), (200, "26.5GHz")));

        Assert.Empty(new TableExtractor().DetectTables(words));
    }

    [Fact]
    public void DetectTables_ProseLines_ReturnsNoTable()
    {
        // Prose: words tightly packed (small uniform gaps) — each line segments into one cell.
        var words = new List<WordBox>();
        for (var line = 0; line < 5; line++)
        {
            var baseline = 700 - line * 20;
            for (var i = 0; i < 8; i++)
            {
                var left = 50 + i * 45;
                words.Add(new WordBox($"word{i}", left, baseline, left + 40, baseline + 10));
            }
        }

        Assert.Empty(new TableExtractor().DetectTables(words));
    }

    [Fact]
    public void DetectTables_RaggedRowWithinOneColumn_IsTolerated()
    {
        var words = new List<WordBox>();
        words.AddRange(Row(700, (50, "Model"),  (200, "Frequency"), (350, "RBW")));
        words.AddRange(Row(680, (50, "N9020B"), (200, "26.5GHz"),   (350, "1Hz")));
        words.AddRange(Row(660, (50, "N9030B"), (200, "50GHz"))); // merged trailing cell (±1 column)
        words.AddRange(Row(640, (50, "N9040B"), (200, "110GHz"),   (350, "10Hz")));

        var table = Assert.Single(new TableExtractor().DetectTables(words));

        Assert.Equal(3, table.Rows.Count);
    }

    [Fact]
    public void DetectTables_MisalignedColumns_TerminateRun()
    {
        var words = new List<WordBox>();
        words.AddRange(Row(700, (50, "Model"),  (200, "Frequency")));
        words.AddRange(Row(680, (50, "N9020B"), (200, "26.5GHz")));
        // Third line shifted far right — breaks alignment, run stays below 3 rows.
        words.AddRange(Row(660, (120, "Note"), (320, "shifted")));

        Assert.Empty(new TableExtractor().DetectTables(words));
    }

    [Fact]
    public void DetectTables_EmptyInput_ReturnsEmpty()
    {
        Assert.Empty(new TableExtractor().DetectTables(Array.Empty<WordBox>()));
    }

    [Fact]
    public void DetectTables_KeyValueTable_NoHeader_KeepsAllRows_DespiteValueColumnJitter()
    {
        // Mirrors the Mercedes slide: a 2-column label/value table whose value column jitters
        // horizontally (200 vs 270). Key column (x=50) is the anchor; all rows must survive,
        // and a 2-column run is reported as key/value (empty Headers, every line a data row).
        var words = new List<WordBox>();
        words.AddRange(Row(700, (50, "DrivingCams"),  (200, "8MP")));
        words.AddRange(Row(680, (50, "Interfaces"),   (270, "GMSL")));   // value indented
        words.AddRange(Row(660, (50, "ParkingCams"),  (200, "3MP")));
        words.AddRange(Row(640, (50, "Intelligence"), (260, "ECU")));    // value indented

        var table = Assert.Single(new TableExtractor().DetectTables(words));

        Assert.Empty(table.Headers);                 // key/value → no header row consumed
        Assert.Equal(4, table.Rows.Count);           // value jitter did not split the run
        Assert.Equal(new[] { "Interfaces", "GMSL" }, table.Rows[1]);
    }

    [Fact]
    public void ClusterLines_GroupsByBaselineTopFirst()
    {
        var words = new List<WordBox>
        {
            new("lower", 50, 660, 90, 670),
            new("upper2", 100, 700, 140, 710),
            new("upper1", 50, 700, 90, 710),
        };

        var lines = TableExtractor.ClusterLines(words);

        Assert.Equal(2, lines.Count);
        Assert.Equal(new[] { "upper1", "upper2" }, lines[0].Select(w => w.Text));
        Assert.Equal(new[] { "lower" }, lines[1].Select(w => w.Text));
    }
}
