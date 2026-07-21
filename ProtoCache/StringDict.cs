// Copyright (c) 2025, Ruan Kunliang.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

using System.Buffers;
using System.Text;

namespace ProtoCache {
    public abstract class StringDict : DictType  {
        protected void Init(DataView data, int word) => Init(data, 0, word);
        public string Key(int idx) => Bytes.ExtractString(IUnit.Jump(KeyAt(idx)));
        private ReadOnlySpan<byte> KeyRaw(int idx) => Bytes.ExtractRaw(IUnit.Jump(KeyAt(idx)));

        public int Find(string key) {
            int byteCount = Encoding.UTF8.GetByteCount(key);
            byte[]? rented = null;
            Span<byte> buffer = byteCount <= 256
                ? stackalloc byte[byteCount]
                : (rented = ArrayPool<byte>.Shared.Rent(byteCount));
            try {
                Encoding.UTF8.GetBytes(key.AsSpan(), buffer);
                var utf8 = buffer[..byteCount];
                int idx = index.Locate(utf8);
                if (idx >= index.Size || !utf8.SequenceEqual(KeyRaw(idx))) {
                    return -1;
                }
                return idx;
            } finally {
                if (rented != null) {
                    ArrayPool<byte>.Shared.Return(rented);
                }
            }
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
            public ReadOnlySpan<byte> Value(int idx) => ValueBytesAt(idx);
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
