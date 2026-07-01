namespace BusinessChronicle.Domain;

/// <summary>
/// Internal domain limits enforced by validation and builders.
/// </summary>
internal static class ChronicleDomainLimits
{
    internal const int MaxCommitMessageLength = 4000;
    internal const int MaxShortDescriptionLength = 256;
    internal const int MaxDisplayNameLength = 512;
    internal const int MaxEntityTypeLength = 256;
    internal const int MaxActorIdLength = 256;
    internal const int MaxTagNameLength = 128;
    internal const int MaxCommentLength = 8000;
    internal const int MaxChangePathLength = 1024;
    internal const int MaxPropertyNameLength = 256;
}
