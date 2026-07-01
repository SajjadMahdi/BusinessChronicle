namespace BusinessChronicle.Abstractions.Models;

/// <summary>
/// A label attached to a chronicle artifact for classification or filtering.
/// </summary>
/// <param name="Name">The tag name.</param>
/// <param name="Value">An optional tag value.</param>
public readonly record struct ChronicleTag(string Name, string? Value = null);
