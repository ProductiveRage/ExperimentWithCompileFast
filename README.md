# ExperimentWithCompileFast
The [FastExpressionCompiler](https://github.com/dadhi/FastExpressionCompiler) library sounds really interesting for I have and do work on - can it really help?

I've taken a greatly simplified version of some code that I've been looking at and experimented with performance differences (in .NET 6.0 only, currently, in case that matters) between code written at compile that is more likely to be inline-able, code written at compile time that is used to set a `Func` property and called that way, code written at compile time that uses LINQ Expressions to compile the same `Func` with types known at compile time *and* a code written at compile time that uses LINQ Expressions to compile a `Func` but where one of the types is not known at compile time and so some casting is required.

I tried a few different casting mechanisms (casting the entire delegate vs casting the specific argument) and I tried using the FastExpressionCompiler's `CompileFast` method (*and* its `LightExpression` types) to see if that could make it better - alas, it made it worse.
