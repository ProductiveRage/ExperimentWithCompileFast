using System.Linq.Expressions;
using System.Reflection;
using FastExpressionCompilerBenchmarks.Entities;

namespace FastExpressionCompilerBenchmarks.PerformanceVariations;

/// <summary>
/// Rather than generating a Func and that takes an object-to-update, and event-to-reflect-object and returns a new object
/// (all of which must be cast in order for the correct types to be used), this generates a strongly-typed Func but records
/// it as a non-typed one that must be cast before calling - so there is only one cast instead of three.
/// 
/// Note: This was a experiment to investigate performance variations in casting and is not representative of what is done
/// in the Event Sourcing demo variant that I was fiddling with.
/// </summary>
internal static class LinqExpressionWithSingleDelegateCast
{
    static LinqExpressionWithSingleDelegateCast()
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
        Delegate updateHandler = Expression
            .Lambda<Func<Account, EmailUpdated, Account>>(methodCall, aggregateParameter, eventToReflectParameter)
            .Compile();

        Apply = (account, emailUpdated) => ((Func<Account, EmailUpdated, Account>)updateHandler)(account, emailUpdated);
    }

    public static Func<Account, EmailUpdated, Account> Apply { get; }
}