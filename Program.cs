using BenchmarkDotNet.Running;
using FastExpressionCompilerBenchmarks;

var summary = BenchmarkRunner.Run<GeneratedCodePerformanceComparisons>();
Console.WriteLine(summary);