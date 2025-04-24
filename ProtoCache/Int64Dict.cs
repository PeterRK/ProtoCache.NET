// Copyright (c) 2025, Ruan Kunliang.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace ProtoCache {
    public class Int64Dict<T> : DictType<T>
        where T : IUnit, new() {
        public long Key(int idx) => BitConverter.ToInt64(KeyAt(idx).Span);

        public int Find(long key) {
            int idx = index.Locate(BitConverter.GetBytes(key));
            if (idx >= index.Size || key != Key(idx)) {
                return -1;
            }
            return idx;
        }
    }
}
