# RecursiveTextSplitter User Guide

## Overview

The **RecursiveTextSplitter** is a C# library that provides intelligent text splitting functionality with semantic awareness. Unlike simple character-based splitting, this library attempts to preserve meaningful boundaries by using a hierarchical approach to text segmentation, from paragraph breaks down to character-level splitting as a last resort.

## Key Features

- **Semantic Awareness**: Maintains natural text boundaries (paragraphs, sentences, words)
- **Configurable Overlap**: Supports overlapping chunks for better context preservation
- **Flexible Separators**: Allows custom separator hierarchies or uses intelligent defaults
- **Detailed Metadata**: Provides comprehensive information about each chunk including position data
- **Line Ending Preservation**: Maintains original line ending formats across different platforms
- **Word-Safe Overlap**: Ensures overlap occurs at natural word boundaries

## Installation

Add the namespace to your C# project:

```csharp
using RecursiveTextSplitting;
```

## Basic Usage

### Simple Text Splitting

The most straightforward way to split text is using the `RecursiveSplit` extension method:

```csharp
string document = "Artificial intelligence is transforming every industry. From healthcare to finance, automation is becoming smarter and more adaptive. However, challenges like bias, interpretability, and safety remain important areas of research.";

var chunks = document.RecursiveSplit(chunkSize: 80, chunkOverlap: 0);

foreach (var chunk in chunks)
{
    Console.WriteLine($"Chunk: {chunk}");
    Console.WriteLine("---");
}
```

### Advanced Splitting with Metadata

For more detailed information about each chunk, use the `AdvancedRecursiveSplit` method:

```csharp
string document = "Artificial intelligence is transforming every industry. From healthcare to finance, automation is becoming smarter and more adaptive. However, challenges like bias, interpretability, and safety remain important areas of research.";

var chunks = document.AdvancedRecursiveSplit(chunkSize: 80, chunkOverlap: 0);

foreach (var chunk in chunks)
{
    Console.WriteLine($"Chunk {chunk.ChunkIndex}: {chunk.Text}");
    Console.WriteLine($"Start Position: {chunk.StartPosition}");
    Console.WriteLine($"End Position: {chunk.EndPosition}");
    Console.WriteLine($"Separator Used: {chunk.SeparatorUsed}");
    Console.WriteLine("---");
}
```

## Working with Overlap

Overlap allows consecutive chunks to share some content, which is particularly useful for maintaining context in applications like search indexing or machine learning.

### Basic Overlap Example

```csharp
string document = "Artificial intelligence is transforming every industry. From healthcare to finance, automation is becoming smarter and more adaptive. However, challenges like bias, interpretability, and safety remain important areas of research.";

// Split with 25 characters of overlap
var chunks = document.RecursiveSplit(chunkSize: 80, chunkOverlap: 25);

foreach (var chunk in chunks)
{
    Console.WriteLine($"Chunk: {chunk}");
    Console.WriteLine("---");
}
```

### Advanced Overlap with Metadata

```csharp
string document = "Artificial intelligence is transforming every industry. From healthcare to finance, automation is becoming smarter and more adaptive. However, challenges like bias, interpretability, and safety remain important areas of research.";

var chunks = document.AdvancedRecursiveSplit(chunkSize: 80, chunkOverlap: 25);

foreach (var chunk in chunks)
{
    Console.WriteLine($"Chunk {chunk.ChunkIndex}:");
    Console.WriteLine($"  Full Text: {chunk.Text}");
    Console.WriteLine($"  Overlap: '{chunk.OverlapText}'");
    Console.WriteLine($"  Original Content: '{chunk.ChunkText}'");
    Console.WriteLine($"  Position: {chunk.StartPosition}-{chunk.EndPosition}");
    Console.WriteLine("---");
}
```

## Understanding the TextChunk Class

The `TextChunk` class provides comprehensive metadata about each split segment:

```csharp
public class TextChunk
{
    public string Text { get; set; }           // Complete text including overlap
    public string OverlapText { get; set; }    // Only the overlap portion
    public string ChunkText { get; set; }      // Original chunk without overlap
    public int StartPosition { get; set; }     // Start position in original text
    public int EndPosition { get; set; }       // End position in original text
    public string SeparatorUsed { get; set; }  // Separator that created this chunk
    public int ChunkIndex { get; set; }        // Sequential chunk number
}
```

## Separator Hierarchy

The library uses a hierarchical approach to splitting, trying larger semantic units first:

1. **Paragraph breaks** (`\n\n`) - Largest semantic units
2. **Sentence endings with newlines** (`.\n`, `!\n`, `?\n`)
3. **Other punctuation with newlines** (`:\n`, `;\n`)
4. **Single newlines** (`\n`) - Line breaks
5. **Sentence endings with spaces** (`. `, `! `, `? `)
6. **Punctuation with spaces** (`; `, `, `)
7. **Word boundaries** (` `) - Single spaces
8. **Character-by-character** - Last resort

## Contributing

We welcome contributions to make RecursiveTextSplitter even better! Here are some ways you can help:

### 🌟 **Star this repository** if you find it useful!

Your star helps others discover this library and motivates continued development.

### 💡 **Feature Ideas & Improvements**

Have ideas for new features or improvements? We'd love to hear them! Some areas we're considering:

- **Additional splitting strategies** (e.g., token-based, language-specific)
- **Performance optimizations** for very large documents
- **Async/await support** for processing large files
- **Custom metadata extraction** during splitting
- **Integration with popular text processing libraries**
- **Support for different text encodings**
- **Streaming text splitting** for memory efficiency

### 🔧 **Pull Requests Welcome**

We're open to pull requests! Whether you want to:

- Fix bugs or improve existing functionality
- Add new features or splitting strategies
- Improve documentation or examples
- Optimize performance
- ...

Please feel free to fork the repository and submit a pull request. For larger changes, consider opening an issue first to discuss your approach.

### 📝 **Reporting Issues**

Found a bug or have a suggestion? Please open an issue with:

- A clear description of the problem or enhancement
- Steps to reproduce (for bugs)
- Sample code demonstrating the issue
- Expected vs actual behavior