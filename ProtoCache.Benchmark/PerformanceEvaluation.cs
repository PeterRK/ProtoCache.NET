// Copyright (c) 2026, Ruan Kunliang.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

#nullable enable

using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Google.Protobuf;
using Pb = ProtoCache.Tests.pb;
using Pc = ProtoCache.Tests.pc;

namespace ProtoCache.Benchmark;

// Offline, dependency-free A/B harness. Setup and validation are outside timed regions.
internal static class PerformanceEvaluation {
    private sealed record Workload(string Name, Action Run, Action Verify, Func<long>? Attempts = null);
    private sealed record Sample(int Operations, double NanosecondsPerOperation,
        double BytesPerOperation, int Gen0, int Gen1, int Gen2, long BuildFailures, double? AttemptsPerOperation);
    private sealed record Result(string Name, Sample[] Samples);

    public static void Run(string[] args) {
        var options = new Dictionary<string, string>();
        for (int i = 0; i < args.Length; i += 2) {
            if (i + 1 == args.Length || !args[i].StartsWith("--"))
                throw new ArgumentException("Expected --option value pairs");
            options.Add(args[i], args[i + 1]);
        }
        string Get(string key, string fallback) => options.GetValueOrDefault(key, fallback);
        foreach (var key in options.Keys)
            if (key is not ("--output" or "--cache-directory" or "--filter" or "--samples" or
                "--warmup-ms" or "--sample-ms" or "--label"))
                throw new ArgumentException($"Unknown option {key}");
        int samples = int.Parse(Get("--samples", "3"));
        int warmupMs = int.Parse(Get("--warmup-ms", "1000"));
        int sampleMs = int.Parse(Get("--sample-ms", "500"));
        if (samples < 1 || warmupMs < 1 || sampleMs < 1) throw new ArgumentOutOfRangeException(nameof(args));
        var cache = Get("--cache-directory", Path.Combine(Path.GetTempPath(), "protocache-perf-fixtures"));
        Directory.CreateDirectory(cache);
        var filter = Get("--filter", "").Split(',', StringSplitOptions.RemoveEmptyEntries).ToHashSet();
        var workloads = CreateWorkloads(cache).Where(w => filter.Count == 0 || filter.Contains(w.Name)).ToArray();
        if (workloads.Length == 0 || (filter.Count != 0 && workloads.Length != filter.Count))
            throw new ArgumentException("Unknown workload in --filter");
        var results = new List<Result>();
        foreach (var workload in workloads) {
            long buildFailures = 0;
            void RunOperation() {
                // Build has a bounded randomized search. Count exhausted searches and
                // include their cost in one successful operation, never discard samples.
                for (int retry = 0; ; retry++) {
                    try { workload.Run(); return; }
                    catch (Exception error) when (error.Message == "fail to build perfect-hash" && retry < 100) {
                        buildFailures++;
                    }
                }
            }
            Action operation = RunOperation;
            operation();
            workload.Verify();
            // Calibrate in batches, allowing tiered compilation/PGO to settle.
            int batch = 1;
            double perOp = 0;
            var warmup = Stopwatch.StartNew();
            do {
                long start = Stopwatch.GetTimestamp();
                Execute(operation, batch);
                double ms = Stopwatch.GetElapsedTime(start).TotalMilliseconds;
                perOp = ms / batch;
                if (ms < 10 && batch < 1_048_576) batch *= 2;
            } while (warmup.ElapsedMilliseconds < warmupMs);
            int operations = (int)Math.Clamp(sampleMs / Math.Max(perOp, 0.000001), 8, 10_000_000);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            var measurements = new Sample[samples];
            for (int sample = 0; sample < samples; sample++) {
                int gen0 = GC.CollectionCount(0), gen1 = GC.CollectionCount(1), gen2 = GC.CollectionCount(2);
                long attempts = workload.Attempts?.Invoke() ?? 0;
                long failures = buildFailures;
                long bytes = GC.GetAllocatedBytesForCurrentThread();
                long start = Stopwatch.GetTimestamp();
                Execute(operation, operations);
                double ns = Stopwatch.GetElapsedTime(start).TotalNanoseconds;
                bytes = GC.GetAllocatedBytesForCurrentThread() - bytes;
                measurements[sample] = new Sample(operations, ns / operations, (double)bytes / operations,
                    GC.CollectionCount(0) - gen0, GC.CollectionCount(1) - gen1, GC.CollectionCount(2) - gen2,
                    buildFailures - failures,
                    workload.Attempts == null ? null : (double)(workload.Attempts() - attempts) / operations);
            }
            workload.Verify();
            results.Add(new Result(workload.Name, measurements));
            Console.WriteLine($"{workload.Name}: {measurements.Average(s => s.NanosecondsPerOperation):F1} ns/op, " +
                $"{measurements.Average(s => s.BytesPerOperation):F1} B/op");
        }
        var output = Path.GetFullPath(Get("--output", "performance.json"));
        Directory.CreateDirectory(Path.GetDirectoryName(output)!);
        File.WriteAllText(output, JsonSerializer.Serialize(new {
            Label = Get("--label", "unspecified"), TimestampUtc = DateTime.UtcNow,
            Runtime = RuntimeInformation.FrameworkDescription, OS = RuntimeInformation.OSDescription,
            Architecture = RuntimeInformation.ProcessArchitecture.ToString(), Environment.ProcessorCount,
            ServerGC = GCSettings.IsServerGC, GCSettings.LatencyMode,
            TieredCompilation = Environment.GetEnvironmentVariable("DOTNET_TieredCompilation") ?? "runtime default",
            TieredPGO = Environment.GetEnvironmentVariable("DOTNET_TieredPGO") ?? "runtime default",
            WarmupMs = warmupMs, SampleMs = sampleMs, Samples = samples,
            Results = results
        }, new JsonSerializerOptions { WriteIndented = true }));
    }

