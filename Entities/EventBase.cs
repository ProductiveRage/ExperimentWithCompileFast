using System.Text.Json.Serialization;

namespace FastExpressionCompilerBenchmarks.Entities;

public abstract record EventBase
{
    [JsonIgnore]
    public Guid AggregateId { get; internal set; }

    [JsonIgnore]
    public int Version { get; internal set; }

    [JsonIgnore]
    public DateTime Timestamp { get; internal set; }

    [JsonIgnore]
    public long EventId { get; internal set; }

    protected EventBase(Guid aggregateId, int version)
    {
        AggregateId = aggregateId;
        Version = version;
        Timestamp = DateTime.UtcNow;
    }
}
