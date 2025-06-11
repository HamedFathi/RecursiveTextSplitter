namespace RecursiveTextSplitting;

/// <summary>
/// Represents a chunk of text created during the splitting process with metadata about its position and overlap.
/// </summary>
public class TextChunk
{
    /// <summary>
    /// Gets or sets the full text content of this chunk, including any overlap from the previous chunk.
    /// </summary>
    /// <value>The complete text content including overlap. Default is empty string.</value>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the overlap portion of text that was carried over from the previous chunk.
    /// </summary>
    /// <value>Only the overlapping text content. Default is empty string.</value>
    public string OverlapText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the original chunk text without any overlap content.
    /// </summary>
    /// <value>The chunk content excluding overlap. Default is empty string.</value>
    public string ChunkText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the starting character position of this chunk in the original document.
    /// </summary>
    /// <value>Zero-based index of the first character of this chunk in the source text.</value>
    public int StartPosition { get; set; }

    /// <summary>
    /// Gets or sets the ending character position of this chunk in the original document.
    /// </summary>
    /// <value>Zero-based index of the last character of this chunk in the source text.</value>
    public int EndPosition { get; set; }

    /// <summary>
    /// Gets or sets the separator string that was used to create this chunk.
    /// </summary>
    /// <value>The separator string used during splitting. Default is empty string.</value>
    public string SeparatorUsed { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the sequential index of this chunk within the document.
    /// </summary>
    /// <value>Zero-based sequential position of this chunk in the split document.</value>
    public int ChunkIndex { get; set; }
}