using BenchmarkDotNet.Running;
using BusinessChronicle.Benchmarks;

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
