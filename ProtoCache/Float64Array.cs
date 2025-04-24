// Copyright (c) 2025, Ruan Kunliang.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace ProtoCache {
    public class Float64Array : IUnit.Object {
        private ReadOnlyMemory<byte> data = ReadOnlyMemory<byte>.Empty;
        private const int width = 8;

        public int Size => data.Length / width;

        public double Get(int idx) => BitConverter.ToDouble(data.Span[(idx * width)..]);

        public override void Init(ReadOnlyMemory<byte> data) {
            if (data.IsEmpty) {
                this.data = ReadOnlyMemory<byte>.Empty;
                return;
            }
            uint mark = BitConverter.ToUInt32(data.Span);
            if ((mark & 3) != width / 4) {
                throw new ArgumentException("illegal int32 array");
            }
            var size = (int)(mark >> 2);
            this.data = data[4..(4 + size * width)];
        }
    }
}
