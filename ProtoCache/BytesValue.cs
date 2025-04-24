// Copyright (c) 2025, Ruan Kunliang.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace ProtoCache {
    public struct BytesValue : IUnit {
        private byte[] value;

        public readonly byte[] Value {
            get { return value; }
        }

        public BytesValue() {
            value = [];
        }

        public static ReadOnlyMemory<byte> ExtractRaw(ReadOnlyMemory<byte> data) {
            uint mark = 0;
            int off = 0;
            for (int sft = 0; sft < 32; sft += 7) {
                byte b = data.Span[off++];
                mark |= ((uint)b & 0x7f) << sft;
                if ((b & 0x80) == 0) {
                    if ((mark & 3) != 0) {
                        break;
                    }
                    int size = (int)(mark >> 2);
                    return data[off..(off+size)];
                }
            }
            throw new ArgumentException("illegal bytes");
        }

        public static byte[] Extract(ReadOnlyMemory<byte> data) {
            var view = ExtractRaw(data);
            var value = new byte[view.Length];
            view.CopyTo(value);
            return value;
        }

        public void Init(ReadOnlyMemory<byte> data) {
            if (data.IsEmpty) {
                value = [];
                return;
            }
            value = Extract(data);
        }

        public void InitByField(ReadOnlyMemory<byte> data) {
            Init(IUnit.Jump(data));
        }
    }
}
