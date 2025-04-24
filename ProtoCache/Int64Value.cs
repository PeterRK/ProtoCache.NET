// Copyright (c) 2025, Ruan Kunliang.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace ProtoCache {
    public struct Int64Value : IUnit {
        private long value;

        public readonly long Value => value;

        public Int64Value() {
            value = 0;
        }

        public void Init(ReadOnlyMemory<byte> data) {
            if (data.IsEmpty) {
                value = 0;
                return;
            }
            value = BitConverter.ToInt64(data.Span);
        }

        public void InitByField(ReadOnlyMemory<byte> data) {
            Init(data);
        }
    }
}
