using BenchmarkDotNet.Attributes;
using FastExpressionCompilerBenchmarks.PerformanceVariations;
using FastExpressionCompilerBenchmarks.Entities;

namespace FastExpressionCompilerBenchmarks;

public class GeneratedCodePerformanceComparisons
{
    private readonly Account _account;
    private readonly EmailUpdated _eventToReflect;
    public GeneratedCodePerformanceComparisons()
    {
        _account = new Account(
            id: Guid.Parse("7327d9ab-34ba-4b72-bb06-4afaaf7ef869"),
            version: 1,
            systemId: Guid.Parse("2e2dc0a6-439c-414f-bbd8-a59fcfa1bf69"),
            verifiedEmail: "dan@example.com");
        
        _eventToReflect = new EmailUpdated(_account.Id, _account.Version + 1, "bob@example.com");
    }

    /// <summary>
    /// This POTENTIALLY allows more inlining than the Func in the CompileCode class? It should give us an indication
    /// of the absolute best performance option.
    /// </summary>
    [Benchmark]
    public Account ViaDirectCompiledCode() => _account.Apply(_eventToReflect);

    /// <summary>
    /// This applies the update via a typed Func that is compile using regular C# code
    /// </summary>
    [Benchmark]
    public Account ViaCompiledCode() => CompiledCode.Apply(_account, _eventToReflect);

    /// <summary>
    /// This applies the update via a compiled LINQ Expression that generates a Func that takes Account and EmailUpdated
    /// types and returns a new Account
    /// </summary>
    [Benchmark]
    public Account ViaLinqExpression() => LinqExpression.Apply(_account, _eventToReflect);

    /// <summary>
    /// This applies the update via compiled LINQ Expression that generates a Func that takes the aggregate and update references
    /// and returns an aggregate instance - the aggregate type is known at compile time while the event type is not and so it will
    /// have to be cast when the compiled expression is executed. This is the closest representation of the code that is currently
    /// in the Event Sourcing demo variation that I was working on.
    /// </summary>
    [Benchmark]
    public Account ViaLinqExpressionWithOnlyEventCasting() => LinqExpressionWithOnlyEventCasting.Apply(_account, _eventToReflect);

    /// <summary>
    /// This is a variation of ViaLinqExpressionWithOnlyEventCasting that uses 'CompileFast' to see if that can generate a more
    /// efficient delegate
    /// </summary>
    [Benchmark]
    public Account ViaLinqExpressionWithOnlyEventCastingCompileFast() => LinqExpressionWithOnlyEventCastingCompileFast.Apply(_account, _eventToReflect);

    /// <summary>
    /// This is the same as ViaLinqExpression except that it uses FastExpressionCompiler's CompileFast to compile the
    /// LINQ Expression
    /// </summary>
    [Benchmark]
    public Account ViaLinqExpressionCompileFast() => LinqExpressionCompileFast.Apply(_account, _eventToReflect);

    /// <summary>
    /// This applies the update via a compiled LINQ Expression that generates a Func that takes the aggregate and update
    /// types as object and casts them to Account and EmailUpdated when the method is called and then casts the return
    /// value back to Account (which most closely resembles what I have in my current Event Sourcing demo variant)
    /// </summary>
    [Benchmark]
    public Account ViaLinqExpressionWithCasting() => LinqExpressionWithCasting.Apply(_account, _eventToReflect);

    /// <summary>
    /// This is the same as ViaLinqExpressionWithCasting except that it uses FastExpressionCompiler's CompileFast to compile
    /// the LINQ Expression
    /// </summary>
    [Benchmark]
    public Account ViaLinqExpressionWithCastingCompileFast() => LinqExpressionWithCastingCompileFast.Apply(_account, _eventToReflect);

    /// <summary>
    /// This applies the update via a compiled FastExpressionCompiler.LightExpression (rather than a LINQ Expression) but
    /// still generates a Func that receives and returns object types that are then cast as required
    /// </summary>
    [Benchmark]
    public Account ViaLightExpressionWithCasting () => LightExpressionWithCasting.Apply(_account, _eventToReflect);

    /// <summary>
    /// This generates a strongly-typed delegate but holds the reference as a non-generic version which must be cast before it can
    /// be called - however, this is only one cast as opposed to generating a non-generic delegate whose two inputs and whose output
    /// must be individually cast (which is what ViaLinqExpressionWithCasting tests) - hopefully one cast is quicker than three!
    /// </summary>
    [Benchmark]
    public Account ViaLinqExpressionWithSingleDelegateCast() => LinqExpressionWithSingleDelegateCast.Apply(_account, _eventToReflect);
}