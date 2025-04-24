// Copyright (c) 2025, Ruan Kunliang.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace ProtoCache {
    public struct UInt32Value : IUnit {
        private uint value;

        public readonly uint Value => value;

        public UInt32Value() {
            value = 0;
        }

        public void Init(ReadOnlyMemory<byte> data) {
            if (data.IsEmpty) {
                value = 0;
                return;
            }
            value = BitConverter.ToUInt32(data.Span);
        }

        public void InitByField(ReadOnlyMemory<byte> data) {
            Init(data);
        }
    }
}
