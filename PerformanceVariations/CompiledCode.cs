using FastExpressionCompilerBenchmarks.Entities;

namespace FastExpressionCompilerBenchmarks.PerformanceVariations;

/// <summary>
/// This is used to compare the behaviour of a Func that is generated from compile-time-written code to that or
/// delegates that are compiled at runtime
/// </summary>
internal static class CompiledCode
{
    static CompiledCode() => Apply = (account, emailUpdated) => account.Apply(emailUpdated);

    public static Func<Account, EmailUpdated, Account> Apply { get; }
}