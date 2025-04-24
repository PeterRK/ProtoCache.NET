// Copyright (c) 2025, Ruan Kunliang.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

using System.Diagnostics;
using pb = global::ProtoCache.Tests.pb;
using pc = global::ProtoCache.Tests.pc;

namespace ProtoCache.Benchmark {
    public class Program {

        private const int loop = 1000000;

        public static void Main(string[] args) {
            var timer = new Stopwatch();

            var raw = File.ReadAllBytes("test.pb");
            var junk = new Junk();
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

            raw = File.ReadAllBytes("test.pc");
            junk = new Junk();
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
        }

        private class Junk {
            private int i32 = 0;
            private float f32 = 0;
            private long i64 = 0;
            private double f64 = 0;

            public void Print() {
                Console.Write("{0:X} {1:X}, {2}, {3}\n", i32, i64, f32, f64);
            }

            private void consume(int v) {
                i32 += v;
            }
            private void consume(uint v) {
                i32 += (int)v;
            }
            private void consume(long v) {
                i64 += v;
            }
            private void consume(ulong v) {
                i64 += (long)v;
            }
            private void consume(float v) {
                f32 += v;
            }
            private void consume(double v) {
                f64 += v;
            }
            private void consume(bool v) {
                if (v) {
                    i32 += 1;
                }
            }
            private void consume(string v) {
                i32 += v.GetHashCode();
            }
            private void consume(byte[] v) {
                i32 += v.Length;
            }

            public void Traverse(pc.Main root) {
                consume(root.I32);
                consume(root.U32);
                consume(root.I64);
                consume(root.U64);
                consume(root.Flag);
                consume(root.Mode);
                consume(root.Str);
                consume(root.Data);
                consume(root.F32);
                consume(root.F64);
                Traverse(root.Object);
                for (int i = 0; i < root.I32V.Size; i++) {
                    consume(root.I32V.Get(i));
                }
                for (int i = 0; i < root.U64V.Size; i++) {
                    consume(root.U64V.Get(i));
                }
                for (int i = 0; i < root.Strv.Size; i++) {
                    consume(root.Strv.Get(i));
                }
                for (int i = 0; i < root.Datav.Size; i++) {
                    consume(root.Datav.Get(i));
                }
                for (int i = 0; i < root.F32V.Size; i++) {
                    consume(root.F32V.Get(i));
                }
                for (int i = 0; i < root.F64V.Size; i++) {
                    consume(root.F64V.Get(i));
                }
                for (int i = 0; i < root.Flags.Size; i++) {
                    consume(root.Flags.Get(i));
                }
                for (int i = 0; i < root.Objectv.Size; i++) {
                    Traverse(root.Objectv.Get(i));
                }

                consume(root.TU32);
                consume(root.TI32);
                consume(root.TS32);
                consume(root.TU64);
                consume(root.TI64);
                consume(root.TS64);

                for (int i = 0; i < root.Index.Size; i++) {
                    consume(root.Index.Key(i));
                    consume(root.Index.Value(i).Value);
                }
                for (int i = 0; i < root.Objects.Size; i++) {
                    consume(root.Objects.Key(i));
                    Traverse(root.Objects.Value(i));
                }

                Traverse(root.Matrix);

                for (int i = 0; i < root.Vector.Size; i++) {
                    Traverse(root.Vector.Get(i));
                }
                Traverse(root.Arrays);
            }

            void Traverse(pc.Small root) {
                consume(root.I32);
                consume(root.Flag);
                consume(root.Str);
            }

            void Traverse(pc.ArrMap map) {
                for (int i = 0; i < map.Size; i++) {
                    consume(map.Key(i));
                    var value = map.Value(i);
                    for (int j = 0; j < value.Size; j++) {
                        consume(value.Get(j));
                    }
                }
            }

            void Traverse(pc.Vec2D vec) {
                for (int i = 0; i < vec.Size; i++) {
                    var line = vec.Get(i);
                    for (int j = 0; j < vec.Size; j++) {
                        consume(line.Get(j));
                    }
                }
            }

            public void Traverse(pb.Main root) {
                consume(root.I32);
                consume(root.U32);
                consume(root.I64);
                consume(root.U64);
                consume(root.Flag);
                consume((int)root.Mode);
                consume(root.Str);
                consume(root.Data.ToByteArray());
                consume(root.F32);
                consume(root.F64);
                Traverse(root.Object);
                foreach (var u in root.I32V) {
                    consume(u);
                }
                foreach (var u in root.U64V) {
                    consume(u);
                }
                foreach (var u in root.Strv) {
                    consume(u);
                }
                foreach (var u in root.Datav) {
                    consume(u.ToByteArray());
                }
                foreach (var u in root.F32V) {
                    consume(u);
                }
                foreach (var u in root.F64V) {
                    consume(u);
                }
                foreach (var u in root.Flags) {
                    consume(u);
                }
                foreach (var u in root.Objectv) {
                    Traverse(u);
                }

                consume(root.TU32);
                consume(root.TI32);
                consume(root.TS32);
                consume(root.TU64);
                consume(root.TI64);
                consume(root.TS64);

                foreach (var u in root.Index) {
                    consume(u.Key);
                    consume(u.Value);
                }
                foreach (var u in root.Objects) {
                    consume(u.Key);
                    Traverse(u.Value);
                }

                Traverse(root.Matrix);

                foreach (var u in root.Vector) {
                    Traverse(u);
                }
                Traverse(root.Arrays);
            }

            void Traverse(pb.Small root) {
                consume(root.I32);
                consume(root.Flag);
                consume(root.Str);
            }

            void Traverse(pb.ArrMap map) {
                foreach (var u in map.X) {
                    consume(u.Key);
                    foreach (var v in u.Value.X) {
                        consume(v);
                    }
                }
            }

            void Traverse(pb.Vec2D vec) {
                foreach (var u in vec.X) {
                    foreach (var v in u.X) {
                        consume(v);
                    }
                }
            }
        }
    }
}