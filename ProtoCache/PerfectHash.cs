using System.Collections;

namespace ProtoCache {
    public sealed class PerfectHash {
        private readonly ReadOnlyMemory<byte> data;
        private readonly int size;
        private readonly int section;

        public ReadOnlyMemory<byte> Data {
            get { return data; }
        }
        public int Size {
            get { return size;  }
        }

        public PerfectHash(ReadOnlyMemory<byte> raw) {
            if (raw.Length < 4) {
                throw new ArgumentException("too short data");
            }
            size = BitConverter.ToInt32(raw.Span) & 0xfffffff;
            if (size < 2) {
                section = 0;
                data = raw[..4];
                return;
            }
            section = CalcSectionSize(size);
            var n = CalcBitmapSize(section);
            if (size > 0xffff) {
                n += n / 2;
            } else if (size > 0xff) {
                n += n / 4;
            } else if (size > 24) {
                n += n / 8;
            }
            var byteSize = n + 8;
            if (raw.Length < byteSize) {
                throw new ArgumentException("too short data");
            }
            data = raw[..byteSize];
        }

        private PerfectHash(Memory<byte> data, int size, int section) {
            this.data = data;
            this.size = size;
            this.section = section;
        }

        private static int CalcSectionSize(int size) {
            return Math.Max(10, (int)((size * 105L + 255) / 256));
        }

        private static int CalcBitmapSize(int section) {
            return (((section * 3 + 31) & ~31) / 4);
        }

        private static int CountValidSlot(ulong v) {
            v &= (v >> 1);
            v = (v & 0x1111111111111111UL) + ((v >> 2) & 0x1111111111111111UL);
            v += (v >> 4);
            v += (v >> 8);
            v = (v & 0xf0f0f0f0f0f0f0fUL) + ((v >> 16) & 0xf0f0f0f0f0f0f0fUL);
            v += (v >> 32);
            return 32 - ((int)v & 0xff);
        }

        private static int[] CalcSlots(uint seed, int section, ReadOnlySpan<byte> key) {
            var slots = new int[3];
            var h = Hash.Hash128(key, (ulong)seed & 0xffffffffL);
            slots[0] = (int)((uint)h.low % (uint)section);
            slots[1] = (int)((uint)(h.low >> 32) % (uint)section) + section;
            slots[2] = (int)((uint)h.high % (uint)section) + section * 2;
            return slots;
        }

        private static int Bit2(ReadOnlySpan<byte> data, int pos) {
            var blk = pos >>> 2;
            var sft = (pos & 3) << 1;
            return (data[blk] >>> sft) & 3;
        }

        private static void SetBit2on11(Span<byte> data, int pos, int val) {
            var blk = pos >>> 2;
            var sft = (pos & 3) << 1;
            data[blk] ^= (byte)((~val & 3) << sft);
        }


        private static byte[] Build(IKeySource src, int width) {
            var size = src.Total();
            var section = CalcSectionSize(size);
            var bmsz = CalcBitmapSize(section);
            var bytes = 8 + bmsz;
            if (bmsz > 8) {
                bytes += (bmsz / 8) * width;
            }
            var data = new byte[bytes];
            var view = new Span<byte>(data);

            var graph = new Graph(size);
            var free = new int[size];
            var book = new BitArray(section*3);

            var bitmap = view[8..];
            var table = view[(8 + bmsz)..];
            BitConverter.TryWriteBytes(view, size);

            var rand = new Random();
            for (int chance = (width == 1) ? 40 : 16; chance >= 0; chance--) {
                var seed = (uint)rand.Next();
                BitConverter.TryWriteBytes(view[4..], seed);
                graph.Init(seed, src);
                if (!graph.Tear(free, book)) {
                    continue;
                }
                graph.Mapping(free, book, bitmap);
                if (bmsz > 8) {
                    int cnt = 0;
                    switch (width) {
                        case 4:
                            for (int i = 0; i < bmsz / 8; i++) {
                                BitConverter.TryWriteBytes(table[(i*4)..], cnt);
                                cnt += CountValidSlot(BitConverter.ToUInt64(bitmap[(i*8)..]));
                            }
                            break;
                        case 2:
                            for (int i = 0; i < bmsz / 8; i++) {
                                BitConverter.TryWriteBytes(table[(i*2)..], (ushort)cnt);
                                cnt += CountValidSlot(BitConverter.ToUInt64(bitmap[(i * 8)..]));
                            }
                            break;
                        default:
                            for (int i = 0; i < bmsz / 8; i++) {
                                table[i] = (byte)cnt;
                                cnt += CountValidSlot(BitConverter.ToUInt64(bitmap[(i * 8)..]));
                            }
                            break;
                    }
                    if (cnt != size) {
                        throw new Exception("item lost");
                    }
                }
                return data;
            }

            throw new Exception("fail to build perfect-hash");
        }

