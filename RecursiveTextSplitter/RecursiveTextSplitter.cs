using System.Text;

namespace RecursiveTextSplitting;

/// <summary>
/// Provides methods for recursively splitting text into chunks using various separators.
/// The splitter tries different separators in order of preference, falling back to character-level splitting if needed.
/// </summary>
public static class RecursiveTextSplitter
{
    /// <summary>
    /// Splits text into chunks using recursive text splitting with optional overlap.
    /// This is a simplified version that returns only the text content of each chunk.
    /// </summary>
    /// <param name="text">The text to split into chunks.</param>
    /// <param name="chunkSize">The maximum size of each chunk in characters.</param>
    /// <param name="chunkOverlap">The number of characters to overlap between chunks (default: 0).</param>
    /// <param name="separators">Custom separators to use for splitting (uses defaults if null).</param>
    /// <returns>An enumerable of text chunks as strings.</returns>
    /// <exception cref="ArgumentException">Thrown when chunkSize is not positive, chunkOverlap is negative, or chunkOverlap is greater than or equal to chunkSize.</exception>
    public static IEnumerable<string> RecursiveSplit(this string text, int chunkSize, int chunkOverlap = 0, string[]? separators = null)
    {
        // Simple wrapper that extracts just the text from the advanced method
        foreach (var result in text.AdvancedRecursiveSplit(chunkSize, chunkOverlap, separators))
        {
            yield return result.Text;
        }
    }

    /// <summary>
    /// Splits text into chunks using recursive text splitting with detailed metadata.
    /// The algorithm tries separators in order of preference (paragraphs, sentences, words, characters).
    /// </summary>
    /// <param name="text">The text to split into chunks.</param>
    /// <param name="chunkSize">The maximum size of each chunk in characters.</param>
    /// <param name="chunkOverlap">The number of characters to overlap between chunks (default: 0).</param>
    /// <param name="separators">Custom separators to use for splitting (uses defaults if null).</param>
    /// <returns>An enumerable of TextChunk objects with detailed metadata.</returns>
    /// <exception cref="ArgumentException">Thrown when chunkSize is not positive, chunkOverlap is negative, or chunkOverlap is greater than or equal to chunkSize.</exception>
    /// <remarks>
    /// The algorithm works by trying separators in order:
    /// 1. Double line breaks (paragraphs)
    /// 2. Sentence-ending punctuation with line breaks
    /// 3. Single line breaks
    /// 4. Sentence-ending punctuation with spaces
    /// 5. Word separators (spaces)
    /// 6. Individual characters (fallback)
    /// </remarks>
    public static IEnumerable<TextChunk> AdvancedRecursiveSplit(this string text, int chunkSize, int chunkOverlap = 0, string[]? separators = null)
    {
        // Handle edge cases
        if (string.IsNullOrEmpty(text))
            return [];

        // Validate parameters
        if (chunkSize <= 0)
            throw new ArgumentException("Chunk size must be positive", nameof(chunkSize));
        if (chunkOverlap < 0)
            throw new ArgumentException("Chunk overlap cannot be negative", nameof(chunkOverlap));
        if (chunkOverlap >= chunkSize)
            throw new ArgumentException("Chunk overlap must be less than chunk size", nameof(chunkSize));

        // Use default separators if none provided
        if (separators == null || separators.Length == 0)
            separators ??= GetDefaultSeparators();

        // Perform the initial recursive splitting
        var chunks = SplitRecursively(text, chunkSize, separators, 0, 0);

        // Apply overlap between chunks if requested
        if (chunkOverlap > 0 && chunks.Count > 1)
        {
            chunks = ApplyOverlap(chunks, chunkOverlap);
        }

        // Set final metadata for all chunks
        for (int i = 0; i < chunks.Count; i++)
        {
            chunks[i].ChunkIndex = i + 1;
            // Calculate line and column positions (1-based indexing)
            chunks[i].StartLine = GetLineColumnFromPosition(text, chunks[i].StartPosition - 1).line;
            chunks[i].StartColumn = GetLineColumnFromPosition(text, chunks[i].StartPosition - 1).column;
            chunks[i].EndLine = GetLineColumnFromPosition(text, chunks[i].EndPosition - 1).line;
            chunks[i].EndColumn = GetLineColumnFromPosition(text, chunks[i].EndPosition - 1).column;
        }

        return chunks;
    }

