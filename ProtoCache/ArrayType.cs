// Copyright (c) 2025, Ruan Kunliang.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace ProtoCache {
    public class ArrayType : IUnit.Object {
        protected int size = 0;
        protected int width = 4;
        protected ReadOnlyMemory<byte> body = null;

        public int Size => size;

        protected ReadOnlyMemory<byte> At(int idx) => body[(idx*width)..];

        public override void Init(ReadOnlyMemory<byte> data) {
            if (data.IsEmpty) {
                size = 0;
                width = 4;
                body = null;
                return;
            }
            uint mark = BitConverter.ToUInt32(data.Span);
            if ((mark & 3) == 0) {
                throw new ArgumentException("illegal array");
            }
            size = (int)(mark >> 2);
            width = (int)(mark & 3) * 4;
            body = data[4..];
        }
    }
}
