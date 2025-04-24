// Copyright (c) 2025, Ruan Kunliang.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace ProtoCache {
    public class BytesArray : ArrayType {
        public override void Init(DataView data) => Init(data, 0);
        public byte[] Get(int idx) => Bytes.ExtractBytes(IUnit.Jump(At(idx)));
    }
}
