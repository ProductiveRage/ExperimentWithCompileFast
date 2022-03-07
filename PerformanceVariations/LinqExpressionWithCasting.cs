using System.Linq.Expressions;
using System.Reflection;
using FastExpressionCompilerBenchmarks.Entities;

namespace FastExpressionCompilerBenchmarks.PerformanceVariations;

/// <summary>
/// This generates an update handler that takes an aggregate and event parameters as objects and returns an object - as
/// such, it requires three casts to perform the update.
/// 
/// Note: This is what I was previously using in my Event Sourcing demo experiments but that is no longer the case (that
/// currently only has to cast the event type, the aggregate type is known at compile time)
/// </summary>
internal static class LinqExpressionWithCasting
{
    private delegate object UpdateHandler(object aggregate, object eventToReflect);

    static LinqExpressionWithCasting()
    {
        var applyMethod = typeof(Account)
            .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(m => m.Name == nameof(Account.Apply))
            .Select(m => (Method: m, Parameters: m.GetParameters()))
            .Where(m => (m.Parameters.Length == 1) && (m.Parameters[0].ParameterType == typeof(EmailUpdated)))
            .Where(m => typeof(Account).IsAssignableFrom(m.Method.ReturnType))
            .Select(m => m.Method)
            .Single();

        var aggregateParameter = Expression.Parameter(typeof(object), "aggregate");
        var eventToReflectParameter = Expression.Parameter(typeof(object), "eventToReflect");
        var methodCall = Expression.Call(
            Expression.Convert(aggregateParameter, typeof(Account)),
            applyMethod,
            Expression.Convert(eventToReflectParameter, typeof(EmailUpdated)));
        var updateHandler = Expression
            .Lambda<UpdateHandler>(methodCall, aggregateParameter, eventToReflectParameter)
            .Compile();

        Apply = (account, emailUpdated) => (Account)updateHandler(account, emailUpdated);
    }

    public static Func<Account, EmailUpdated, Account> Apply { get; }
}