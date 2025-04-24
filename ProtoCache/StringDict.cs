// Copyright (c) 2025, Ruan Kunliang.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

using System.Text;

namespace ProtoCache {
    public abstract class StringDict : DictType  {
        protected void Init(DataView data, int word) => Init(data, 0, word);
        public string Key(int idx) => Bytes.ExtractString(IUnit.Jump(KeyAt(idx)));

        public int Find(string key) {
            int idx = index.Locate(Encoding.UTF8.GetBytes(key));
            if (idx >= index.Size || !key.Equals(Key(idx))) {
                return -1;
            }
            return idx;
        }

        public class BoolValue : StringDict {
            public override void Init(DataView data) => Init(data, 1);
            public bool Value(int idx) => ValueAt(idx).GetBool();
        }

        public class Int32Value : StringDict {
            public override void Init(DataView data) => Init(data, 1);
            public int Value(int idx) => ValueAt(idx).GetInt32();
        }

        public class UInt32Value : StringDict {
            public override void Init(DataView data) => Init(data, 1);
            public uint Value(int idx) => ValueAt(idx).GetUInt32();
        }

        public class Float32Value : StringDict {
            public override void Init(DataView data) => Init(data, 1);
            public float Value(int idx) => ValueAt(idx).GetFloat32();
        }

        public class Int64Value : StringDict {
            public override void Init(DataView data) => Init(data, 2);
            public long Value(int idx) => ValueAt(idx).GetInt64();
        }

        public class UInt64Value : StringDict {
            public override void Init(DataView data) => Init(data, 2);
            public ulong Value(int idx) => ValueAt(idx).GetUInt64();
        }

        public class Float64Value : StringDict {
            public override void Init(DataView data) => Init(data, 2);
            public double Value(int idx) => ValueAt(idx).GetFloat64();
        }

        public class BytesValue : StringDict {
            public override void Init(DataView data) => Init(data, 0);
            public byte[] Value(int idx) => Bytes.ExtractBytes(IUnit.Jump(ValueAt(idx)));
        }

        public class StringValue : StringDict {
            public override void Init(DataView data) => Init(data, 0);
            public string Value(int idx) => Bytes.ExtractString(IUnit.Jump(ValueAt(idx)));
        }

        public class ObjectValue<T> : StringDict where T : class, IUnit, new() {
            public override void Init(DataView data) => Init(data, 0);
            public T Value(int idx) => IUnit.NewByField<T>(ValueAt(idx));
            public T Value(int idx, T unit) => IUnit.InitByField(ValueAt(idx), unit);
        }
    }
}
