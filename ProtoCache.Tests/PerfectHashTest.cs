// Copyright (c) 2025, Ruan Kunliang.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

using System.Collections;
using System.Text;

namespace ProtoCache.Tests {
    public class PerfectHashTest {

        private static void DoTest(int size) {
            byte[][] keys = new byte[size][];
            for (int i = 0; i < size; i++) {
                keys[i] = Encoding.ASCII.GetBytes(Convert.ToString(i));
            }
            var ph = PerfectHash.Build(new Reader(keys));
            ph = new PerfectHash(ph.Data.ToArray());
            Assert.That(ph.Size, Is.EqualTo(size));
            var mark = new BitArray(size);
            for (int i = 0; i < size; i++) {
                int pos = ph.Locate(keys[i]);
                Assert.That(pos, Is.InRange(0, size - 1));
                Assert.That(mark.Get(pos), Is.False);
                mark.Set(pos, true);
            }
        }

        [TestCase(25)]
        [TestCase(255)]
        [TestCase(256)]
        [TestCase(2047)]
        [TestCase(2048)]
        [TestCase(2049)]
        [TestCase(65535)]
        [TestCase(65536)]
        public void SerializedIndexWidthBoundaryTest(int size) => DoTest(size);

        [Test]
        public void TinyTest() {
            DoTest(0);
            DoTest(1);
            DoTest(2);
            DoTest(24);
        }

        [Test]
        public void SmallTest() {
            DoTest(200);
            DoTest(1000);
        }

        [Test]
        public void BitTest() {
            DoTest(100000);
        }

        [TestCase(24)]
        [TestCase(256)]
        [TestCase(65536)]
        public void ExhaustedBudgetStopsAfterFortyAttempts(int size) {
            // Duplicate keys cannot form a perfect hash, so every attempt must fail.
            // Cover each index width without a probabilistic stress assertion.
            var reader = new Reader(Enumerable.Repeat(new byte[] { 1, 2, 3 }, size).ToArray());
            Assert.That(() => PerfectHash.Build(reader),
                Throws.Exception.With.Message.EqualTo("fail to build perfect-hash"));
            Assert.That(reader.Resets, Is.EqualTo(40));
        }

        private class Reader(byte[][] keys) : PerfectHash.IKeySource {
            private readonly byte[][] keys = keys;
            int current = 0;

            public int Resets { get; private set; }
            public void Reset() { current = 0; Resets++; }
            public int Total() => keys.Length;
            public ReadOnlySpan<byte> Next() => keys[current++];
        }
}

}
