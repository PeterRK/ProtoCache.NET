// Copyright (c) 2025, Ruan Kunliang.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace ProtoCache {
    public struct Float32Value : IUnit {
        private float value;

        public readonly float Value => value;

        public Float32Value() {
            value = 0;
        }

        public void Init(ReadOnlyMemory<byte> data) {
            if (data.IsEmpty) {
                value = 0;
                return;
            }
            value = BitConverter.ToSingle(data.Span);
        }

        public void InitByField(ReadOnlyMemory<byte> data) {
            Init(data);
        }
    }
}
