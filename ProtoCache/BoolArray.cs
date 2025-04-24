// Copyright (c) 2025, Ruan Kunliang.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace ProtoCache {
    public class BoolArray : IUnit.Object {
        private ReadOnlyMemory<byte> body = ReadOnlyMemory<byte>.Empty;

        public int Size => body.Length;

        public bool Get(int idx) => body.Span[idx] != 0;

        public override void Init(ReadOnlyMemory<byte> data) {
            if (data.IsEmpty) {
                body = ReadOnlyMemory<byte>.Empty;
                return;
            }
            body = BytesValue.Extract(data);
        }
    }
}