    /// <summary>
    /// Converts a character position to line and column numbers (1-based).
    /// </summary>
    /// <param name="text">The source text.</param>
    /// <param name="position">The 0-based character position.</param>
    /// <returns>A tuple containing the 1-based line and column numbers.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when position is outside the text bounds.</exception>
    private static (int line, int column) GetLineColumnFromPosition(string text, int position)
    {
        if (position < 0 || position > text.Length)
            throw new ArgumentOutOfRangeException(nameof(position));

        // Count newlines up to the position to determine line number
        var substring = text.Substring(0, position);
        var line = substring.Count(c => c == '\n') + 1;

        // Find the last newline to calculate column position
        var lastNewline = substring.LastIndexOf('\n');
        var column = position - lastNewline;

        return (line, column);
    }

    /// <summary>
    /// Applies overlap between consecutive chunks by prepending part of the previous chunk to each chunk.
    /// The overlap is word-safe, meaning it tries to break at word boundaries.
    /// </summary>
    /// <param name="chunks">The list of chunks to apply overlap to.</param>
    /// <param name="chunkOverlap">The maximum number of characters to overlap.</param>
    /// <returns>A new list of chunks with overlap applied.</returns>
    private static List<TextChunk> ApplyOverlap(List<TextChunk> chunks, int chunkOverlap)
    {
        if (chunks.Count == 0)
            return chunks;

        // First chunk doesn't need overlap
        var overlappedChunks = new List<TextChunk> { chunks[0] };
        var sb = new StringBuilder();

        // Apply overlap to each subsequent chunk
        for (int i = 1; i < chunks.Count; i++)
        {
            var prev = chunks[i - 1];
            var current = chunks[i];

            // Get word-safe overlap from the previous chunk
            var overlap = GetWordSafeOverlap(prev.Text, chunkOverlap);

            // Combine overlap with current chunk text
            sb.Clear();
            sb.Append(overlap);
            sb.Append(current.Text);

            // Create new chunk with overlap
            var newChunk = new TextChunk
            {
                Text = sb.ToString(),
                OverlapText = overlap,
                ChunkText = current.Text,
                StartPosition = current.StartPosition,
                EndPosition = current.EndPosition,
                SeparatorUsed = current.SeparatorUsed,
                ChunkIndex = current.ChunkIndex
            };

            overlappedChunks.Add(newChunk);
        }

        return overlappedChunks;
    }

    /// <summary>
    /// Extracts an overlap string from the end of text, trying to break at word boundaries.
    /// This ensures that overlap doesn't split words awkwardly.
    /// </summary>
    /// <param name="text">The text to extract overlap from.</param>
    /// <param name="maxLength">The maximum length of overlap to extract.</param>
    /// <returns>The overlap text, potentially shorter than maxLength to preserve word boundaries.</returns>
    private static string GetWordSafeOverlap(string text, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(text) || maxLength <= 0)
            return string.Empty;

        // Get the candidate overlap text from the end
        var start = Math.Max(0, text.Length - maxLength);
        var candidate = text.Substring(start);

        // Look for word boundary characters to split at
        int wordStart = candidate.IndexOfAny([
            ' ', '\n', '\r', '\t', '\f',           // Whitespace
            '.', ',', ';', ':', '!', '?',          // Punctuation
            '(', ')', '[', ']', '{', '}',          // Brackets
            '\"', '\'', '`'                        // Quotes
        ]);

        // If we found a word boundary, use text after it
        if (wordStart >= 0 && wordStart < candidate.Length - 1)
        {
            return candidate.Substring(wordStart + 1);
        }

