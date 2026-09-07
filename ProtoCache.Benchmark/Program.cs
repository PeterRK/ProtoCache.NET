// Copyright (c) 2025, Ruan Kunliang.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

using System.Diagnostics;
using Google.Protobuf;
using pb = global::ProtoCache.Tests.pb;
using pc = global::ProtoCache.Tests.pc;
using fb = global::ProtoCache.Tests.fb;

namespace ProtoCache.Benchmark {
    public class Program {
        private const int loop = 1000000;

        public static void Main(string[] args) {
            if (args.Length > 0 && args[0] == "--evaluate") {
                PerformanceEvaluation.Run(args[1..]);
                return;
            }
            var timer = new Stopwatch();

            var fixture = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "test.json"));
            var message = JsonParser.Default.Parse<pb.Main>(fixture);
            var pcRaw = ProtoCache.Serialize(message);
            var pbRaw = message.ToByteArray();
            var fbPath = Path.Combine(AppContext.BaseDirectory, "test.fb");
            if (!File.Exists(fbPath)) {
                throw new FileNotFoundException(
                    "Run ProtoCache.Benchmark/generate-fixtures.sh before the benchmark.", fbPath);
            }
            var fbRaw = File.ReadAllBytes(fbPath);
            ValidateFixtures(message, pcRaw, fbRaw);

            var raw = pcRaw;
            for (int i = 0; i < loop; i++) {
                Utils.Decompress(Utils.Compress(raw));
            }
            timer.Restart();
            for (int i = 0; i < loop; i++) {
                Utils.Compress(raw);
            }
            timer.Stop();
            Console.Write("compress: {0} ns\n", timer.Elapsed.TotalNanoseconds / loop);

            var compressed = Utils.Compress(raw);
            timer.Restart();
            for (int i = 0; i < loop; i++) {
                Utils.Decompress(compressed);
            }
            timer.Stop();
            Console.Write("decompress: {0} ns\n", timer.Elapsed.TotalNanoseconds / loop);

            var junk = new Junk();
            for (int i = 0; i < loop; i++) {
                junk.Traverse(new pc.Main(raw));
            }
            timer.Restart();
            for (int i = 0; i < loop; i++) {
                junk.Traverse(new pc.Main(raw));
            }
            timer.Stop();
            junk.Print();
            Console.Write("protocache: {0} ns/op\n", timer.Elapsed.TotalNanoseconds / loop);

            raw = pbRaw;
            junk = new Junk();
            for (int i = 0; i < loop; i++) {
                junk.Traverse(pb.Main.Parser.ParseFrom(raw));
            }
            timer.Restart();
            for (int i = 0; i < loop; i++) {
                junk.Traverse(pb.Main.Parser.ParseFrom(raw));
            }
            timer.Stop();
            junk.Print();
            Console.Write("protobuf: {0} ns/op\n", timer.Elapsed.TotalNanoseconds / loop);

