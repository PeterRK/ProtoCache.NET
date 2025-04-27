// Copyright (c) 2025, Ruan Kunliang.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace ProtoCache {
    public class Utils {
        private struct CompressContext {
            readonly byte[] src;
            public int k;
            public MemoryStream output;
            public CompressContext(byte[] src) {
                this.src = src;
                k = 0;
                output = new MemoryStream();
                int n = src.Length;
                while ((n & ~0x7f) != 0) {
                    output.WriteByte((byte)(0x80 | (n & 0x7f)));
                    n >>>= 7;
                }
                output.WriteByte((byte)n);
            }

            public int Pick() {
                int cnt = 1;
                byte ch = src[k++];
                if (ch == 0) {
                    while (k < src.Length && cnt < 4 && src[k] == 0) {
                        k++;
                        cnt++;
                    }
                    return 0x8 | (cnt - 1);
                } else if (ch == (byte)0xff) {
                    while (k < src.Length && cnt < 4 && src[k] == (byte)0xff) {
                        k++;
                        cnt++;
                    }
                    return 0xC | (cnt - 1);
                } else {
                    while (k < src.Length && cnt < 7 && src[k] != 0 && src[k] != (byte)0xff) {
                        k++;
                        cnt++;
                    }
                    return cnt;
                }
            }
        }

        public static byte[] Compress(byte[] src) {
            if (src.Length == 0) {
                return [];
            }
            CompressContext context = new(src);
            while (context.k < src.Length) {
                int x = context.k;
                int a = context.Pick();
                if (context.k == src.Length) {
                    context.output.WriteByte((byte)a);
                    if ((a & 0x8) == 0) {
                        context.output.Write(src, x, a);
                    }
                    break;
                }
                int y = context.k;
                int b = context.Pick();
                context.output.WriteByte((byte)(a | (b << 4)));
                if ((a & 0x8) == 0) {
                    context.output.Write(src, x, a);
                }
                if ((b & 0x8) == 0) {
                    context.output.Write(src, y, b);
                }
            }
            return context.output.ToArray();
        }

        private struct DecompressContext {
            readonly byte[] src;
            public byte[] output;
            public int k;
            public int off;
            public DecompressContext(byte[] src, int k, int size) {
                this.src = src;
                this.k = k;
                output = new byte[size];
                off = 0;
            }

            public bool Unpack(int mark) {
                if ((mark & 8) != 0) {
                    int cnt = (mark & 3) + 1;
                    if (off + cnt > output.Length) {
                        return false;
                    }
                    byte v = 0;
                    if ((mark & 4) != 0) {
                        v = (byte)0xff;
                    }
                    for (; cnt != 0; cnt--) {
                        output[off++] = v;
                    }
                } else {
                    int l = mark & 7;
                    if (k + l > src.Length) {
                        return false;
                    }
                    for (; l != 0; l--) {
                        output[off++] = src[k++];
                    }
                }
                return true;
            }
        }

        public static byte[] Decompress(byte[] src) {
            if (src.Length == 0) {
                return [];
            }
            int k = 0;
            int size = 0;
            for (int sft = 0; sft < 32; sft += 7) {
                byte b = src[k++];
                size |= ((int)b & 0x7f) << sft;
                if ((b & 0x80) == 0) {
                    break;
                }
            }
            DecompressContext context = new(src, k, size);
            while (context.k < src.Length) {
                int mark = src[context.k++] & 0xff;
                if (!context.Unpack(mark & 0xf) || !context.Unpack(mark >> 4)) {
                    throw new ArgumentException("broken data");
                }
            }
            if (context.off != size) {
                throw new ArgumentException("size mismatch");
            }
            return context.output;
        }
    }
}
