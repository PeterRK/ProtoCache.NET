// Copyright (c) 2025, Ruan Kunliang.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace ProtoCache {
    public readonly struct DataView(byte[] data, int offset = 0) {
        private readonly byte[] data = data;
        private readonly int start = offset;

        public static readonly DataView Null = new ([], -1);
        public static readonly DataView Empty = new([], 0);

        public bool IsValid => start >= 0;
        public int Size => data.Length - start;
        public ReadOnlySpan<byte> Span => data.AsSpan()[start..];
        public ReadOnlyMemory<byte> Memory => new ReadOnlyMemory<byte>(data)[start..];
        public DataView Forward(int delta) => new DataView(data, start + delta);

        public byte GetByte(int off = 0) => data[start + off];
        public bool GetBool(int off = 0) => data[start + off] != 0;
        public short GetInt16(int off = 0) => BitConverter.ToInt16(data, start + off);
        public ushort GetUInt16(int off = 0) => BitConverter.ToUInt16(data, start + off);
        public int GetInt32(int off = 0) => BitConverter.ToInt32(data, start+off);
        public uint GetUInt32(int off = 0) => BitConverter.ToUInt32(data, start + off);
        public long GetInt64(int off = 0) => BitConverter.ToInt64(data, start + off);
        public ulong GetUInt64(int off = 0) => BitConverter.ToUInt64(data, start + off);
        public float GetFloat32(int off = 0) => BitConverter.ToSingle(data, start + off);
        public double GetFloat64(int off = 0) => BitConverter.ToDouble(data, start + off);
    }
}