            raw = fbRaw;
            junk = new Junk();
            for (int i = 0; i < loop; i++) {
                var root = fb.Main.GetRootAsMain(new Google.FlatBuffers.ByteBuffer(raw));
                junk.Traverse(root);
            }
            timer.Restart();
            for (int i = 0; i < loop; i++) {
                var root = fb.Main.GetRootAsMain(new Google.FlatBuffers.ByteBuffer(raw));
                junk.Traverse(root);
            }
            timer.Stop();
            junk.Print();
            Console.Write("flatbuffers: {0} ns/op\n", timer.Elapsed.TotalNanoseconds / loop);
        }

        internal static void ValidateFixtures(pb.Main message, byte[] pcRaw, byte[] fbRaw) {
            var expected = new Junk();
            expected.Traverse(message);
            var actual = new Junk();
            actual.Traverse(new pc.Main(pcRaw));
            expected.AssertEquivalent(actual);
            actual = new Junk();
            actual.Traverse(fb.Main.GetRootAsMain(new Google.FlatBuffers.ByteBuffer(fbRaw)));
            expected.AssertEquivalent(actual);

            // Unequal row lengths catch using the outer dimension in the inner loop.
            float[][] rows = [[], [1.25f], [2.5f, -3.75f, 4.5f, 8f]];
            var matrix = new pb.Vec2D();
            var builder = new Google.FlatBuffers.FlatBufferBuilder(128);
            var offsets = new Google.FlatBuffers.Offset<fb.Vec1D>[rows.Length];
            for (int i = 0; i < rows.Length; i++) {
                var row = new pb.Vec2D.Types.Vec1D();
                row.X.Add(rows[i]);
                matrix.X.Add(row);
                offsets[i] = fb.Vec1D.CreateVec1D(builder, fb.Vec1D.Create_Vector(builder, rows[i]));
            }
            var vector = fb.Vec2D.Create_Vector(builder, offsets);
            builder.Finish(fb.Vec2D.CreateVec2D(builder, vector).Value);
            expected = new Junk();
            expected.Traverse(matrix);
            actual = new Junk();
            var pcMatrix = new pc.Vec2D();
            pcMatrix.Init(new DataView(ProtoCache.Serialize(matrix)));
            actual.Traverse(pcMatrix);
            expected.AssertEquivalent(actual);
            actual = new Junk();
            actual.Traverse(fb.Vec2D.GetRootAsVec2D(builder.DataBuffer));
            expected.AssertEquivalent(actual);
        }

        internal sealed class Junk {
            private int i32 = 0;
            private float f32 = 0;
            private long i64 = 0;
            private double f64 = 0;

            public void AssertEquivalent(Junk other) {
                if (i32 != other.i32 || i64 != other.i64 ||
                    Math.Abs(f32 - other.f32) > 1e-5 * Math.Max(1, Math.Abs(f32)) ||
                    Math.Abs(f64 - other.f64) > 1e-10 * Math.Max(1, Math.Abs(f64))) {
                    throw new InvalidOperationException("Traversal checksums differ");
                }
            }

            public void Print() => Console.Write("{0:X} {1:X}, {2}, {3}\n", i32, i64, f32, f64);

            private void Consume(int v) => i32 += v;
            private void Consume(uint v) => i32 += (int)v;
            private void Consume(long v) => i64 += v;
            private void Consume(ulong v) => i64 += (long)v;
            private void Consume(float v) => f32 += v;
            private void Consume(double v) => f64 += v;
            private void Consume(bool v) {
                if (v) {
                    i32 += 1;
                }
            }
            private void Consume(string v) {
                if (v == null || v.Length == 0) {
                    return;
                }
                i32 += v.GetHashCode();
            }
            private void Consume(ReadOnlySpan<byte> v) {
                i32 += v.Length;
            }

            private readonly pc.Small tmpSmall = new();
            private readonly pc.ArrMap tmpArrMap = new();
            private readonly pc.ArrMap.Array tmpArray = new();
            private readonly pc.Vec2D.Vec1D tmpVec1D = new();

            public void Traverse(pc.Main root) {
                TraverseA(root);
                TraverseB(root);
                TraverseC(root);
            }


            public void TraverseA(pc.Main root) {
                Consume(root.I32);
                Consume(root.U32);
                Consume(root.I64);
                Consume(root.U64);
                Consume(root.Flag);
                Consume(root.Mode);
                Consume(root.Str);
                Consume(root.Data);
                Consume(root.F32);
                Consume(root.F64);
                Traverse(root.Object);
                Consume(root.TU32);
                Consume(root.TI32);
                Consume(root.TS32);
                Consume(root.TU64);
                Consume(root.TI64);
                Consume(root.TS64);
            }

            public void TraverseB(pc.Main root) {
                for (int i = 0; i < root.I32V.Size; i++) {
                    Consume(root.I32V.Get(i));
                }
                for (int i = 0; i < root.U64V.Size; i++) {
                    Consume(root.U64V.Get(i));
                }
                for (int i = 0; i < root.Strv.Size; i++) {
                    Consume(root.Strv.Get(i));
                }
                for (int i = 0; i < root.Datav.Size; i++) {
                    Consume(root.Datav.Get(i));
                }
                for (int i = 0; i < root.F32V.Size; i++) {
                    Consume(root.F32V.Get(i));
                }
                for (int i = 0; i < root.F64V.Size; i++) {
                    Consume(root.F64V.Get(i));
                }
                for (int i = 0; i < root.Flags.Size; i++) {
                    Consume(root.Flags.Get(i));
                }
                for (int i = 0; i < root.Objectv.Size; i++) {
                    Traverse(root.Objectv.Get(i, tmpSmall));
                }
            }

            public void TraverseC(pc.Main root) {
                for (int i = 0; i < root.Index.Size; i++) {
                    Consume(root.Index.Key(i));
                    Consume(root.Index.Value(i));
                }
                for (int i = 0; i < root.Objects.Size; i++) {
                    Consume(root.Objects.Key(i));
                    Traverse(root.Objects.Value(i, tmpSmall));
                }

                Traverse(root.Matrix);

                for (int i = 0; i < root.Vector.Size; i++) {
                    Traverse(root.Vector.Get(i, tmpArrMap));
                }
                Traverse(root.Arrays);
            }

            void Traverse(pc.Small root) {
                Consume(root.I32);
                Consume(root.Flag);
                Consume(root.Str);
            }

            void Traverse(pc.ArrMap map) {
                for (int i = 0; i < map.Size; i++) {
                    Consume(map.Key(i));
                    var value = map.Value(i, tmpArray);
                    for (int j = 0; j < value.Size; j++) {
                        Consume(value.Get(j));
                    }
                }
            }

            internal void Traverse(pc.Vec2D vec) {
                for (int i = 0; i < vec.Size; i++) {
                    var line = vec.Get(i, tmpVec1D);
                    for (int j = 0; j < line.Size; j++) {
                        Consume(line.Get(j));
                    }
                }
            }

            public void Traverse(pb.Main root) {
                TraverseA(root);
                TraverseB(root);
                TraverseC(root);
            }

            public void TraverseA(pb.Main root) {
                Consume(root.I32);
                Consume(root.U32);
                Consume(root.I64);
                Consume(root.U64);
                Consume(root.Flag);
                Consume((int)root.Mode);
                Consume(root.Str);
                Consume(root.Data.Span);
                Consume(root.F32);
                Consume(root.F64);
                Traverse(root.Object);
                Consume(root.TU32);
                Consume(root.TI32);
                Consume(root.TS32);
                Consume(root.TU64);
                Consume(root.TI64);
                Consume(root.TS64);
            }

            public void TraverseB(pb.Main root) {
                foreach (var u in root.I32V) {
                    Consume(u);
                }
                foreach (var u in root.U64V) {
                    Consume(u);
                }
                foreach (var u in root.Strv) {
                    Consume(u);
                }
                foreach (var u in root.Datav) {
                    Consume(u.Span);
                }
                foreach (var u in root.F32V) {
                    Consume(u);
                }
                foreach (var u in root.F64V) {
                    Consume(u);
                }
                foreach (var u in root.Flags) {
                    Consume(u);
                }
                foreach (var u in root.Objectv) {
                    Traverse(u);
                }
            }

            public void TraverseC(pb.Main root) {
                foreach (var u in root.Index) {
                    Consume(u.Key);
                    Consume(u.Value);
                }
                foreach (var u in root.Objects) {
                    Consume(u.Key);
                    Traverse(u.Value);
                }

                Traverse(root.Matrix);

                foreach (var u in root.Vector) {
                    Traverse(u);
                }
                Traverse(root.Arrays);
            }

            void Traverse(pb.Small root) {
                Consume(root.I32);
                Consume(root.Flag);
                Consume(root.Str);
            }

            void Traverse(pb.ArrMap map) {
                foreach (var u in map.X) {
                    Consume(u.Key);
                    foreach (var v in u.Value.X) {
                        Consume(v);
                    }
                }
            }

            internal void Traverse(pb.Vec2D vec) {
                foreach (var u in vec.X) {
                    foreach (var v in u.X) {
                        Consume(v);
                    }
                }
            }

            public void Traverse(fb.Main root) {
                TraverseA(root);
                TraverseB(root);
                TraverseC(root);
            }

            public void TraverseA(fb.Main root) {
                Consume(root.I32);
                Consume(root.U32);
                Consume(root.I64);
                Consume(root.U64);
                Consume(root.Flag);
                Consume((int)root.Mode);
                Consume(root.Str);
                Consume(root.DataLength);
                Consume(root.F32);
                Consume(root.F64);
                Traverse(root.Object.Value);
                Consume(root.TU32);
                Consume(root.TI32);
                Consume(root.TS32);
                Consume(root.TU64);
                Consume(root.TI64);
                Consume(root.TS64);
            }

            public void TraverseB(fb.Main root) {
                for (int i = 0; i < root.I32vLength; i++) {
                    Consume(root.I32v(i));
                }
                for (int i = 0; i < root.U64vLength; i++) {
                    Consume(root.U64v(i));
                }
                for (int i = 0; i < root.StrvLength; i++) {
                    Consume(root.Strv(i));
                }
                for (int i = 0; i < root.DatavLength; i++) {
                    Consume(root.Datav(i).Value._Length);
                }
                for (int i = 0; i < root.F32vLength; i++) {
                    Consume(root.F32v(i));
                }
                for (int i = 0; i < root.F64vLength; i++) {
                    Consume(root.F64v(i));
                }
                for (int i = 0; i < root.FlagsLength; i++) {
                    Consume(root.Flags(i));
                }

                for (int i = 0; i < root.ObjectvLength; i++) {
                    Traverse(root.Objectv(i).Value);
                }
            }

            public void TraverseC(fb.Main root) {
                for (int i = 0; i < root.IndexLength; i++) {
                    var entry = root.Index(i).Value;
                    Consume(entry.Key);
                    Consume(entry.Value);
                }
                for (int i = 0; i < root.ObjectsLength; i++) {
                    var entry = root.Objects(i).Value;
                    Consume(entry.Key);
                    Traverse(entry.Value.Value);
                }

                Traverse(root.Matrix.Value);

                for (int i = 0; i < root.VectorLength; i++) {
                    Traverse(root.Vector(i).Value);
                }
                Traverse(root.Arrays.Value);
            }


            void Traverse(fb.Small root) {
                Consume(root.I32);
                Consume(root.Flag);
                Consume(root.Str);
            }

            void Traverse(fb.ArrMap map) {
                for (int i = 0; i < map._Length; i++) {
                    var entry = map._(i).Value;
                    Consume(entry.Key);
                    var value = entry.Value.Value;
                    for (int j = 0; j < value._Length; j++) {
                        Consume(value._(j));
                    }
                }
            }

            internal void Traverse(fb.Vec2D vec) {
                for (int i = 0; i < vec._Length; i++) {
                    var line = vec._(i).Value;
                    for (int j = 0; j < line._Length; j++) {
                        Consume(line._(j));
                    }
                }
            }
        }
    }
}