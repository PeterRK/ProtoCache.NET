// Copyright (c) 2025, Ruan Kunliang.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace ProtoCache {
    public interface IUnit {
        void Init(DataView data);

        public static T InitByField<T>(DataView data, T unit) where T : class, IUnit {
            unit.Init(Jump(data));
            return unit;
        }
        public static T NewByField<T>(DataView data) where T : class, IUnit, new()
            => InitByField(data, new T());

        public static DataView Jump(DataView data) {
            var mark = data.GetUInt32();
            if ((mark & 3) == 3) {
                return data.Forward((int)(mark & 0xfffffffc));
            }
            return data;
        }
    }
}
