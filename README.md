# RecursiveTextSplitter User Guide

## Overview

The **RecursiveTextSplitter** is a C# library that provides intelligent text splitting functionality with semantic awareness. Unlike simple character-based splitting, this library attempts to preserve meaningful boundaries by using a hierarchical approach to text segmentation, from paragraph breaks down to character-level splitting as a last resort.

## Key Features

- **Semantic Awareness**: Maintains natural text boundaries (paragraphs, sentences, words)
- **Configurable Overlap**: Supports overlapping chunks for better context preservation
- **Flexible Separators**: Allows custom separator hierarchies or uses intelligent defaults
- **Detailed Metadata**: Provides comprehensive information about each chunk including position data and line/column tracking
- **Word-Safe Overlap**: Ensures overlap occurs at natural word boundaries
- **Position Tracking**: Tracks both character positions and line/column coordinates in the original text

## Installation

### Via NuGet Package Manager

Install the RecursiveTextSplitter package from NuGet:

```bash
dotnet add package RecursiveTextSplitter
```

Or via Package Manager Console in Visual Studio:

```powershell
Install-Package RecursiveTextSplitter
```

Or search for "RecursiveTextSplitter" in the Visual Studio NuGet Package Manager UI.

**NuGet Package:** https://www.nuget.org/packages/RecursiveTextSplitter/

### Usage

Add the namespace to your C# project:

```csharp
using RecursiveTextSplitting;
```

## Basic Usage

### Simple Text Splitting

The most straightforward way to split text is using the `RecursiveSplit` extension method:

```csharp
string document = "Artificial intelligence is transforming every industry.\nFrom healthcare to finance, automation is becoming smarter and more adaptive.\n\nHowever, challenges like bias, interpretability, and safety remain important areas of research.";

var chunks = document.RecursiveSplit(chunkSize: 80, chunkOverlap: 0);

foreach (var chunk in chunks)
{
    Console.WriteLine($"Chunk: {chunk}");
    Console.WriteLine("---");
}
```

### Advanced Splitting with Metadata

For more detailed information about each chunk, including line and column positions, use the `AdvancedRecursiveSplit` method:

```csharp
string document = "Artificial intelligence is transforming every industry.\nFrom healthcare to finance, automation is becoming smarter and more adaptive.\n\nHowever, challenges like bias, interpretability, and safety remain important areas of research.";

var chunks = document.AdvancedRecursiveSplit(chunkSize: 80, chunkOverlap: 0);

foreach (var chunk in chunks)
{
    Console.WriteLine($"Chunk {chunk.ChunkIndex}: {chunk.Text}");
    Console.WriteLine($"Start Position: {chunk.StartPosition} (Line {chunk.StartLine}, Column {chunk.StartColumn})");
    Console.WriteLine($"End Position: {chunk.EndPosition} (Line {chunk.EndLine}, Column {chunk.EndColumn})");
    Console.WriteLine($"Separator Used: {chunk.SeparatorUsed}");
    Console.WriteLine("---");
}
```

## Working with Overlap

Overlap allows consecutive chunks to share some content, which is particularly useful for maintaining context in applications like search indexing or machine learning.

### Basic Overlap Example

```csharp
string document = "Artificial intelligence is transforming every industry.\nFrom healthcare to finance, automation is becoming smarter and more adaptive.\n\nHowever, challenges like bias, interpretability, and safety remain important areas of research.";

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
string document = "Artificial intelligence is transforming every industry.\nFrom healthcare to finance, automation is becoming smarter and more adaptive.\n\nHowever, challenges like bias, interpretability, and safety remain important areas of research.";

var chunks = document.AdvancedRecursiveSplit(chunkSize: 80, chunkOverlap: 25);

foreach (var chunk in chunks)
{
    Console.WriteLine($"Chunk {chunk.ChunkIndex}:");
    Console.WriteLine($"  Full Text: {chunk.Text}");
    Console.WriteLine($"  Overlap: '{chunk.OverlapText}'");
    Console.WriteLine($"  Original Content: '{chunk.ChunkText}'");
    Console.WriteLine($"  Position: {chunk.StartPosition}-{chunk.EndPosition}");
    Console.WriteLine($"  Location: Lines {chunk.StartLine}-{chunk.EndLine}");
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
    public int StartPosition { get; set; }     // 1-based start position in original text
    public int EndPosition { get; set; }       // 1-based end position in original text
    public string SeparatorUsed { get; set; }  // Separator that created this chunk
    public int ChunkIndex { get; set; }        // Sequential chunk number (1-based)
    public int StartColumn { get; set; }       // 1-based column where chunk starts
    public int StartLine { get; set; }         // 1-based line where chunk starts
    public int EndColumn { get; set; }         // 1-based column where chunk ends
    public int EndLine { get; set; }           // 1-based line where chunk ends
}
```

### Position Tracking Features

The library now provides detailed position tracking with both character-level and line/column coordinates:

- **Character Positions**: `StartPosition` and `EndPosition` provide 1-based character indices in the original text
- **Line/Column Tracking**: `StartLine`, `StartColumn`, `EndLine`, `EndColumn` provide 1-based line and column coordinates
- **Comprehensive Coverage**: All positions are tracked accurately even when overlap is applied

## Custom Separators

You can provide your own separator hierarchy for specialized splitting needs:

```csharp
string document = "Section 1|Subsection A;Item 1,Item 2|Section 2;Item 3";

// Custom separators prioritizing sections, then subsections, then items
string[] customSeparators = { "|", ";", "," };

var chunks = document.AdvancedRecursiveSplit(
    chunkSize: 20, 
    chunkOverlap: 0, 
    separators: customSeparators
);

foreach (var chunk in chunks)
{
    Console.WriteLine($"Chunk: {chunk.Text}");
    Console.WriteLine($"Split using: {chunk.SeparatorUsed}");
    Console.WriteLine($"At line {chunk.StartLine}, column {chunk.StartColumn}");
    Console.WriteLine("---");
}
```

## Separator Hierarchy

The library uses a hierarchical approach to splitting, trying larger semantic units first:

1. **Paragraph breaks** (`\r\n\r\n`, `\n\n`) - Largest semantic units
2. **Sentence endings with line breaks** (`.\r\n`, `!\r\n`, `?\r\n`, `:\r\n`, `;\r\n`)
3. **Single line breaks** (`\r\n`)
4. **Sentence endings with newlines** (`.\n`, `!\n`, `?\n`, `:\n`, `;\n`)
5. **Single newlines** (`\n`)
6. **Sentence endings with spaces** (`. `, `! `, `? `)
7. **Punctuation with spaces** (`; `, `, `)
8. **Word boundaries** (` `) - Single spaces
9. **Character-by-character** (`""`) - Last resort

## Contributing

We welcome contributions to make RecursiveTextSplitter even better! Here are some ways you can help:

### 🌟 **Star this repository** if you find it useful!

Your star helps others discover this library and motivates continued development.

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
