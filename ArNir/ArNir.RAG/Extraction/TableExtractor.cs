namespace ArNir.RAG.Extraction;

/// <summary>
/// A positioned word on a PDF page, in PDF point coordinates (origin bottom-left).
/// Abstraction over PdfPig's <c>Word</c> so the table heuristic stays unit-testable
/// without fabricating PdfPig types.
/// </summary>
/// <param name="Text">The word text.</param>
/// <param name="Left">The left X of the word's bounding box.</param>
/// <param name="Bottom">The bottom Y of the word's bounding box.</param>
/// <param name="Right">The right X of the word's bounding box.</param>
/// <param name="Top">The top Y of the word's bounding box.</param>
public sealed record WordBox(string Text, double Left, double Bottom, double Right, double Top);

/// <summary>
/// A table detected on a PDF page: a header row, data rows, the union bounding box of all member
/// words, and the set of consumed words (so the caller can exclude them from the plain-text stream
/// and avoid duplicated content in text chunks).
/// </summary>
public sealed class DetectedTable
{
    /// <summary>Gets the header cell texts (first row of the detected run).</summary>
    public IReadOnlyList<string> Headers { get; init; } = Array.Empty<string>();

    /// <summary>Gets the data rows; each row is an ordered list of cell texts.</summary>
    public IReadOnlyList<IReadOnlyList<string>> Rows { get; init; } = Array.Empty<IReadOnlyList<string>>();

    /// <summary>Gets the left X of the table region (union of member word boxes).</summary>
    public double X1 { get; init; }

    /// <summary>Gets the bottom Y of the table region (union of member word boxes).</summary>
    public double Y1 { get; init; }

    /// <summary>Gets the right X of the table region (union of member word boxes).</summary>
    public double X2 { get; init; }

    /// <summary>Gets the top Y of the table region (union of member word boxes).</summary>
    public double Y2 { get; init; }

    /// <summary>Gets the words that belong to this table (for exclusion from the text stream).</summary>
    public IReadOnlySet<WordBox> ConsumedWords { get; init; } = new HashSet<WordBox>();
}

/// <summary>
/// Heuristic table detector over positioned page words (PdfPig has no table API).
/// <para>
/// Pipeline: cluster words into lines by baseline Y → segment each line into cells on large
/// horizontal gaps → a table is a run of ≥ <see cref="MinTableRows"/> consecutive multi-cell rows
/// whose column start Xs align within <see cref="ColumnAlignmentTolerance"/> points (±1 column
/// count tolerated for merged cells). The first row of a run is the header.
/// </para>
/// <para>
/// Deliberately conservative: any line that breaks alignment terminates the run, and runs shorter
/// than the minimum produce no table — the page then falls through to plain text chunking, so a
/// missed table can never break normal text extraction.
/// </para>
/// </summary>
public sealed class TableExtractor
{
    /// <summary>Minimum consecutive aligned multi-cell rows (including header) to qualify as a table.</summary>
    private const int MinTableRows = 3;

    /// <summary>Minimum cells per row for the row to count as a table candidate.</summary>
    private const int MinColumns = 2;

    /// <summary>Maximum X drift (points) a cell start may have against the header's column start.</summary>
    private const double ColumnAlignmentTolerance = 6.0;

    /// <summary>Minimum horizontal gap (points) that separates two cells regardless of font metrics.</summary>
    private const double MinCellGap = 12.0;

    /// <summary>Fallback baseline tolerance (points) when the median word height is degenerate.</summary>
    private const double FallbackLineTolerance = 3.0;

    /// <summary>
    /// Detects tables among the page's words. Returns an empty list when nothing qualifies.
    /// </summary>
    /// <param name="words">All positioned words of one page.</param>
    public IReadOnlyList<DetectedTable> DetectTables(IReadOnlyList<WordBox> words)
    {
        if (words.Count == 0)
            return Array.Empty<DetectedTable>();

        var lines = ClusterLines(words);
        var candidates = lines.Select(SegmentCells).ToList();

        var tables = new List<DetectedTable>();
        var runStart = -1;

        for (var i = 0; i <= candidates.Count; i++)
        {
            var isCandidate = i < candidates.Count
                && candidates[i].Count >= MinColumns
                && (runStart < 0 || AlignsWith(candidates[runStart], candidates[i]));

            if (isCandidate)
            {
                if (runStart < 0) runStart = i;
                continue;
            }

            if (runStart >= 0)
            {
                var runLength = i - runStart;
                if (runLength >= MinTableRows)
                    tables.Add(BuildTable(candidates, runStart, runLength));

                // The current line may itself start a new run.
                runStart = i < candidates.Count && candidates[i].Count >= MinColumns ? i : -1;
            }
        }

        return tables;
    }

