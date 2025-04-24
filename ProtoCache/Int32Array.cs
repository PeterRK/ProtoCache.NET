// Copyright (c) 2025, Ruan Kunliang.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace ProtoCache {
    public class Int32Array : ArrayType {
        public override void Init(DataView data) => Init(data, 1);
        public int Get(int idx) => At(idx).GetInt32();
    }
}
