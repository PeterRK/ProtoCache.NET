// Copyright (c) 2025, Ruan Kunliang.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

using System;
using System.Buffers.Binary;

namespace ProtoCache {
    public abstract class Int64Dict : DictType {
        protected void Init(DataView data, int word) => Init(data, 2, word);
        public long Key(int idx) => KeyAt(idx).GetInt64();

        public int Find(long key) {
            Span<byte> raw = stackalloc byte[8];
            BinaryPrimitives.WriteInt64LittleEndian(raw, key);
            int idx = index.Locate(raw);
            if (idx >= index.Size || key != Key(idx)) {
                return -1;
            }
            return idx;
        }

        public class BoolValue : Int64Dict {
            public override void Init(DataView data) => Init(data, 1);
            public bool Value(int idx) => ValueAt(idx).GetBool();
        }

        public class Int32Value : Int64Dict {
            public override void Init(DataView data) => Init(data, 1);
            public int Value(int idx) => ValueAt(idx).GetInt32();
        }

        public class UInt32Value : Int64Dict {
            public override void Init(DataView data) => Init(data, 1);
            public uint Value(int idx) => ValueAt(idx).GetUInt32();
        }

        public class Float32Value : Int64Dict {
            public override void Init(DataView data) => Init(data, 1);
            public float Value(int idx) => ValueAt(idx).GetFloat32();
        }

        public class Int64Value : Int64Dict {
            public override void Init(DataView data) => Init(data, 2);
            public long Value(int idx) => ValueAt(idx).GetInt64();
        }

        public class UInt64Value : Int64Dict {
            public override void Init(DataView data) => Init(data, 2);
            public ulong Value(int idx) => ValueAt(idx).GetUInt64();
        }

        public class Float64Value : Int64Dict {
            public override void Init(DataView data) => Init(data, 2);
            public double Value(int idx) => ValueAt(idx).GetFloat64();
        }

        public class BytesValue : Int64Dict {
            public override void Init(DataView data) => Init(data, 0);
            public ReadOnlySpan<byte> Value(int idx) => ValueBytesAt(idx);
        }

        public class StringValue : Int64Dict {
            public override void Init(DataView data) => Init(data, 0);
            public string Value(int idx) => Bytes.ExtractString(IUnit.Jump(ValueAt(idx)));
        }

        public class ObjectValue<T> : Int64Dict where T : class, IUnit, new() {
            public override void Init(DataView data) => Init(data, 0);
            public T Value(int idx) => IUnit.NewByField<T>(ValueAt(idx));
            public T Value(int idx, T unit) => IUnit.InitByField(ValueAt(idx), unit);
        }
    }
}
