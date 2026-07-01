namespace BusinessChronicle.Internal;

internal static class IdGenerator
{
    internal static string NewId() => Guid.NewGuid().ToString("N");
}