    // Warm and measure the same loop so OSR does not first optimize a new loop
    // inside the timed sample. Keep its call boundary identical in both paths.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Execute(Action operation, int count) {
        for (int i = 0; i < count; i++) operation();
    }

    private static byte[] Fixture(string directory, string name, Func<byte[]> create) {
        var path = Path.Combine(directory, name + ".pc");
        if (!File.Exists(path)) File.WriteAllBytes(path, create());
        return File.ReadAllBytes(path);
    }

    private static IEnumerable<Workload> CreateWorkloads(string cache) {
        var message = JsonParser.Default.Parse<Pb.Main>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "test.json")));
        var fixedRaw = Fixture(cache, "standard", () => ProtoCache.Serialize(message));
        Program.ValidateFixtures(message, fixedRaw, File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "test.fb")));
        var expected = new Program.Junk();
        expected.Traverse(message);
        byte[] output = [];
        void VerifyStandard() {
            var actual = new Program.Junk();
            actual.Traverse(new Pc.Main(output));
            expected.AssertEquivalent(actual);
        }
        yield return new("serialize.standard", () => output = ProtoCache.Serialize(message), VerifyStandard);
        var junk = new Program.Junk();
        yield return new("roundtrip.standard", () => {
            output = ProtoCache.Serialize(message);
            junk.Traverse(new Pc.Main(output));
        }, VerifyStandard);
        yield return new("traverse.standard", () => junk.Traverse(new Pc.Main(fixedRaw)), () => GC.KeepAlive(junk));
        var pbRaw = message.ToByteArray();
        yield return new("protobuf.decode-traverse", () => junk.Traverse(Pb.Main.Parser.ParseFrom(pbRaw)), () => GC.KeepAlive(junk));
        yield return new("compress.standard", () => output = Utils.Compress(fixedRaw), () => {
            if (!Utils.Decompress(output).AsSpan().SequenceEqual(fixedRaw)) throw new Exception("Compression mismatch");
        });
        var compressed = Utils.Compress(fixedRaw);
        yield return new("decompress.standard", () => output = Utils.Decompress(compressed), () => {
            if (!output.AsSpan().SequenceEqual(fixedRaw)) throw new Exception("Decompression mismatch");
        });

        var strings = new Pb.Main();
        for (int i = 0; i < 128; i++) {
            strings.Strv.Add($"字段-{i:D4}-" + new string('x', i % 64));
            strings.Datav.Add(ByteString.CopyFrom(new byte[i % 64]));
        }
        yield return new("serialize.strings", () => output = ProtoCache.Serialize(strings), () => {
            var root = new Pc.Main(output);
            if (root.Strv.Size != strings.Strv.Count || root.Datav.Size != strings.Datav.Count) throw new Exception("Size mismatch");
            for (int i = 0; i < strings.Strv.Count; i++)
                if (root.Strv.Get(i) != strings.Strv[i] || !root.Datav.Get(i).SequenceEqual(strings.Datav[i].Span))
                    throw new Exception("String/bytes mismatch");
        });
        foreach (int size in new[] { 1024, 4096, 65535, 65536, 100000 }) {
            var map = new Pb.MapCases();
            for (int i = 0; i < size; i++) map.Int32Int32.Add(i, i * 17);
            yield return new($"serialize.map-int.{size}", () => output = ProtoCache.Serialize(map), () => {
                var root = new Pc.MapCases(output).Int32Int32;
                if (root.Size != size || root.Find(-1) != -1) throw new Exception("Map size/miss mismatch");
                for (int i = 0; i < size; i++) {
                    int index = root.Find(i);
                    if (index < 0 || root.Value(index) != i * 17) throw new Exception("Map value mismatch");
                }
            });
        }
        var stringMap = new Pb.Main();
        var keys = Enumerable.Range(0, 4096).Select(i => $"公共前缀/字段/{i:D8}/" + new string('k', i % 300)).ToArray();
        for (int i = 0; i < keys.Length; i++) stringMap.Index.Add(keys[i], i);
        void VerifyStringMap(byte[] bytes) {
            var root = new Pc.Main(bytes).Index;
            if (root.Size != keys.Length || root.Find("absent") != -1) throw new Exception("Map size/miss mismatch");
            for (int i = 0; i < keys.Length; i++) {
                int index = root.Find(keys[i]);
                if (index < 0 || root.Value(index) != i) throw new Exception("String map value mismatch");
            }
        }
        yield return new("serialize.map-string.4096", () => output = ProtoCache.Serialize(stringMap), () => VerifyStringMap(output));
        var lookupRaw = Fixture(cache, "string-map-4096", () => ProtoCache.Serialize(stringMap));
        VerifyStringMap(lookupRaw);
        var lookup = new Pc.Main(lookupRaw).Index;
        int cursor = 0, sink = 0;
        var missing = keys.Select(key => key + "!").ToArray();
        yield return new("lookup.string-hit-miss", () => {
            int i = cursor++ & 4095;
            sink = lookup.Value(lookup.Find(keys[i])) + lookup.Find(missing[i]);
        }, () => GC.KeepAlive(sink));
        foreach (int size in new[] { 24, 256, 4096, 100000 }) {
            var source = new KeySource(size);
            PerfectHash? hash = null;
            yield return new($"hash.build.{size}", () => hash = PerfectHash.Build(source), () => {
                var restored = new PerfectHash(hash!.Data.ToArray());
                var seen = new bool[size];
                if (restored.Size != size) throw new Exception("Hash size mismatch");
                for (int i = 0; i < size; i++) {
                    int slot = restored.Locate(source.At(i));
                    if ((uint)slot >= size || seen[slot]) throw new Exception("Hash collision/range mismatch");
                    seen[slot] = true;
                }
            }, () => source.Resets);
        }
    }

    private sealed class KeySource : PerfectHash.IKeySource {
        private readonly byte[] bytes;
        private int current;
        public long Resets { get; private set; }
        public KeySource(int size) {
            bytes = new byte[size * 8];
            for (int i = 0; i < size; i++) BinaryPrimitives.WriteInt64LittleEndian(bytes.AsSpan(i * 8), i);
        }
        public int Total() => bytes.Length / 8;
        public void Reset() { current = 0; Resets++; }
        public ReadOnlySpan<byte> Next() => At(current++);
        public ReadOnlySpan<byte> At(int i) => bytes.AsSpan(i * 8, 8);
    }
}
