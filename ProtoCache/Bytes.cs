// Copyright (c) 2025, Ruan Kunliang.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

using System.Text;

namespace ProtoCache {
    public struct Bytes {
        private static (int Offset, int Length) ExtractRange(ReadOnlySpan<byte> data) {
            uint mark = 0;
            int off = 0;
            for (int sft = 0; sft < 32; sft += 7) {
                byte b = data[off++];
                mark |= ((uint)b & 0x7f) << sft;
                if ((b & 0x80) == 0) {
                    if ((mark & 3) != 0) {
                        break;
                    }
                    int size = (int)(mark >> 2);
                    if (size > data.Length - off) {
                        break;
                    }
                    return (off, size);
                }
            }
            throw new ArgumentException("illegal bytes");
        }

        public static ReadOnlySpan<byte> ExtractRaw(ReadOnlySpan<byte> data) {
            var range = ExtractRange(data);
            return data.Slice(range.Offset, range.Length);
        }

        public static ReadOnlySpan<byte> ExtractRaw(DataView data) => ExtractRaw(data.Span);

        public static ReadOnlyMemory<byte> ExtractRawMemory(DataView data) {
            var range = ExtractRange(data.Span);
            return data.Memory.Slice(range.Offset, range.Length);
        }

        public static string ExtractString(DataView data) => Encoding.UTF8.GetString(ExtractRaw(data));
    }
}
