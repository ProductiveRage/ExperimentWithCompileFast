using System.Reflection;
using FastExpressionCompiler.LightExpression;
using FastExpressionCompilerBenchmarks.Entities;

namespace FastExpressionCompilerBenchmarks.PerformanceVariations;

/// <summary>
/// This is a variation on LinqExpressionWithCastingCompileFast that uses FastExpressionCompiler' Expressions as well
/// as its CompileFast method to see if there were performance gains to be had by taking that approach
/// </summary>
internal static class LightExpressionWithCasting
{
    private delegate object UpdateHandler(object aggregate, object eventToReflect);

    static LightExpressionWithCasting()
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
            .CompileFast(ifFastFailedReturnNull: true) ?? throw new Exception("CompileFast could not be applied");

        Apply = (account, emailUpdated) => (Account)updateHandler(account, emailUpdated);
    }

    public static Func<Account, EmailUpdated, Account> Apply { get; }
}