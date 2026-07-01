namespace BusinessChronicle.Abstractions.Enums;

/// <summary>
/// Classifies the origin of an actor that performed a chronicle operation.
/// </summary>
public enum ActorType
{
    /// <summary>
    /// An authenticated human user.
    /// </summary>
    User = 0,

    /// <summary>
    /// An automated system process without a human initiator.
    /// </summary>
    System = 1,

    /// <summary>
    /// A backend service or microservice identity.
    /// </summary>
    Service = 2,

    /// <summary>
    /// A client application identity.
    /// </summary>
    Application = 3,

    /// <summary>
    /// An unidentified or explicitly anonymous actor.
    /// </summary>
    Anonymous = 4,
}
