// Copyright (c) 2025, Ruan Kunliang.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace ProtoCache {
    public class StringArray : ArrayType {
        private string[] list = [];

        public string Get(int idx) {
            if (list[idx] == null) {
                list[idx] = StringValue.Extract(IUnit.Jump(At(idx)));
            }
            return list[idx];
        }

        public override void Init(ReadOnlyMemory<byte> data) {
            base.Init(data);
            list = new string[Size];
        }
    }
}
