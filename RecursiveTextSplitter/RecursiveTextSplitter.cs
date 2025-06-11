// ReSharper disable UnusedMember.Global

using System.Text;

namespace RecursiveTextSplitting;

/// <summary>
/// Provides recursive text splitting functionality with semantic awareness and configurable overlap.
/// This splitter attempts to maintain semantic boundaries by using a hierarchy of separators,
/// from paragraph breaks down to character-level splitting as a last resort.
/// </summary>
public static class RecursiveTextSplitter
{
    /// <summary>
    /// Recursively splits text into chunks of specified size with optional overlap, returning only the text content.
    /// </summary>
    /// <param name="text">The input text to split into chunks.</param>
    /// <param name="chunkSize">The maximum size of each chunk in characters.</param>
    /// <param name="chunkOverlap">The number of characters to overlap between consecutive chunks. Default is 0.</param>
    /// <param name="separators">Custom array of separators to use for splitting. If null, default semantic separators are used.</param>
    /// <returns>An enumerable collection of text strings representing the chunks.</returns>
    /// <exception cref="ArgumentException">Thrown when chunkSize is not positive, chunkOverlap is negative, or chunkOverlap is greater than or equal to chunkSize.</exception>
    /// <example>
    /// <code>
    /// string document = "This is a sample document. It has multiple sentences.";
    /// var chunks = document.RecursiveSplit(chunkSize: 30, chunkOverlap: 5);
    /// foreach (var chunk in chunks)
    /// {
    ///     Console.WriteLine(chunk);
    /// }
    /// </code>
    /// </example>
    public static IEnumerable<string> RecursiveSplit(this string text, int chunkSize, int chunkOverlap = 0
        , string[]? separators = null)
    {
        foreach (var result in text.AdvancedRecursiveSplit(chunkSize, chunkOverlap, separators))
        {
            yield return result.Text;
        }
    }

    /// <summary>
    /// Recursively splits text into chunks with detailed metadata about each chunk including overlap information and position data.
    /// This method provides the most comprehensive splitting functionality with full chunk metadata.
    /// </summary>
    /// <param name="text">The input text to split into chunks.</param>
    /// <param name="chunkSize">The maximum size of each chunk in characters.</param>
    /// <param name="chunkOverlap">The number of characters to overlap between consecutive chunks. Default is 0.</param>
    /// <param name="separators">Custom array of separators to use for splitting. If null, default semantic separators are used.</param>
    /// <returns>An enumerable collection of <see cref="TextChunk"/> objects containing the text and metadata.</returns>
    /// <exception cref="ArgumentException">Thrown when chunkSize is not positive, chunkOverlap is negative, or chunkOverlap is greater than or equal to chunkSize.</exception>
    /// <remarks>
    /// The splitting process follows a hierarchical approach:
    /// 1. Paragraph breaks (\n\n)
    /// 2. Sentence endings with newlines (.\n, !\n, ?\n)
    /// 3. Other punctuation with newlines (:, ;)
    /// 4. Single newlines
    /// 5. Sentence endings with spaces
    /// 6. Word boundaries (spaces)
    /// 7. Character-by-character splitting (last resort)
    /// 
    /// Line endings are normalized during processing and restored to the original format in the output.
    /// </remarks>
    /// <example>
    /// <code>
    /// string document = "First paragraph.\n\nSecond paragraph with multiple sentences. This is another sentence.";
    /// var chunks = document.AdvancedRecursiveSplit(chunkSize: 50, chunkOverlap: 10);
    /// foreach (var chunk in chunks)
    /// {
    ///     Console.WriteLine($"Chunk {chunk.ChunkIndex}: {chunk.Text}");
    ///     Console.WriteLine($"Overlap: '{chunk.OverlapText}'");
    ///     Console.WriteLine($"Position: {chunk.StartPosition}-{chunk.EndPosition}");
    ///     Console.WriteLine($"Separator: {chunk.SeparatorUsed}");
    /// }
    /// </code>
    /// </example>
    public static IEnumerable<TextChunk> AdvancedRecursiveSplit(this string text, int chunkSize, int chunkOverlap = 0
        , string[]? separators = null)
    {
        if (string.IsNullOrEmpty(text))
            return [];

        if (chunkSize <= 0)
            throw new ArgumentException("Chunk size must be positive", nameof(chunkSize));

        if (chunkOverlap < 0)
            throw new ArgumentException("Chunk overlap cannot be negative", nameof(chunkOverlap));

        if (chunkOverlap >= chunkSize)
            throw new ArgumentException("Chunk overlap must be less than chunk size", nameof(chunkOverlap));

        var originalLineEnding = DetectLineEnding(text);
        var normalizedText = NormalizeLineEndings(text);

        if (separators == null || separators.Length == 0)
            separators ??= GetDefaultSeparators();

        var chunks = SplitRecursively(normalizedText, chunkSize, separators, 0, 0);

        if (chunkOverlap > 0 && chunks.Count > 1)
        {
            chunks = ApplyOverlap(chunks, chunkOverlap);
        }

        // Update chunk indices and restore original line endings
        for (int i = 0; i < chunks.Count; i++)
        {
            chunks[i].ChunkIndex = i;
            if (originalLineEnding != "\n")
            {
                chunks[i].Text = chunks[i].Text.Replace("\n", originalLineEnding);
                chunks[i].OverlapText = chunks[i].OverlapText.Replace("\n", originalLineEnding);
                chunks[i].ChunkText = chunks[i].ChunkText.Replace("\n", originalLineEnding);
            }
        }

        return chunks;
    }

