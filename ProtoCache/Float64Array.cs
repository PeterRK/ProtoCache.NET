// Copyright (c) 2025, Ruan Kunliang.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace ProtoCache {
    public class Float64Array : ArrayType {
        public override void Init(DataView data) => Init(data, 2);
        public double Get(int idx) => At(idx).GetFloat64();
    }
}
