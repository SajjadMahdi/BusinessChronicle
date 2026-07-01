namespace BusinessChronicle.Abstractions.Models;

/// <summary>
/// Lightweight pointer to a revision within the version graph.
/// </summary>
/// <param name="Id">The revision identifier.</param>
/// <param name="Entity">The entity the revision belongs to.</param>
public readonly record struct RevisionReference(Identifiers.RevisionId Id, EntityReference Entity);
