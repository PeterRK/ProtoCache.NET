// Copyright (c) 2025, Ruan Kunliang.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

using System.Text;

namespace ProtoCache {
    public class StringDict<T> : DictType<T>
        where T : IUnit, new() {
        public string Key(int idx) {
            return StringValue.Extract(IUnit.Jump(KeyAt(idx)));
        }

        public int Find(string key) {
            int idx = index.Locate(Encoding.UTF8.GetBytes(key));
            if (idx >= index.Size || !key.Equals(Key(idx))) {
                return -1;
            }
            return idx;
        }
    }
}
