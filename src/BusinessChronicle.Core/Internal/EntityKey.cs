using BusinessChronicle.Abstractions.Identifiers;
using BusinessChronicle.Abstractions.Models;

namespace BusinessChronicle.Internal;

internal static class EntityKey
{
    internal static string Create(EntityReference entity) =>
        string.Create(null, stackalloc char[entity.Id.Value.Length + (entity.EntityType?.Length ?? 0) + 1],
            $"{entity.EntityType}\u001f{entity.Id.Value}");

    internal static string Create(EntityId id, string? entityType) =>
        string.Create(null, stackalloc char[id.Value.Length + (entityType?.Length ?? 0) + 1],
            $"{entityType}\u001f{id.Value}");
}
