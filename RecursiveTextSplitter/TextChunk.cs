namespace RecursiveTextSplitting;

/// <summary>
/// Represents a chunk of text with metadata about its position, overlap, and splitting information.
/// </summary>
public class TextChunk
{
    /// <summary>
    /// Gets or sets the complete text of the chunk, including any overlap from the previous chunk.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the overlapping text from the previous chunk (empty for the first chunk).
    /// </summary>
    public string OverlapText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the original chunk text without overlap.
    /// </summary>
    public string ChunkText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the 1-based starting position of this chunk in the original text.
    /// </summary>
    public int StartPosition { get; set; }

    /// <summary>
    /// Gets or sets the 1-based ending position of this chunk in the original text.
    /// </summary>
    public int EndPosition { get; set; }

    /// <summary>
    /// Gets or sets the separator that was used to create this chunk.
    /// </summary>
    public string SeparatorUsed { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the 1-based index of this chunk in the sequence of chunks.
    /// </summary>
    public int ChunkIndex { get; set; }

    /// <summary>
    /// Gets or sets the 1-based column number where this chunk starts in the original text.
    /// </summary>
    public int StartColumn { get; set; }

    /// <summary>
    /// Gets or sets the 1-based line number where this chunk starts in the original text.
    /// </summary>
    public int StartLine { get; set; }

    /// <summary>
    /// Gets or sets the 1-based column number where this chunk ends in the original text.
    /// </summary>
    public int EndColumn { get; set; }

    /// <summary>
    /// Gets or sets the 1-based line number where this chunk ends in the original text.
    /// </summary>
    public int EndLine { get; set; }
}
