﻿using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using CloudAPI.AL.DataAccess;
using SharedLibrary;
using System;
using System.IO;
using System.Linq;

namespace ZBenchmark;

[InProcess]
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class FileSystemBenchmark2
{
    static AlbumInfoProvider _ai = new AlbumInfoProvider();
    static SystemIOAbstraction _io = new SystemIOAbstraction();
    static string source = @"D:\Production\WKRAPI";
    static string[] suitableFileFormats = new string[] { ".json", ".txt" };


    //[Benchmark]
    //public void FastGetSuitableFilePathsWithNaturalSort() {
    //    var res = _io.GetSuitableFilePathsWithNaturalSort(source, _ai.SuitableFileFormats, 1);

    //}

    //[Benchmark]
    //public void ExpGetSuitableFilePathsWithNaturalSort() {
    //    var res = _io.GetSuitableFilePathsWithNaturalSort(source, _ai.SuitableFileFormats, 1);

    //}

    [Benchmark]
    public void UseIndexOf() {
        var res = Directory.EnumerateFiles(source, "*.*", SearchOption.AllDirectories)
            .Where(file => Array.IndexOf(suitableFileFormats, Path.GetExtension(file)) > -1)
            .ToList();
    }

    [Benchmark]
    public void UseContains() {
        var res = Directory.EnumerateFiles(source, "*.*", SearchOption.AllDirectories)
            .Where(file => suitableFileFormats.Contains(Path.GetExtension(file)))
            .ToList();
    }
}