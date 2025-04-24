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
            var mark = new BitArray(size);
            for (int i = 0; i < size; i++) {
                int pos = ph.Locate(keys[i]);
                Assert.That(mark.Get(pos), Is.False);
                mark.Set(pos, true);
            }
        }

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

        private class Reader : PerfectHash.IKeySource {
            private readonly byte[][] keys;
            int current = 0;

            public Reader(byte[][] keys) {
                this.keys = keys;
            }


            public void Reset() {
                current = 0;
            }

            public int Total() {
                return keys.Length;
            }

            public ReadOnlySpan<byte> Next() {
                return keys[current++];
            }
        }
}

}
