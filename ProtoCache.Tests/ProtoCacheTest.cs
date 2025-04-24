// Copyright (c) 2025, Ruan Kunliang.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

using Google.Protobuf;
using System.Text;

namespace ProtoCache.Tests {
    public class ProtoCacheTest {

        [Test]
        public void BinaryTest() {
            var text = File.ReadAllText("test.json");
            var message = JsonParser.Default.Parse(text, pb.Main.Descriptor);
            var raw = ProtoCache.Serialize(message);
            Assert.That(raw.Length, Is.EqualTo(780));

            var root = new pc.Main(raw);

            Assert.That(root.I32, Is.EqualTo(-999));
            Assert.That(root.U32, Is.EqualTo(1234));
            Assert.That(root.I64, Is.EqualTo(-9876543210L));
            Assert.That(root.U64, Is.EqualTo(98765432123456789L));
            Assert.That(root.Flag, Is.True);
            Assert.That(root.Mode, Is.EqualTo(pc.Mode.MODE_C));
            Assert.That(root.Str, Is.EqualTo("Hello World!"));
            Assert.That(root.Data, Is.EqualTo(Encoding.ASCII.GetBytes("abc123!?$*&()'-=@~")));
            Assert.That(root.F32, Is.EqualTo(-2.1f));
            Assert.That(root.F64, Is.EqualTo(1.0));

            Assert.That(root.Object.I32, Is.EqualTo(88));
            Assert.That(root.Object.Flag, Is.False);
            Assert.That(root.Object.Str, Is.EqualTo("tmp"));

            Assert.That(root.I32V.Size, Is.EqualTo(2));
            Assert.That(root.I32V.Get(0), Is.EqualTo(1));
            Assert.That(root.I32V.Get(1), Is.EqualTo(2));

            Assert.That(root.U64V.Size, Is.EqualTo(1));
            Assert.That(root.U64V.Get(0), Is.EqualTo(12345678987654321UL));

            var expectedStrv = new string[] {
                "abc", "apple", "banana", "orange", "pear", "grape",
                "strawberry", "cherry", "mango", "watermelon"};
            Assert.That(root.Strv.Size, Is.EqualTo(expectedStrv.Length));
            for (int i = 0; i < expectedStrv.Length; i++) {
                Assert.That(root.Strv.Get(i), Is.EqualTo(expectedStrv[i]));
            }

            Assert.That(root.F32V.Size, Is.EqualTo(2));
            Assert.That(root.F32V.Get(0), Is.EqualTo(1.1f));
            Assert.That(root.F32V.Get(1), Is.EqualTo(2.2f));

            var expectedF64v = new double[] { 9.9, 8.8, 7.7, 6.6, 5.5 };
            Assert.That(root.F64V.Size, Is.EqualTo(expectedF64v.Length));
            for (int i = 0; i < expectedF64v.Length; i++) {
                Assert.That(root.F64V.Get(i), Is.EqualTo(expectedF64v[i]));
            }

            var expectedFlags = new bool[] { true, true, false, true, false, false, false };
            Assert.That(root.Flags.Size, Is.EqualTo(expectedFlags.Length));
            for (int i = 0; i < expectedFlags.Length; i++) {
                Assert.That(root.Flags.Get(i), Is.EqualTo(expectedFlags[i]));
            }

            Assert.That(root.Objectv.Size, Is.EqualTo(3));
            Assert.That(root.Objectv.Get(0).I32, Is.EqualTo(1));
            Assert.That(root.Objectv.Get(1).Flag, Is.True);
            Assert.That(root.Objectv.Get(2).Str, Is.EqualTo("good luck!"));

            Assert.That(root.HasField(pc.Main._t_i32), Is.False);

            Assert.That(root.Index.Size, Is.EqualTo(6));
            var idx = root.Index.Find("abc-1");
            Assert.That(idx, Is.GreaterThanOrEqualTo(0));
            Assert.That(root.Index.Value(idx), Is.EqualTo(1));
            idx = root.Index.Find("abc-2");
            Assert.That(idx, Is.GreaterThanOrEqualTo(0));
            Assert.That(root.Index.Value(idx), Is.EqualTo(2));
            Assert.That(root.Index.Find("abc-3"), Is.LessThan(0));
            Assert.That(root.Index.Find("abc-4"), Is.LessThan(0));

            for (int i = 0; i < root.Objects.Size; i++) {
                var key = root.Objects.Key(i);
                Assert.That(key, Is.Not.EqualTo(0));
                Assert.That(root.Objects.Value(i).I32, Is.EqualTo(key));
            }

            Assert.That(root.Matrix.Size, Is.EqualTo(3));
            var line = root.Matrix.Get(2);
            Assert.That(line.Size, Is.EqualTo(3));
            Assert.That(line.Get(2), Is.EqualTo(9));

            Assert.That(root.Vector.Size, Is.EqualTo(2));
            var map = root.Vector.Get(0);
            idx = map.Find("lv2");
            Assert.That(idx, Is.GreaterThanOrEqualTo(0));
            var val = map.Value(idx);
            Assert.That(val.Size, Is.EqualTo(2));
            Assert.That(val.Get(0), Is.EqualTo(21));
            Assert.That(val.Get(1), Is.EqualTo(22));

            Assert.That(root.Arrays.Size, Is.EqualTo(2));
            idx = root.Arrays.Find("lv5");
            Assert.That(idx, Is.GreaterThanOrEqualTo(0));
            val = root.Arrays.Value(idx);
            Assert.That(val.Get(0), Is.EqualTo(51));
            Assert.That(val.Get(1), Is.EqualTo(52));
        }

        [Test]
        public void AliasTest() {
            var text = File.ReadAllText("test-alias.json");
            var message = JsonParser.Default.Parse(text, pb.Main.Descriptor);
            var raw = ProtoCache.Serialize(message);
            Assert.That(raw.Length, Is.EqualTo(68));

            Assert.That(BitConverter.ToUInt32(raw, 20), Is.EqualTo(0xd));
            Assert.That(BitConverter.ToUInt32(raw, 24), Is.EqualTo(1));
            Assert.That(BitConverter.ToUInt32(raw, 28), Is.EqualTo(1));
        }
    }
}
