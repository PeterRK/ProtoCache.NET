// Copyright (c) 2025, Ruan Kunliang.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace ProtoCache.Tests {
    public class EdgeCaseTest {
        private static int allocationSink;


        [Test]
        public void EmptyMessageTest() {
            var raw = ProtoCache.Serialize(new pb.Main());
            Assert.That(raw, Has.Length.EqualTo(4));

            var root = new pc.Main(raw);
            Assert.Multiple(() => {
                Assert.That(root.I32, Is.Zero);
                Assert.That(root.Str, Is.Empty);
                Assert.That(root.I32V.Size, Is.Zero);
                Assert.That(root.Index.Size, Is.Zero);
                Assert.That(root.HasField(pc.Main._i32), Is.False);
            });
        }

        [Test]
        public void CyclicSchemaTest() {
            var message = new pb.CyclicA {
                Value = 1,
                Cyclic = new pb.CyclicB { Value = 2 }
            };

            var root = new pc.CyclicA(ProtoCache.Serialize(message));
            Assert.Multiple(() => {
                Assert.That(root.Value, Is.EqualTo(1));
                Assert.That(root.Cyclic.Value, Is.EqualTo(2));
                Assert.That(root.Cyclic.Cyclic.Value, Is.Zero);
            });
        }

        [Test]
        public void CompressionRoundTripPatternsTest() {
            byte[][] samples = [
                [],
                [0],
                [0xff],
                [1, 2, 3, 4, 5, 6, 7],
                [0, 0, 0, 0, 0xff, 0xff, 0xff, 0xff],
                Enumerable.Range(0, 1024).Select(i => (byte)i).ToArray(),
                Enumerable.Repeat((byte)0, 1024).ToArray(),
                Enumerable.Repeat((byte)0xff, 1024).ToArray()
            ];

            foreach (var sample in samples) {
                Assert.That(Utils.Decompress(Utils.Compress(sample)),
                    Is.EqualTo(sample));
            }
        }

        [TestCaseSource(nameof(BrokenCompressedData))]
        public void BrokenCompressedDataTest(byte[] data) {
            Assert.That(() => Utils.Decompress(data),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void PerfectHashRejectsTruncatedDataTest() {
            Assert.Multiple(() => {
                Assert.That(() => new PerfectHash([]),
                    Throws.TypeOf<ArgumentException>());
                Assert.That(() => new PerfectHash(BitConverter.GetBytes(25)),
                    Throws.TypeOf<ArgumentException>());
            });
        }

        [Test]
        public void PopCountWidthSumMatchesPortableTest() {
            for (uint i = 0; i <= ushort.MaxValue; i++) {
                uint value32 = i | (i << 16);
                ulong value64 = value32 | ((ulong)~value32 << 32);
                if (Message.Count32(value32) != PortableWidthSum(value32, 16)
                    || Message.Count64(value64) != PortableWidthSum(value64, 32)) {
                    Assert.Fail("popcount width sum mismatch for {0}", i);
                }
            }
        }

        private static int PortableWidthSum(ulong value, int count) {
            int sum = 0;
            for (int i = 0; i < count; i++, value >>= 2) {
                sum += (int)value & 3;
            }
            return sum;
        }

        [Test]
        public void AllMapBranchesTest() {
            var message = new pb.MapCases();
            var longKey = new string('x', 300);
            message.StringBool.Add("bool", true);
            message.StringBool.Add("other", false);
            message.StringBool.Add("键", false);
            message.StringBool.Add(longKey, true);
            message.Int32Int32.Add(-1, -11);
            message.Int32Int32.Add(int.MinValue, 1);
            message.Uint32Uint32.Add(2, 22);
            message.Uint32Uint32.Add(uint.MaxValue, 1);
            message.Int64Int64.Add(-3, -33);
            message.Int64Int64.Add(long.MinValue, 1);
            message.Uint64Uint64.Add(4, 44);
            message.Uint64Uint64.Add(ulong.MaxValue, 1);
            message.StringFloat.Add("float", 1.25f);
            message.StringDouble.Add("double", 2.5);
            message.StringBytes.Add("bytes",
                Google.Protobuf.ByteString.CopyFromUtf8("data"));
            message.StringString.Add("string", "value");
            message.StringMessage.Add("message", new pb.Small { I32 = 55 });
            message.StringEnum.Add("enum", pb.Mode.C);

            var root = new pc.MapCases(ProtoCache.Serialize(message));

            var stringBool = root.StringBool;
            var int32Int32 = root.Int32Int32;
            var uint32Uint32 = root.Uint32Uint32;
            var int64Int64 = root.Int64Int64;
            var uint64Uint64 = root.Uint64Uint64;
            var stringFloat = root.StringFloat;
            var stringDouble = root.StringDouble;
            var stringBytes = root.StringBytes;
            var stringString = root.StringString;
            var stringMessage = root.StringMessage;
            var stringEnum = root.StringEnum;

            Assert.Multiple(() => {
                Assert.That(stringBool.Value(stringBool.Find("bool")), Is.True);
                Assert.That(stringBool.Value(stringBool.Find(longKey)), Is.True);
                Assert.That(stringBool.Value(stringBool.Find("键")), Is.False);
                Assert.That(int32Int32.Value(int32Int32.Find(-1)), Is.EqualTo(-11));
                Assert.That(int32Int32.Value(int32Int32.Find(int.MinValue)), Is.EqualTo(1));
                Assert.That(uint32Uint32.Value(uint32Uint32.Find(2)), Is.EqualTo(22));
                Assert.That(uint32Uint32.Value(uint32Uint32.Find(uint.MaxValue)), Is.EqualTo(1));
                Assert.That(int64Int64.Value(int64Int64.Find(-3)), Is.EqualTo(-33));
                Assert.That(int64Int64.Value(int64Int64.Find(long.MinValue)), Is.EqualTo(1));
                Assert.That(uint64Uint64.Value(uint64Uint64.Find(4)), Is.EqualTo(44));
                Assert.That(uint64Uint64.Value(uint64Uint64.Find(ulong.MaxValue)), Is.EqualTo(1));
                Assert.That(stringFloat.Value(stringFloat.Find("float")), Is.EqualTo(1.25f));
                Assert.That(stringDouble.Value(stringDouble.Find("double")), Is.EqualTo(2.5));
                Assert.That(stringBytes.Value(stringBytes.Find("bytes")).SequenceEqual("data"u8),
                    Is.True);
                Assert.That(stringString.Value(stringString.Find("string")),
                    Is.EqualTo("value"));
                Assert.That(stringMessage.Value(stringMessage.Find("message")).I32,
                    Is.EqualTo(55));
                Assert.That(stringEnum.Value(stringEnum.Find("enum")),
                    Is.EqualTo((int)pb.Mode.C));
            });
        }

        [Test]
        public void MapFindShortKeysDoNotAllocateTest() {
            var message = new pb.MapCases();
            message.StringBool.Add("one", true);
            message.StringBool.Add("two", false);
            message.StringBool.Add("three", true);
            message.Int32Int32.Add(1, 1);
            message.Int32Int32.Add(2, 2);
            message.Int32Int32.Add(3, 3);
            message.Uint32Uint32.Add(1, 1);
            message.Uint32Uint32.Add(2, 2);
            message.Uint32Uint32.Add(3, 3);
            message.Int64Int64.Add(1, 1);
            message.Int64Int64.Add(2, 2);
            message.Int64Int64.Add(3, 3);
            message.Uint64Uint64.Add(1, 1);
            message.Uint64Uint64.Add(2, 2);
            message.Uint64Uint64.Add(3, 3);

            var root = new pc.MapCases(ProtoCache.Serialize(message));
            var strings = root.StringBool;
            var int32s = root.Int32Int32;
            var uint32s = root.Uint32Uint32;
            var int64s = root.Int64Int64;
            var uint64s = root.Uint64Uint64;

            Assert.Multiple(() => {
                Assert.That(strings.Find("two"), Is.GreaterThanOrEqualTo(0));
                Assert.That(int32s.Find(2), Is.GreaterThanOrEqualTo(0));
                Assert.That(uint32s.Find(2), Is.GreaterThanOrEqualTo(0));
                Assert.That(int64s.Find(2), Is.GreaterThanOrEqualTo(0));
                Assert.That(uint64s.Find(2), Is.GreaterThanOrEqualTo(0));
            });

            int total = 0;
            for (int i = 0; i < 1000; i++) {
                total += strings.Find("two");
                total += int32s.Find(2);
                total += uint32s.Find(2);
                total += int64s.Find(2);
                total += uint64s.Find(2);
            }

            long before = GC.GetAllocatedBytesForCurrentThread();
            total = 0;
            for (int i = 0; i < 1000; i++) {
                total += strings.Find("two");
                total += int32s.Find(2);
                total += uint32s.Find(2);
                total += int64s.Find(2);
                total += uint64s.Find(2);
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            allocationSink = total;
            Assert.That(allocated, Is.Zero);
        }

        [Test]
        public void ZeroCopySpansDoNotAllocateTest() {
            var message = new pb.Main {
                Data = Google.Protobuf.ByteString.CopyFromUtf8("data")
            };
            message.Datav.Add(Google.Protobuf.ByteString.CopyFromUtf8("bytes"));
            message.Flags.Add(true);

            var root = new pc.Main(ProtoCache.Serialize(message));
            Assert.Multiple(() => {
                Assert.That(root.Data.SequenceEqual("data"u8), Is.True);
                Assert.That(root.Datav.Get(0).SequenceEqual("bytes"u8), Is.True);
                Assert.That(root.Flags.Get(0), Is.True);
            });

            int total = 0;
            for (int i = 0; i < 1000; i++) {
                total += root.Data.Length;
                total += root.Datav.Get(0).Length;
                total += root.Flags.Get(0) ? 1 : 0;
            }

            long before = GC.GetAllocatedBytesForCurrentThread();
            total = 0;
            for (int i = 0; i < 1000; i++) {
                total += root.Data.Length;
                total += root.Datav.Get(0).Length;
                total += root.Flags.Get(0) ? 1 : 0;
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            allocationSink = total;
            Assert.That(allocated, Is.Zero);
        }


        private static IEnumerable<byte[]> BrokenCompressedData() {
            yield return [0x80];
            yield return [0x80, 0x80, 0x80, 0x80, 0x80];
            yield return [0x00, 0x01];
            yield return [0x00, 0x01, 0x2a];
            yield return [0x0a];
            yield return [0xff, 0xff, 0xff, 0xff, 0x07];
            yield return [0x80, 0x80, 0x80, 0x80, 0x10];
        }
    }
}