        public static PerfectHash Build(IKeySource src) {
            int size = src.Total();
            if (size < 0 || size > 0xfffffff) {
                throw new ArgumentException("illegal size");
            }
            byte[] data;
            if (size > 0xffff) {
                data = Build(src, 4);
            } else if (size > 0xff) {
                data = Build(src, 2);
            } else if (size > 1) {
                data = Build(src, 1);
            } else {
                data = new byte[4];
                BitConverter.TryWriteBytes(data, size);
                return new PerfectHash(data, size, 0);
            }
            return new PerfectHash(data, size, CalcSectionSize(size));
        }

        public int Locate(ReadOnlySpan<byte> key) {
            if (size < 2) {
                return 0;
            }
            var slots = CalcSlots(BitConverter.ToUInt32(data[4..].Span), section, key);

            var bitmap = data[8..].Span;
            var table = data[(8+CalcBitmapSize(section))..].Span;

            var m = Bit2(bitmap, slots[0]) + Bit2(bitmap, slots[1]) + Bit2(bitmap, slots[2]);
            var slot = slots[m % 3];

            var a = slot >>> 5;
            var b = slot & 31;

            int off = 0;
            if (size > 0xffff) {
                off = (int)BitConverter.ToUInt32(table[(a*4)..]);
            } else if (size > 0xff) {
                off = (int)BitConverter.ToUInt16(table[(a*2)..]);
            } else if (size > 24) {
                off = (int)table[a];
            }

            var block = BitConverter.ToUInt64(bitmap[(a*8)..]);
            block |= 0xffffffffffffffffL << (b << 1);
            return off + CountValidSlot(block);
        }

        public interface IKeySource {
            void Reset();
            int Total();
            ReadOnlySpan<byte> Next();
        }

        private struct Vertex {
            public int slot;
            public int prev;
            public int next;
        }

        private sealed class Graph {
            public Vertex[][] edges;
            public int[] nodes;

            public Graph(int size) {
                edges = new Vertex[size][];
                for (int i = 0; i < size; i++) {
                    edges[i] = new Vertex[3];
                }
                var section = CalcSectionSize(size);
                nodes = new int[section * 3];
            }

            private static bool TestAndSet(BitArray book, int pos) {
                if (book.Get(pos)) {
                    return false;
                }
                book.Set(pos, true);
                return true;
            }

            public void Init(uint seed, IKeySource src) {
                Array.Fill(nodes, -1);
                var section = nodes.Length / 3;
                var total = src.Total();
                src.Reset();
                for (int i = 0; i < total; i++) {
                    var slots = CalcSlots(seed, section, src.Next());
                    Vertex[] edge = edges[i];
                    for (int j = 0; j < 3; j++) {
                        edge[j].slot = slots[j];
                        edge[j].prev = -1;
                        edge[j].next = nodes[edge[j].slot];
                        nodes[edge[j].slot] = i;
                        if (edge[j].next != -1) {
                            edges[edge[j].next][j].prev = i;
                        }
                    }
                }
            }

            public bool Tear(int[] free, BitArray book) {
                book.SetAll(false);

                int tail = 0;
                for (int i = edges.Length - 1; i >= 0; i--) {
                    Vertex[] edge = edges[i];
                    for (int j = 0; j < 3; j++) {
                        if (edge[j].prev == -1 && edge[j].next == -1 && TestAndSet(book, i)) {
                            free[tail++] = i;
                        }
                    }
                }

                for (int head = 0; head < tail; head++) {
                    var curr = free[head];
                    Vertex[] edge = edges[curr];
                    for (int j = 0; j < 3; j++) {
                        var i = -1;
                        if (edge[j].prev != -1) {
                            i = edge[j].prev;
                            edges[i][j].next = edge[j].next;
                        }
                        if (edge[j].next != -1) {
                            i = edge[j].next;
                            edges[i][j].prev = edge[j].prev;
                        }
                        if (i == -1) {
                            continue;
                        }
                        if (edges[i][j].prev == -1 && edges[i][j].next == -1 && TestAndSet(book, i)) {
                            free[tail++] = i;
                        }
                    }
                }

                return tail == free.Length;
            }

            public void Mapping(int[] free, BitArray book, Span<byte> data) {
                book.SetAll(false);
                data.Fill(byte.MaxValue);

                for (int i = free.Length - 1; i >= 0; i--) {
                    Vertex[] edge = edges[free[i]];
                    int a = edge[0].slot;
                    int b = edge[1].slot;
                    int c = edge[2].slot;
                    if (TestAndSet(book, a)) {
                        book.Set(b, true);
                        book.Set(c, true);
                        int sum = Bit2(data, b) + Bit2(data, c);
                        SetBit2on11(data, a, (6 - sum) % 3);
                    } else if (TestAndSet(book, b)) {
                        book.Set(c, true);
                        int sum = Bit2(data, a) + Bit2(data, c);
                        SetBit2on11(data, b, (7 - sum) % 3);
                    } else if (TestAndSet(book, c)) {
                        int sum = Bit2(data, a) + Bit2(data, b);
                        SetBit2on11(data, c, (8 - sum) % 3);
                    } else {
                        throw new Exception("broken graph");
                    }
                }
            }
        }

    }
}