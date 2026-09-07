// Copyright (c) 2026, Ruan Kunliang.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace ProtoCache.Tests {
    public class ShortAliasTest {
        private static IEnumerable<TestCaseData> BoolPatterns() {
            for (int size = 0; size <= 4; size++) {
                for (int mask = 0; mask < (1 << size); mask++) {
                    var values = Enumerable.Range(0, size)
                        .Select(i => (mask & (1 << i)) != 0).ToArray();
                    yield return new TestCaseData(new object[] { values })
                        .SetName($"BoolAlias_{size}_{mask}");
                }
            }
        }

        private static void AssertValues(BoolArray actual, bool[] expected) {
            Assert.That(actual.Size, Is.EqualTo(expected.Length));
            for (int i = 0; i < expected.Length; i++) {
                Assert.That(actual.Get(i), Is.EqualTo(expected[i]), $"element {i}");
            }
        }

        [TestCaseSource(nameof(BoolPatterns))]
        public void BoolAliasRoundTrip(bool[] values) {
            var alias = new pb.BoolAlias();
            alias.X.Add(values);
            var raw = ProtoCache.Serialize(alias);
            var standalone = new pc.BoolAlias();
            standalone.Init(new DataView(raw));
            AssertValues(standalone, values);
            Assert.That(raw.Length, Is.EqualTo((1 + values.Length + 3) & ~3));
            Assert.That(raw[0], Is.EqualTo(values.Length << 2));

            var source = new pb.AliasHolder { Flags = alias };
            source.Rows.Add(new pb.BoolAlias());
            source.Rows.Add(alias);
            source.Entries.Add(0, new pb.BoolAlias());
            source.Entries.Add(1, alias);
            var root = new pc.AliasHolder(ProtoCache.Serialize(source));
            AssertValues(root.Flags, values);
            Assert.That(root.HasField(pc.AliasHolder._flags),
                Is.EqualTo(values.Length != 0));
            Assert.That(root.Rows.Size, Is.EqualTo(2));
            AssertValues(root.Rows.Get(0), []);
            AssertValues(root.Rows.Get(1), values);
            var entries = root.Entries;
            Assert.That(entries.Size, Is.EqualTo(2));
            AssertValues(entries.Value(entries.Find(0)), []);
            AssertValues(entries.Value(entries.Find(1)), values);

            // Ordinary repeated bool must keep the same element semantics.
            var ordinary = new pb.Main();
            ordinary.Flags.Add(values);
            AssertValues(new pc.Main(ProtoCache.Serialize(ordinary)).Flags, values);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void NumericAliasesRoundTrip(bool populated) {
            var longs = new pb.LongAlias();
            var map = new pb.LongMapAlias();
            if (populated) {
                longs.X.Add(42);
                map.X.Add(7, 9);
            }
            var rawLongs = ProtoCache.Serialize(longs);
            var rawMap = ProtoCache.Serialize(map);
            var standaloneLongs = new pc.LongAlias();
            standaloneLongs.Init(new DataView(rawLongs));
            var standaloneMap = new pc.LongMapAlias();
            standaloneMap.Init(new DataView(rawMap));
            Assert.That(standaloneLongs.Size, Is.EqualTo(populated ? 1 : 0));
            Assert.That(standaloneMap.Size, Is.EqualTo(populated ? 1 : 0));

            var source = new pb.AliasHolder { Longs = longs, LongMap = map };
            source.LongRows.Add(longs);
            source.MapRows.Add(map);
            var root = new pc.AliasHolder(ProtoCache.Serialize(source));
            Assert.Multiple(() => {
                Assert.That(root.Longs.Size, Is.EqualTo(populated ? 1 : 0));
                Assert.That(root.LongMap.Size, Is.EqualTo(populated ? 1 : 0));
                Assert.That(root.LongRows.Get(0).Size, Is.EqualTo(populated ? 1 : 0));
                Assert.That(root.MapRows.Get(0).Size, Is.EqualTo(populated ? 1 : 0));
            });
            if (populated) {
                Assert.That(root.Longs.Get(0), Is.EqualTo(42));
                Assert.That(root.LongMap.Value(root.LongMap.Find(7)), Is.EqualTo(9));
                Assert.That(root.LongRows.Get(0).Get(0), Is.EqualTo(42));
                var nestedMap = root.MapRows.Get(0);
                Assert.That(nestedMap.Value(nestedMap.Find(7)), Is.EqualTo(9));
            }
        }

        [Test]
        public void LegacyEmptyAliasHeadersAreReadable() {
            // Empty headers emitted by the existing C++ and .NET writers.
            var array = new Int64Array();
            array.Init(new DataView(new byte[] { 1, 0, 0, 0 }));
            var map = new Int64Dict.Int64Value();
            map.Init(new DataView(new byte[] { 0, 0, 0, 0x50 }));
            Assert.Multiple(() => {
                Assert.That(array.Size, Is.Zero);
                Assert.That(map.Size, Is.Zero);
                Assert.That(map.Find(7), Is.EqualTo(-1));
            });
        }

        [Test]
        public void NonEmptyContainerWidthsAreStillChecked() {
            // A nonempty 32-bit array/map cannot be read as 64-bit values.
            Assert.That(() => new Int64Array().Init(new DataView(
                new byte[] { 5, 0, 0, 0, 42, 0, 0, 0 })),
                Throws.TypeOf<ArgumentException>());
            Assert.That(() => new Int64Dict.Int64Value().Init(new DataView(
                new byte[] { 1, 0, 0, 0x50, 7, 0, 0, 0, 9, 0, 0, 0 })),
                Throws.TypeOf<ArgumentException>());
            Assert.That(() => new Int64Array().Init(new DataView(new byte[4])),
                Throws.TypeOf<ArgumentException>());
            Assert.That(() => new Int64Dict.Int64Value().Init(new DataView(new byte[4])),
                Throws.TypeOf<ArgumentException>());
        }
    }
}
