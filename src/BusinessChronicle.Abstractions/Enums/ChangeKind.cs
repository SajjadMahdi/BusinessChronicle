namespace BusinessChronicle.Abstractions.Enums;

/// <summary>
/// Describes the kind of structural change captured by a <see cref="Models.ChangeDescriptor"/>.
/// </summary>
public enum ChangeKind
{
    /// <summary>
    /// A member or element was added.
    /// </summary>
    Added = 0,

    /// <summary>
    /// A member or element was modified.
    /// </summary>
    Modified = 1,

    /// <summary>
    /// A member or element was removed.
    /// </summary>
    Removed = 2,

    /// <summary>
    /// A member or element was renamed.
    /// </summary>
    Renamed = 3,
}
