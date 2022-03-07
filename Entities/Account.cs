namespace FastExpressionCompilerBenchmarks.Entities;

public sealed record Account
{
    public Account(Guid id, int version, Guid systemId, string verifiedEmail)
    {
        Id = id;
        Version = version;
        VerifiedEmail = verifiedEmail;
        SystemId = systemId;
    }

    public Guid Id { get; }
    public int Version { get; init; }
    public bool IsDeleted { get; init; }

    public string VerifiedEmail { get; init; }
    public Guid SystemId { get; }

    // Note: Only internal (rather than private) so that we can call it directly for performance comparisons
    // (I don't think that we would want Apply methods like this to be more accessible than private in the
    // general case because there is no validation performed here)
    internal Account Apply(EmailUpdated change) =>
        this with { Version = change.Version, IsDeleted = false, VerifiedEmail = change.VerifiedEmail };
}
