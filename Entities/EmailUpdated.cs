namespace FastExpressionCompilerBenchmarks.Entities;

public sealed record EmailUpdated(Guid AggregateId, int Version, string VerifiedEmail) : EventBase(AggregateId, Version);