    /// <summary>
    /// Applies overlap between consecutive chunks by taking text from the end of each chunk
    /// and prepending it to the next chunk, ensuring word-safe boundaries.
    /// </summary>
    /// <param name="chunks">The list of chunks to apply overlap to.</param>
    /// <param name="chunkOverlap">The maximum number of characters to overlap between chunks.</param>
    /// <returns>A new list of chunks with overlap applied.</returns>
    /// <remarks>
    /// The overlap is applied in a word-safe manner, meaning it will attempt to start the overlap
    /// at a word or punctuation boundary rather than splitting words in the middle.
    /// </remarks>
    private static List<TextChunk> ApplyOverlap(List<TextChunk> chunks, int chunkOverlap)
    {
        if (chunks.Count == 0)
            return chunks;

        var overlappedChunks = new List<TextChunk> { chunks[0] };
        var sb = new StringBuilder();

        for (int i = 1; i < chunks.Count; i++)
        {
            var prev = chunks[i - 1];
            var current = chunks[i];

            // Get word-safe overlap from end of previous chunk
            var overlap = GetWordSafeOverlap(prev.Text, chunkOverlap);

            // Build new chunk text: overlap + current content
            sb.Clear();
            sb.Append(overlap);
            sb.Append(current.Text);

            // Create new chunk with overlap information
            var newChunk = new TextChunk
            {
                Text = sb.ToString(), // Full text including overlap
                OverlapText = overlap, // Only the overlap portion
                ChunkText = current.Text, // Original chunk without overlap
                StartPosition = current.StartPosition,
                EndPosition = current.EndPosition,
                SeparatorUsed = current.SeparatorUsed,
                ChunkIndex = current.ChunkIndex // Will be updated later
            };

            overlappedChunks.Add(newChunk);
        }

        return overlappedChunks;
    }

    /// <summary>
    /// Extracts a word-safe overlap from the end of a text string, attempting to start at word boundaries.
    /// </summary>
    /// <param name="text">The source text to extract overlap from.</param>
    /// <param name="maxLength">The maximum length of overlap to extract.</param>
    /// <returns>A substring from the end of the text that starts at a word boundary when possible.</returns>
    /// <remarks>
    /// This method looks for natural break points such as spaces, punctuation, and brackets
    /// to avoid splitting words. If no suitable boundary is found, it returns the full requested length.
    /// </remarks>
    private static string GetWordSafeOverlap(string text, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(text) || maxLength <= 0)
            return string.Empty;

        // Calculate starting position from end of text
        var start = Math.Max(0, text.Length - maxLength);

        // Extract candidate overlap substring from end
        var candidate = text.Substring(start);

        // Find first word/sentence boundary character in candidate
        int wordStart = candidate.IndexOfAny([
            ' ', '\n', '\r', '\t', '\f',       // Whitespace characters
            '.', ',', ';', ':', '!', '?',      // Punctuation marks
            '(', ')', '[', ']', '{', '}',      // Brackets
            '\"', '\'', '`'                    // Quote characters
        ]);

