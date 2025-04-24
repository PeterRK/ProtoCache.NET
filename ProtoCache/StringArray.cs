// Copyright (c) 2025, Ruan Kunliang.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace ProtoCache {
    public class StringArray : ArrayType {
        public override void Init(DataView data) => Init(data, 0);
        public string Get(int idx) => Bytes.ExtractString(IUnit.Jump(At(idx)));
    }
}
