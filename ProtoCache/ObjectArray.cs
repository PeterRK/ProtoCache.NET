// Copyright (c) 2025, Ruan Kunliang.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ProtoCache {
    public class ObjectArray<T> : ArrayType
                where T : class, IUnit, new() {
        public override void Init(DataView data) => Init(data, 0);
        public T Get(int idx) => IUnit.NewByField<T>(At(idx));

        public T Get(int idx, T unit) => IUnit.InitByField(At(idx), unit);
    }
}
