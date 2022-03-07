# ExperimentWithCompileFast
The [FastExpressionCompiler](https://github.com/dadhi/FastExpressionCompiler) library sounds really interesting for I have and do work on - can it really help?

I've taken a greatly simplified version of some code that I've been looking at and experimented with performance differences (in .NET 6.0 only, currently, in case that matters) between code written at compile that is more likely to be inline-able, code written at compile time that is used to set a `Func` property and called that way, code written at compile time that uses LINQ Expressions to compile the same `Func` with types known at compile time *and* a code written at compile time that uses LINQ Expressions to compile a `Func` but where one of the types is not known at compile time and so some casting is required.

I tried a few different casting mechanisms (casting the entire delegate vs casting the specific argument) and I tried using the FastExpressionCompiler's `CompileFast` method (*and* its `LightExpression` types) to see if that could make it better - alas, it made it worse.

## Results

As with regular [BenchmarkDotNet](https://benchmarkdotnet.org/) projects, run from the command line in release build - ie. `dotnet run -c Release` from the solution root.

|                                           Method |      Mean |     Error |    StdDev | Notes
|------------------------------------------------- |----------:|----------:|----------:|------
|                            ViaDirectCompiledCode |  8.597 ns | 0.2278 ns | 0.2237 ns | The update applied directly - not via `Func` property call
|                                  ViaCompiledCode |  9.623 ns | 0.2597 ns | 0.3725 ns | A `Func` created with compiled code and called to apply the update
|                                ViaLinqExpression |  9.846 ns | 0.2614 ns | 0.4295 ns | Best case for LINQ Expressions - all types known at compile time
|                     ViaLinqExpressionCompileFast |  9.965 ns | 0.2195 ns | 0.1833 ns | The above but with `CompileFast` - no real improvement / marginally worse
|            ViaLinqExpressionWithOnlyEventCasting | 10.951 ns | 0.2906 ns | 0.3347 ns | The real case that I'm interested in - the target type known at compile time but the event type not (and so it will be cast from object when the update is applied)
| ViaLinqExpressionWithOnlyEventCastingCompileFast | 11.362 ns | 0.2963 ns | 0.2910 ns | The above but with `CompileFast` - no real improvement / marginally worse
|                     ViaLinqExpressionWithCasting | 12.886 ns | 0.3111 ns | 0.2910 ns | A variation on the LINQ Expression approach where NO types are known at compile time and the two inputs must be cast, as must the return type - not applicable to what I'm looking at right now but interesting for curiosity sake
|          ViaLinqExpressionWithCastingCompileFast | 12.857 ns | 0.2250 ns | 0.2104 ns | The above but with `CompileFast` - no real improvement
|                    ViaLightExpressionWithCasting | 13.617 ns | 0.3406 ns | 0.2844 ns | The above with `LightExpression` *and* `CompileFast` - no improvement / marginally worse
|          ViaLinqExpressionWithSingleDelegateCast | 16.169 ns | 0.3914 ns | 0.8914 ns | An alternate approach to casting (casting the delegate, instead of the unknown argument - clearly the worst)

The really important results are these two:

|                                           Method |      Mean |     Error |    StdDev |
|------------------------------------------------- |----------:|----------:|----------:|
|                            ViaDirectCompiledCode |  8.597 ns | 0.2278 ns | 0.2237 ns |
|            ViaLinqExpressionWithOnlyEventCasting | 10.951 ns | 0.2906 ns | 0.3347 ns |

.. as they demonstrate the performance difference (**approx 1.27x slower** using LINQ Expressions where one of the types is not known at compile time vs hand-written code where all types *are* known at compile time).

However, it's also interesting to note these two:

|                                           Method |      Mean |     Error |    StdDev |
|------------------------------------------------- |----------:|----------:|----------:|
|                            ViaDirectCompiledCode |  8.597 ns | 0.2278 ns | 0.2237 ns |
|                                ViaLinqExpression |  9.846 ns | 0.2614 ns | 0.4295 ns |

.. as they demonstrate that when the playing field is a little more equal and a compiled LINQ Expression is used that *does* know the types at compile time (as the hand-written code does) then the results are very close (approx 1.15x slower).
