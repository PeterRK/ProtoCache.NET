// Copyright (c) 2025, Ruan Kunliang.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace ProtoCache {
    public struct Int32Value : IUnit {
        private int value;

        public readonly int Value => value;

        public Int32Value() {
            value = 0;
        }

        public void Init(ReadOnlyMemory<byte> data) {
            if (data.IsEmpty) {
                value = 0;
                return;
            }
            value = BitConverter.ToInt32(data.Span);
        }

        public void InitByField(ReadOnlyMemory<byte> data) {
            Init(data);
        }
    }
}