        // Otherwise, return the full candidate
        return candidate;
    }

    /// <summary>
    /// Gets the default separators used for text splitting, ordered by preference.
    /// The order goes from larger semantic units (paragraphs) to smaller ones (characters).
    /// </summary>
    /// <returns>An array of separators in order of preference.</returns>
    private static string[] GetDefaultSeparators()
    {
        return [
            // Paragraph separators (highest priority)
            "\r\n\r\n", "\n\n",
            
            // Sentence endings with line breaks
            ".\r\n", "!\r\n", "?\r\n", ":\r\n", ";\r\n",
            "\r\n",
            
            // Sentence endings with newlines
            ".\n", "!\n", "?\n", ":\n", ";\n",
            "\n",
            
            // Word and character separators (lowest priority)
            ". ", "! ", "? ", "; ", ", ", " ", ""
        ];
    }

    /// <summary>
    /// Recursively splits text using the available separators, falling back to the next separator if the current one doesn't work.
    /// This is the core algorithm that implements the recursive splitting strategy.
    /// </summary>
    /// <param name="text">The text to split.</param>
    /// <param name="chunkSize">The maximum chunk size.</param>
    /// <param name="separators">The array of separators to try.</param>
    /// <param name="separatorIndex">The current separator index being tried.</param>
    /// <param name="currentPosition">The current position in the original text (for tracking metadata).</param>
    /// <returns>A list of text chunks.</returns>
    private static List<TextChunk> SplitRecursively(string text, int chunkSize, string[] separators, int separatorIndex, int currentPosition)
    {
        var chunks = new List<TextChunk>();

        // Base case: text fits in one chunk
        if (text.Length <= chunkSize)
        {
            if (text.Length > 0)
            {
                chunks.Add(new TextChunk
                {
                    Text = text,
                    OverlapText = string.Empty,
                    ChunkText = text,
                    StartPosition = currentPosition + 1,    // 1-based indexing
                    EndPosition = currentPosition + text.Length + 1,
                    SeparatorUsed = separatorIndex < separators.Length ? separators[separatorIndex] : "none",
                    ChunkIndex = 0  // Will be set later
                });
            }
            return chunks;
        }

        // Fallback case: no more separators, split by characters
        if (separatorIndex >= separators.Length)
        {
            for (var i = 0; i < text.Length; i += chunkSize)
            {
                var length = Math.Min(chunkSize, text.Length - i);
                var chunkText = text.Substring(i, length);
                chunks.Add(new TextChunk
                {
                    Text = chunkText,
                    OverlapText = string.Empty,
                    ChunkText = chunkText,
                    StartPosition = currentPosition + i + 1,
                    EndPosition = currentPosition + i + length + 1,
                    SeparatorUsed = "",
                    ChunkIndex = 0
                });
            }
            return chunks;
        }

        var separator = separators[separatorIndex];
        var remainingText = text;
        var textPosition = currentPosition;

        // Special case: empty separator means character-level splitting
        if (separator == "")
        {
            var sb = new StringBuilder(chunkSize);
            for (var i = 0; i < text.Length; i += chunkSize)
            {
                sb.Clear();
                var length = Math.Min(chunkSize, text.Length - i);
                sb.Append(text, i, length);
                var chunkText = sb.ToString();
                chunks.Add(new TextChunk
                {
                    Text = chunkText,
                    OverlapText = string.Empty,
                    ChunkText = chunkText,
                    StartPosition = currentPosition + i + 1,
                    EndPosition = currentPosition + i + length + 1,
                    SeparatorUsed = "",
                    ChunkIndex = 0
                });
            }
            return chunks;
        }

        // Main splitting loop
        while (remainingText.Length > 0)
        {
            // If remaining text fits in chunk size, we're done
            if (remainingText.Length <= chunkSize)
            {
                chunks.Add(new TextChunk
                {
                    Text = remainingText,
                    OverlapText = string.Empty,
                    ChunkText = remainingText,
                    StartPosition = textPosition + 1,
                    EndPosition = textPosition + remainingText.Length + 1,
                    SeparatorUsed = separator,
                    ChunkIndex = 0
                });
                break;
            }

            // If current separator doesn't exist, try the next one
            if (!remainingText.Contains(separator))
            {
                var recursiveChunks = SplitRecursively(remainingText, chunkSize, separators, separatorIndex + 1, textPosition);
                chunks.AddRange(recursiveChunks);
                break;
            }

            // Find the best place to split within the chunk size
            var splitAt = remainingText.LastIndexOf(separator, Math.Min(chunkSize - 1, remainingText.Length - 1), StringComparison.Ordinal);

            if (splitAt == -1)
            {
                // No separator found within chunk size, handle the oversized portion
                var firstSeparatorIndex = remainingText.IndexOf(separator, StringComparison.Ordinal);

                // Skip separator if it's at the beginning
                if (firstSeparatorIndex == 0)
                {
                    var separatorLength = separator.Length;
                    remainingText = remainingText.Substring(separatorLength);
                    textPosition += separatorLength;
                    continue;
                }

                // Recursively split the oversized chunk before the first separator
                var oversizedChunk = remainingText.Substring(0, firstSeparatorIndex);
                var recursiveChunks = SplitRecursively(oversizedChunk, chunkSize, separators, separatorIndex + 1, textPosition);
                chunks.AddRange(recursiveChunks);

                // Skip past the processed text and separator
                var skipLength = firstSeparatorIndex + separator.Length;
                remainingText = remainingText.Substring(skipLength);
                textPosition += skipLength;
            }
            else
            {
                // Found a good split point, create chunk including the separator
                var chunkLength = splitAt + separator.Length;
                var chunkText = remainingText.Substring(0, chunkLength);
                chunks.Add(new TextChunk
                {
                    Text = chunkText,
                    OverlapText = string.Empty,
                    ChunkText = chunkText,
                    StartPosition = textPosition + 1,
                    EndPosition = textPosition + chunkLength + 1,
                    SeparatorUsed = separator,
                    ChunkIndex = 0
                });

                // Move to the next portion of text
                remainingText = remainingText.Substring(chunkLength);
                textPosition += chunkLength;
            }
        }

        return chunks;
    }
}