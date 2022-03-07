using System.Linq.Expressions;
using System.Reflection;
using FastExpressionCompiler;
using FastExpressionCompilerBenchmarks.Entities;

namespace FastExpressionCompilerBenchmarks.PerformanceVariations;

/// <summary>
/// This is a variation on LinqExpressionCompile that uses FastExpressionCompiler' CompileFast method but is not
/// different it any other way - just to see if there were performance gains to be had
/// </summary>
internal static class LinqExpressionCompileFast
{
    static LinqExpressionCompileFast()
    {
        var applyMethod = typeof(Account)
            .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(m => m.Name == nameof(Account.Apply))
            .Select(m => (Method: m, Parameters: m.GetParameters()))
            .Where(m => (m.Parameters.Length == 1) && (m.Parameters[0].ParameterType == typeof(EmailUpdated)))
            .Where(m => typeof(Account).IsAssignableFrom(m.Method.ReturnType))
            .Select(m => m.Method)
            .Single();

        var aggregateParameter = Expression.Parameter(typeof(Account), "aggregate");
        var eventToReflectParameter = Expression.Parameter(typeof(EmailUpdated), "eventToReflect");
        var methodCall = Expression.Call(aggregateParameter, applyMethod, eventToReflectParameter);
        Apply = Expression
            .Lambda<Func<Account, EmailUpdated, Account>>(methodCall, aggregateParameter, eventToReflectParameter)
            .CompileFast(ifFastFailedReturnNull: true) ?? throw new Exception("CompileFast could not be applied");
    }

    public static Func<Account, EmailUpdated, Account> Apply { get; }
}