// Copyright (c) 2025, Ruan Kunliang.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace ProtoCache {
    public class BoolArray : IUnit {

        private byte[] body = [];

        public int Size => body.Length;

        public bool Get(int idx) => body[idx] != 0;

        public void Init(DataView data) {
            if (!data.IsValid) {
                body = [];
                return;
            }
            body = Bytes.ExtractBytes(data);
        }
    }
}