    /// <summary>
    /// Clusters words into visual lines by baseline (<see cref="WordBox.Bottom"/>), top of page
    /// first, each line sorted left-to-right. Exposed for callers that need to rebuild page text
    /// after excluding table words.
    /// </summary>
    /// <param name="words">All positioned words of one page.</param>
    public static IReadOnlyList<IReadOnlyList<WordBox>> ClusterLines(IReadOnlyList<WordBox> words)
    {
        if (words.Count == 0)
            return Array.Empty<IReadOnlyList<WordBox>>();

        var heights = words.Select(w => w.Top - w.Bottom).Where(h => h > 0).OrderBy(h => h).ToList();
        var medianHeight = heights.Count > 0 ? heights[heights.Count / 2] : 0;
        var tolerance = Math.Max(medianHeight / 2.0, FallbackLineTolerance);

        var lines = new List<List<WordBox>>();
        List<WordBox>? current = null;
        double currentBaseline = 0;

        foreach (var word in words.OrderByDescending(w => w.Bottom))
        {
            if (current is null || Math.Abs(word.Bottom - currentBaseline) > tolerance)
            {
                current = new List<WordBox>();
                currentBaseline = word.Bottom;
                lines.Add(current);
            }
            current.Add(word);
        }

        foreach (var line in lines)
            line.Sort((a, b) => a.Left.CompareTo(b.Left));

        return lines;
    }

    /// <summary>
    /// Segments a line into cells: words separated by a gap larger than
    /// <c>max(2 × median gap, MinCellGap)</c> start a new cell. Lines whose words have no large
    /// gaps yield a single cell (i.e. not a table candidate).
    /// </summary>
    /// <param name="line">The line's words, sorted left-to-right.</param>
    private static List<Cell> SegmentCells(IReadOnlyList<WordBox> line)
    {
        var cells = new List<Cell>();
        if (line.Count == 0)
            return cells;

        var gaps = new List<double>();
        for (var i = 1; i < line.Count; i++)
            gaps.Add(Math.Max(0, line[i].Left - line[i - 1].Right));

        // Estimate the single-space width from the SMALL gaps only — in a table row with
        // single-word cells every gap is a column gap, so a plain median would inflate the
        // threshold past the column gaps themselves and suppress detection.
        var smallGaps = gaps.Where(g => g > 0 && g <= MinCellGap).OrderBy(g => g).ToList();
        var spaceWidth = smallGaps.Count > 0 ? smallGaps[smallGaps.Count / 2] : 0;
        var threshold = Math.Max(2 * spaceWidth, MinCellGap);

        var cellWords = new List<WordBox> { line[0] };
        for (var i = 1; i < line.Count; i++)
        {
            if (gaps[i - 1] > threshold)
            {
                cells.Add(Cell.From(cellWords));
                cellWords = new List<WordBox>();
            }
            cellWords.Add(line[i]);
        }
        cells.Add(Cell.From(cellWords));

        return cells;
    }

    /// <summary>
    /// Checks whether a row belongs to the same table as the reference row. Anchors on the
    /// <b>key column</b> (cell 0) start X within <see cref="ColumnAlignmentTolerance"/> and an
    /// equal-ish column count (±1 for merged cells). Value columns are deliberately NOT required
    /// to align tightly — wrapped or indented value cells (common in label/value slides) jitter
    /// horizontally and would otherwise split one table into fragments.
    /// </summary>
    /// <param name="reference">The run's first candidate row.</param>
    /// <param name="row">The row to test.</param>
    private static bool AlignsWith(List<Cell> reference, List<Cell> row)
    {
        if (Math.Abs(reference.Count - row.Count) > 1)
            return false;

        return Math.Abs(reference[0].Left - row[0].Left) <= ColumnAlignmentTolerance;
    }

    /// <summary>
    /// Materialises a detected run into a <see cref="DetectedTable"/>. A run whose every row has
    /// exactly two cells is treated as a <b>key/value</b> table (no header row — every line is a
    /// label/value pair), signalled by empty <see cref="DetectedTable.Headers"/>. Otherwise the
    /// first row is the header and the rest are data rows.
    /// </summary>
    /// <param name="candidates">All segmented lines of the page.</param>
    /// <param name="runStart">The index of the run's first line.</param>
    /// <param name="runLength">The number of lines in the run.</param>
    private static DetectedTable BuildTable(List<List<Cell>> candidates, int runStart, int runLength)
    {
        var runRows = candidates.Skip(runStart).Take(runLength).ToList();
        var consumed = new HashSet<WordBox>(runRows.SelectMany(r => r).SelectMany(c => c.Words));

        var isKeyValue = runRows.All(r => r.Count == 2);

        var headers = isKeyValue
            ? new List<string>()
            : runRows[0].Select(c => c.Text).ToList();

        var rows = (isKeyValue ? runRows : runRows.Skip(1))
            .Select(r => (IReadOnlyList<string>)r.Select(c => c.Text).ToList())
            .ToList();

        return new DetectedTable
        {
            Headers       = headers,
            Rows          = rows,
            X1            = consumed.Min(w => w.Left),
            Y1            = consumed.Min(w => w.Bottom),
            X2            = consumed.Max(w => w.Right),
            Y2            = consumed.Max(w => w.Top),
            ConsumedWords = consumed
        };
    }

    /// <summary>One table cell: its words, joined text, and start X used for column alignment.</summary>
    private sealed class Cell
    {
        /// <summary>Gets the cell's member words in left-to-right order.</summary>
        public List<WordBox> Words { get; private init; } = new();

        /// <summary>Gets the space-joined cell text.</summary>
        public string Text { get; private init; } = string.Empty;

        /// <summary>Gets the cell's start X (left edge of its first word).</summary>
        public double Left { get; private init; }

        /// <summary>Builds a cell from its member words.</summary>
        /// <param name="words">The cell's words, sorted left-to-right.</param>
        public static Cell From(List<WordBox> words) => new()
        {
            Words = words,
            Text  = string.Join(" ", words.Select(w => w.Text)),
            Left  = words[0].Left
        };
    }
}
