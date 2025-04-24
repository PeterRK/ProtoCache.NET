// Copyright (c) 2025, Ruan Kunliang.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace ProtoCache {
    public struct Message {
        private static readonly DataView empty = new(new byte[4]);

        private DataView data;

        public Message() => data = empty;

        public void Init(DataView data) {
            if (!data.IsValid) {
                this.data = empty;
                return;
            }
            this.data = data;
        }


        public readonly bool HasField(int id) {
            var header = data.GetUInt32();
            int section = (byte)header;
            if (id < 12) {
                var v = header >> 8;
                var width = (int)(v >> (id << 1)) & 3;
                return width != 0;
            } else {
                int a = (id - 12) / 25;
                int b = (id - 12) % 25;
                if (a >= section) {
                    return false;
                }
                var v = data.GetUInt64(4 + a * 8);
                var width = (int)(v >> (b << 1)) & 3;
                return width != 0;
            }
        }

        public static int Count32(uint v) {
            v = (v & 0x33333333) + ((v >> 2) & 0x33333333);
            v += (v >> 4);
            v = (v & 0xf0f0f0f) + ((v >> 8) & 0xf0f0f0f);
            v += (v >> 16);
            return (int)v & 0xff;
        }

        public static int Count64(ulong v) {
            v = (v & 0x3333333333333333L) + ((v >> 2) & 0x3333333333333333L);
            v += (v >> 4);
            v = (v & 0xf0f0f0f0f0f0f0fL) + ((v >> 8) & 0xf0f0f0f0f0f0f0fL);
            v += (v >> 16);
            v += (v >> 32);
            return (int)v & 0xff;
        }

        private readonly DataView GetField(int id) {
            var header = data.GetUInt32();
            int section = (byte)header;
            int off = 1 + section * 2;
            if (id < 12) {
                var v = header >> 8;
                var width = (int)(v >> (id << 1)) & 3;
                if (width == 0) {
                    return DataView.Null;
                }
                v &= ~(0xffffffff << (id << 1));
                off += Count32(v);
            } else {
                int a = (id - 12) / 25;
                int b = (id - 12) % 25;
                if (a >= section) {
                    return DataView.Null;
                }
                var v = data.GetUInt64(4 + a * 8);
                var width = (int)(v >> (b << 1)) & 3;
                if (width == 0) {
                    return DataView.Null;
                }
                off += (int)(v >> 50);
                v &= ~(0xffffffffffffffffL << (b << 1));
                off += Count64(v);
            }
            return data.Forward(off * 4);
        }

        public readonly bool GetBool(int id) {
            var field = GetField(id);
            if (!field.IsValid) {
                return false;
            }
            return field.Span[0] != 0;
        }

        public readonly int GetInt32(int id) {
            var field = GetField(id);
            if (!field.IsValid) {
                return 0;
            }
            return field.GetInt32();
        }

        public readonly long GetInt64(int id) {
            var field = GetField(id);
            if (!field.IsValid) {
                return 0;
            }
            return field.GetInt64();
        }

        public readonly uint GetUInt32(int id) {
            var field = GetField(id);
            if (!field.IsValid) {
                return 0;
            }
            return field.GetUInt32();
        }

        public readonly ulong GetUInt64(int id) {
            var field = GetField(id);
            if (!field.IsValid) {
                return 0;
            }
            return field.GetUInt64();
        }

        public readonly float GetFloat32(int id) {
            var field = GetField(id);
            if (!field.IsValid) {
                return 0;
            }
            return field.GetFloat32();
        }

        public readonly double GetFloat64(int id) {
            var field = GetField(id);
            if (!field.IsValid) {
                return 0;
            }
            return field.GetFloat64();
        }

        public readonly byte[] GetBytes(int id) {
            var field = GetField(id);
            if (!field.IsValid) {
                return [];
            }
            return Bytes.ExtractBytes(IUnit.Jump(field));
        }

        public readonly string GetString(int id) {
            var field = GetField(id);
            if (!field.IsValid) {
                return "";
            }
            return Bytes.ExtractString(IUnit.Jump(field));
        }

        public readonly T GetObject<T>(int id) where T : class, IUnit, new() {
            var field = GetField(id);
            if (!field.IsValid) {
                var unit = new T();
                unit.Init(DataView.Null);
                return unit;
            } 
            return IUnit.NewByField<T>(field);
        }
    }
}
