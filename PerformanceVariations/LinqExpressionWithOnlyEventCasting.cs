using System.Linq.Expressions;
using System.Reflection;
using FastExpressionCompilerBenchmarks.Entities;

namespace FastExpressionCompilerBenchmarks.PerformanceVariations;

/// <summary>
/// This compiles a LINQ Expression that knows the aggregate type at compile time but does not know the event type and so the
/// event will have to be cast in order for the update to be applied.
/// 
/// After recent changes to the Event Sourcing demo variation that I was fiddling with, this is the most accurate representation
/// of it as it currently stands and so the performance of this approach (in comparison to direct code) is what is most important.
/// </summary>
internal static class LinqExpressionWithOnlyEventCasting
{
    private delegate Account UpdateHandler(Account aggregate, object eventToReflect);

    static LinqExpressionWithOnlyEventCasting()
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
        var eventToReflectParameter = Expression.Parameter(typeof(object), "eventToReflect");
        var eventToReflectParameterCastToCommandType = Expression.Convert(eventToReflectParameter, typeof(EmailUpdated));
        var methodCall = Expression.Call(aggregateParameter, applyMethod, eventToReflectParameterCastToCommandType);
        var updateHandler = Expression
            .Lambda<UpdateHandler>(methodCall, aggregateParameter, eventToReflectParameter)
            .Compile();

        Apply = (account, emailUpdated) => updateHandler(account, emailUpdated);
    }

    public static Func<Account, EmailUpdated, Account> Apply { get; }
}