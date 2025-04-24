// Copyright (c) 2025, Ruan Kunliang.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace ProtoCache {
    public struct BoolValue : IUnit {
        private bool value;

        public readonly bool Value => value;

        public BoolValue() {
            value = false;
        }

        public void Init(ReadOnlyMemory<byte> data) {
            if (data.IsEmpty) {
                value = false;
                return;
            }
            value = BitConverter.ToBoolean(data.Span);
        }

        public void InitByField(ReadOnlyMemory<byte> data) {
            Init(data);
        }
    }
}
