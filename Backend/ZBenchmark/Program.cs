using System;
using System.IO;
using BenchmarkDotNet;
using BenchmarkDotNet.Running;
using ZBenchmark;


BenchmarkRunner.Run<FileSystemBenchmark2>();

Console.ReadLine();