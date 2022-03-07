using System.Linq.Expressions;
using System.Reflection;
using FastExpressionCompilerBenchmarks.Entities;

namespace FastExpressionCompilerBenchmarks.PerformanceVariations;

/// <summary>
/// This is the best case for a compiled LINQ expression - where the aggregate and event types are known at compile time.
/// It is included here to compare the performance of this to direct code and to compare it to the performance of variations
/// that do not know any/all of the types at compile time and so more casting is required at runtime.
/// 
/// Note: Since no runtime casting is required here, it is not representative of what is done in the Event Sourcing demo variant
/// that I was fiddling with - it is for 'best case' performance comparison purposes only.
/// </summary>
internal static class LinqExpression
{
    static LinqExpression()
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
            .Compile();
    }

    public static Func<Account, EmailUpdated, Account> Apply { get; }
}