        // If boundary found and not at end, start from after boundary (safe block)
        if (wordStart >= 0 && wordStart < candidate.Length - 1)
        {
            return candidate.Substring(wordStart + 1);
        }

        // No good boundary found, return full candidate
        return candidate;
    }

    /// <summary>
    /// Detects the line ending style used in the input text.
    /// </summary>
    /// <param name="text">The text to analyze for line ending patterns.</param>
    /// <returns>The detected line ending string: "\r\n" for Windows, "\r" for old Mac, or "\n" for Unix/Linux.</returns>
    /// <remarks>
    /// Windows-style CRLF (\r\n) is checked first as it contains both \r and \n characters.
    /// If no line endings are found, defaults to Unix-style LF (\n).
    /// </remarks>
    private static string DetectLineEnding(string text)
    {
        // Windows style (CRLF) - check first as it contains both \r and \n
        if (text.Contains("\r\n"))
            return "\r\n";

        // Old Mac style (CR only)
        if (text.Contains("\r"))
            return "\r";

        // Unix/Linux style (LF) - default fallback
        return "\n";
    }

    /// <summary>
    /// Gets the default hierarchy of separators used for semantic text splitting.
    /// </summary>
    /// <returns>An array of separator strings ordered from most to least semantic significance.</returns>
    /// <remarks>
    /// The default separators are arranged in hierarchical order:
    /// <list type="number">
    /// <item>"\n\n" - Paragraph breaks (largest semantic unit)</item>
    /// <item>".\n", "!\n", "?\n" - Sentence endings with newlines</item>
    /// <item>":\n", ";\n" - Other punctuation with newlines</item>
    /// <item>"\n" - Single newlines (line breaks)</item>
    /// <item>". ", "! ", "? " - Sentence endings with spaces</item>
    /// <item>"; ", ", " - Punctuation with spaces</item>
    /// <item>" " - Single spaces (word boundaries)</item>
    /// <item>"" - Character-by-character splitting (last resort)</item>
    /// </list>
    /// </remarks>
    private static string[] GetDefaultSeparators()
    {
        return [
            "\n\n",    // Paragraph breaks (largest semantic unit)
            ".\n",     // Sentence endings with newline
            "!\n",     // Exclamation with newline
            "?\n",     // Question with newline
            ":\n",     // Colon with newline
            ";\n",     // Semicolon with newline
            "\n",      // Single newlines (line breaks)
            ". ",      // Sentence endings with space
            "! ",      // Exclamation with space
            "? ",      // Question with space
            "; ",      // Semicolon with space
            ", ",      // Comma with space
            " ",       // Single spaces (word boundaries)
            ""         // Character-by-character (last resort)
        ];
    }

    /// <summary>
    /// Normalizes line endings in text to use Unix-style LF (\n) for consistent processing.
    /// </summary>
    /// <param name="text">The text with potentially mixed line endings.</param>
    /// <returns>Text with all line endings normalized to LF (\n).</returns>
    /// <remarks>
    /// This method converts both Windows CRLF (\r\n) and old Mac CR (\r) line endings to Unix LF (\n).
    /// The original line ending style is preserved and restored in the final output.
    /// </remarks>
    private static string NormalizeLineEndings(string text)
    {
        return text.Replace("\r\n", "\n").Replace("\r", "\n");
    }

    /// <summary>
    /// Recursively splits text using a hierarchy of separators, attempting to maintain semantic boundaries.
    /// </summary>
    /// <param name="text">The text to split.</param>
    /// <param name="chunkSize">The maximum size for each chunk.</param>
    /// <param name="separators">The array of separators to try in order.</param>
    /// <param name="separatorIndex">The current index in the separators array.</param>
    /// <param name="currentPosition">The current character position in the original document.</param>
    /// <returns>A list of text chunks with position and separator metadata.</returns>
    /// <remarks>
    /// This is the core recursive algorithm that:
    /// <list type="bullet">
    /// <item>Returns single chunk if text fits within size limit</item>
    /// <item>Tries current separator to split text at semantic boundaries</item>
    /// <item>Falls back to next separator level if current one doesn't work</item>
    /// <item>Forces character-level splitting as last resort</item>
    /// <item>Maintains position tracking throughout the process</item>
    /// </list>
    /// </remarks>
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
                    OverlapText = string.Empty, // No overlap for initial chunks
                    ChunkText = text, // Same as Text for chunks without overlap
                    StartPosition = currentPosition,
                    EndPosition = currentPosition + text.Length,
                    SeparatorUsed = separatorIndex < separators.Length ? separators[separatorIndex] : "none",
                    ChunkIndex = 0 // Will be set later
                });
            }
            return chunks;
        }

        // No more separators available, force split by character
        if (separatorIndex >= separators.Length)
        {
            for (var i = 0; i < text.Length; i += chunkSize)
            {
                var length = Math.Min(chunkSize, text.Length - i);
                var chunkText = text.Substring(i, length);

                chunks.Add(new TextChunk
                {
                    Text = chunkText,
                    OverlapText = string.Empty, // No overlap for initial chunks
                    ChunkText = chunkText, // Same as Text for chunks without overlap
                    StartPosition = currentPosition + i,
                    EndPosition = currentPosition + i + length,
                    SeparatorUsed = "char",
                    ChunkIndex = 0 // Will be set later
                });
            }
            return chunks;
        }

        var separator = separators[separatorIndex];
        var remainingText = text;
        var textPosition = currentPosition;

        // Handle empty separator (character-by-character splitting)
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
                    OverlapText = string.Empty, // No overlap for initial chunks
                    ChunkText = chunkText, // Same as Text for chunks without overlap
                    StartPosition = currentPosition + i,
                    EndPosition = currentPosition + i + length,
                    SeparatorUsed = "char",
                    ChunkIndex = 0 // Will be set later
                });
            }
            return chunks;
        }

        // Process text while there's content remaining
        while (remainingText.Length > 0)
        {
            // Remaining text fits in one chunk
            if (remainingText.Length <= chunkSize)
            {
                chunks.Add(new TextChunk
                {
                    Text = remainingText,
                    OverlapText = string.Empty, // No overlap for initial chunks
                    ChunkText = remainingText, // Same as Text for chunks without overlap
                    StartPosition = textPosition,
                    EndPosition = textPosition + remainingText.Length,
                    SeparatorUsed = separator,
                    ChunkIndex = 0 // Will be set later
                });
                break;
            }

            // Current separator not found, try next separator level
            if (!remainingText.Contains(separator))
            {
                var recursiveChunks = SplitRecursively(remainingText, chunkSize, separators, separatorIndex + 1, textPosition);
                chunks.AddRange(recursiveChunks);
                break;
            }

            // Find last occurrence of separator within chunk size limit
            var splitAt = remainingText.LastIndexOf(separator,
                Math.Min(chunkSize - 1, remainingText.Length - 1), StringComparison.Ordinal);

            // No separator found within chunk size
            if (splitAt == -1)
            {
                var firstSeparatorIndex = remainingText.IndexOf(separator, StringComparison.Ordinal);

                // Text starts with separator, skip it
                if (firstSeparatorIndex == 0)
                {
                    var separatorLength = separator.Length;
                    remainingText = remainingText.Substring(separatorLength);
                    textPosition += separatorLength;
                    continue;
                }

                // Extract oversized chunk before first separator
                var oversizedChunk = remainingText.Substring(0, firstSeparatorIndex);
                var recursiveChunks = SplitRecursively(oversizedChunk, chunkSize, separators, separatorIndex + 1, textPosition);
                chunks.AddRange(recursiveChunks);

                // Update remaining text and position
                var skipLength = firstSeparatorIndex + separator.Length;
                remainingText = remainingText.Substring(skipLength);
                textPosition += skipLength;
            }
            else
            {
                // Found good split point, create chunk including separator
                var chunkLength = splitAt + separator.Length;
                var chunkText = remainingText.Substring(0, chunkLength);

                chunks.Add(new TextChunk
                {
                    Text = chunkText,
                    OverlapText = string.Empty, // No overlap for initial chunks
                    ChunkText = chunkText, // Same as Text for chunks without overlap
                    StartPosition = textPosition,
                    EndPosition = textPosition + chunkLength,
                    SeparatorUsed = separator,
                    ChunkIndex = 0 // Will be set later
                });

                // Continue with text after the separator
                remainingText = remainingText.Substring(chunkLength);
                textPosition += chunkLength;
            }
        }

        return chunks;
    }
}