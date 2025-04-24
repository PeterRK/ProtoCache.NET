// Copyright (c) 2025, Ruan Kunliang.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace ProtoCache {
    public struct Message : IUnit {
        private static readonly byte[] empty = new byte[4];

        static Message() {
            BitConverter.TryWriteBytes(empty.AsSpan(), (uint)0);
        }

        private ReadOnlyMemory<byte> data;

        public Message() {
            data = empty;
        }

        public void Init(ReadOnlyMemory<byte> data) {
            if (data.IsEmpty) {
                this.data = empty;
                return;
            }
            this.data = data;
        }

        public void InitByField(ReadOnlyMemory<byte> data) {
            Init(IUnit.Jump(data));
        }


        public readonly bool HasField(int id) {
            int section = data.Span[0];
            if (id < 12) {
                var v = BitConverter.ToUInt32(data.Span) >> 8;
                var width = (int)(v >> (id << 1)) & 3;
                return width != 0;
            } else {
                int a = (id - 12) / 25;
                int b = (id - 12) % 25;
                if (a >= section) {
                    return false;
                }
                var v = BitConverter.ToUInt64(data.Span[(4+a*8)..]);
                var width = (int)(v >> (b << 1)) & 3;
                return width != 0;
            }
        }

        private readonly ReadOnlyMemory<byte> GetField(int id) {
            int section = data.Span[0];
            int off = 1 + section * 2;
            if (id < 12) {
                var v = BitConverter.ToUInt32(data.Span) >> 8;
                var width = (int)(v >> (id << 1)) & 3;
                if (width == 0) {
                    return ReadOnlyMemory<byte>.Empty;
                }
                v &= ~(0xffffffff << (id << 1));
                v = (v & 0x33333333) + ((v >> 2) & 0x33333333);
                v += (v >> 4);
                v = (v & 0xf0f0f0f) + ((v >> 8) & 0xf0f0f0f);
                v += (v >> 16);
                off += (int)v & 0xff;
            } else {
                int a = (id - 12) / 25;
                int b = (id - 12) % 25;
                if (a >= section) {
                    return ReadOnlyMemory<byte>.Empty;
                }
                var v = BitConverter.ToUInt64(data.Span[(4 + a * 8)..]);
                var width = (int)(v >> (b << 1)) & 3;
                if (width == 0) {
                    return ReadOnlyMemory<byte>.Empty;
                }
                off += (int)(v >> 50);
                v &= ~(0xffffffffffffffffL << (b << 1));
                v = (v & 0x3333333333333333L) + ((v >> 2) & 0x3333333333333333L);
                v += (v >> 4);
                v = (v & 0xf0f0f0f0f0f0f0fL) + ((v >> 8) & 0xf0f0f0f0f0f0f0fL);
                v += (v >> 16);
                v += (v >> 32);
                off += (int)v & 0xff;
            }
            return data[(off * 4)..];
        }

        public readonly bool GetBool(int id) {
            var field = GetField(id);
            if (field.IsEmpty) {
                return false;
            }
            return field.Span[0] != 0;
        }

        public readonly int GetInt32(int id) {
            var field = GetField(id);
            if (field.IsEmpty) {
                return 0;
            }
            return BitConverter.ToInt32(field.Span);
        }

        public readonly long GetInt64(int id) {
            var field = GetField(id);
            if (field.IsEmpty) {
                return 0;
            }
            return BitConverter.ToInt64(field.Span);
        }

        public readonly uint GetUInt32(int id) {
            var field = GetField(id);
            if (field.IsEmpty) {
                return 0;
            }
            return BitConverter.ToUInt32(field.Span);
        }

        public readonly ulong GetUInt64(int id) {
            var field = GetField(id);
            if (field.IsEmpty) {
                return 0;
            }
            return BitConverter.ToUInt64(field.Span);
        }

        public readonly float GetFloat32(int id) {
            var field = GetField(id);
            if (field.IsEmpty) {
                return 0;
            }
            return BitConverter.ToSingle(field.Span);
        }

        public readonly double GetFloat64(int id) {
            var field = GetField(id);
            if (field.IsEmpty) {
                return 0;
            }
            return BitConverter.ToDouble(field.Span);
        }

        public readonly byte[] GetBytes(int id)
            => IUnit.NewByField<BytesValue>(GetField(id)).Value;

        public readonly string GetString(int id)
            => IUnit.NewByField<StringValue>(GetField(id)).Value;

        public readonly T GetObject<T>(int id) where T : IUnit, new()
            => IUnit.NewByField<T>(GetField(id));
    }
}
