// Copyright (c) 2025, Ruan Kunliang.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

using Google.Protobuf;
using Google.Protobuf.Reflection;
using System.Collections;
using System.Text;

namespace ProtoCache {
    public sealed class ProtoCache {
        public static byte[] Serialize(IMessage message) {
            var descriptor = message.Descriptor;
            var originFields = descriptor.Fields.InFieldNumberOrder();
            if (originFields.Count == 0) {
                throw new ArgumentException(string.Format("no fields in {0}", descriptor.FullName));
            }
            int maxId = originFields.Last().FieldNumber;
            if (maxId > (12 + 25 * 255)) {
                throw new ArgumentException(string.Format("too many fields in {0}", descriptor.FullName));
            }
            if ((maxId - originFields.Count) > 6 && maxId > originFields.Count * 2) {
                throw new ArgumentException(string.Format("message {0} is too sparse", descriptor.FullName));
            }
            var fields = new FieldDescriptor[maxId];
            foreach (var field in originFields) {
                fields[field.FieldNumber - 1] = field;
            }

            var parts = new List<byte[]?>();
            foreach (var field in fields) {
                if (field == null) {
                    parts.Add(null);
                    continue;
                }
                var name = field.Name;
                if (field.IsMap) {
                    parts.Add(SerializeMap(field, field.Accessor.GetValue(message)));
                } else if (field.IsRepeated) {
                    parts.Add(SerializeList(field, field.Accessor.GetValue(message)));
                } else {
                    parts.Add(SerializeField(field, field.Accessor.GetValue(message)));
                }
            }

            if (fields.Length == 1 && (fields[0].Name.Equals("_") || fields[0].Name.Equals("_x_"))) {
                // trim message wrapper
                var unit = parts[0];
                if (unit == null) {
                    unit = new byte[4];
                    if (fields[0].IsMap) {
                        BitConverter.TryWriteBytes(unit, (uint)5 << 28);
                    } else {
                        BitConverter.TryWriteBytes(unit, (uint)1);
                    }
                }
                return unit;
            }

            while (parts.Count > 0 && parts.Last() == null) {
                parts.RemoveAt(parts.Count - 1);
            }
            if (parts.Count == 0) {
                var unit = new byte[4];
                BitConverter.TryWriteBytes(unit, (uint)0);
                return unit;
            }

            int section = (parts.Count + 12) / 25;
            if (section > 0xff) {
                throw new Exception("too many fields");
            }

            int size = 1 + section * 2;
            int cnt = 0;
            uint head = (uint)section;
            for (int i = 0; i < Math.Min(12, parts.Count); i++) {
                var one = parts[i];
                if (one == null) {
                    continue;
                }
                if (one.Length / 4 < 4) {
                    head |= (uint)(one.Length / 4) << (8 + i * 2);
                    size += one.Length / 4;
                    cnt += one.Length / 4;
                } else {
                    head |= 1U << (8 + i * 2);
                    size += 1 + one.Length / 4;
                    cnt += 1;
                }
            }
            for (int i = 12; i < parts.Count; i++) {
                var one = parts[i];
                if (one == null) {
                    continue;
                }
                if (one.Length / 4 < 4) {
                    size += one.Length / 4;
                } else {
                    size += 1 + one.Length / 4;
                }
                if (size >= (1 << 30)) {
                    throw new ArgumentException("message size overflow");
                }
            }
            var data = new byte[size * 4];
            BitConverter.TryWriteBytes(data, head);

            int off = 4;
            for (int i = 12; i < parts.Count;) {
                int next = Math.Min(i + 25, parts.Count);
                if (cnt >= (1 << 14)) {
                    throw new ArgumentException("message parts overflow");
                }
                ulong mark = ((ulong)cnt) << 50;
                for (int j = 0; i < next; j += 2) {
                    var one = parts[i++];
                    if (one == null) {
                        continue;
                    }
                    if (one.Length / 4 < 4) {
                        mark |= ((ulong)one.Length / 4) << j;
                        cnt += one.Length / 4;
                    } else {
                        mark |= 1UL << j;
                        cnt += 1;
                    }
                }
                BitConverter.TryWriteBytes(data.AsSpan()[off..], mark);
                off += 8;
            }

            foreach (var one in parts) {
                if (one == null) {
                    continue;
                }
                if (one.Length / 4 < 4) {
                    Array.Copy(one, 0, data, off, one.Length);
                    off += one.Length;
                } else {
                    off += 4;
                }
            }
            int tail = off;
            off = 4 + section * 8;
            foreach (var one in parts) {
                if (one == null) {
                    continue;
                }
                if (one.Length / 4 < 4) {
                    off += one.Length;
                } else {
                    BitConverter.TryWriteBytes(data.AsSpan()[off..], (uint)(tail - off) | 3);
                    Array.Copy(one, 0, data, tail, one.Length);
                    tail += one.Length;
                    off += 4;
                }
            }
            if (tail != data.Length) {
                throw new Exception("size mismatch");
            }
            return data;
        }

        private static byte[] Serialize(string value) => Serialize(Encoding.UTF8.GetBytes(value));

        private static byte[] Serialize(ByteString value) => Serialize(value.ToByteArray());

        private static byte[] Serialize(byte[] value) {
            if (value.Length >= (1 << 28)) {
                throw new ArgumentException("too long string");
            }

            var tmp = new byte[5];
            var mark = (uint)value.Length << 2;
            int w = 0;
            while ((mark & ~0x7f) != 0) {
                tmp[w++] = (byte)(0x80 | (mark & 0x7f));
                mark >>= 7;
            }
            tmp[w++] = (byte)mark;

            var data = new byte[((w + value.Length) + 3) & 0xfffffffc];
            Array.Copy(tmp, 0, data, 0, w);
            Array.Copy(value, 0, data, w, value.Length);
            return data;
        }

        private static byte[]? SerializeField(FieldDescriptor field, object value) {
            switch (field.FieldType) {
                case FieldType.Message: {
                        if (value == null) {
                            return null;
                        }
                        var data = Serialize((IMessage)value);
                        if (data.Length == 4) {
                            return null;
                        }
                        return data;
                    }
                case FieldType.Bytes: {
                        var v = (ByteString)value;
                        if (v.IsEmpty) {
                            return null;
                        }
                        return Serialize(v);
                    }
                case FieldType.String: {
                        var v = (string)value;
                        if (v.Length == 0) {
                            return null;
                        }
                        return Serialize(v);
                    }
                case FieldType.Double: {
                        var v = (double)value;
                        if (v == 0) {
                            return null;
                        }
                        return BitConverter.GetBytes(v);
                    }
                case FieldType.Float: {
                        var v = (float)value;
                        if (v == 0) {
                            return null;
                        }
                        return BitConverter.GetBytes(v);
                    }
                case FieldType.Fixed64:
                case FieldType.UInt64: {
                        var v = (ulong)value;
                        if (v == 0) {
                            return null;
                        }
                        return BitConverter.GetBytes(v);
                    }
                case FieldType.Fixed32:
                case FieldType.UInt32: {
                        var v = (uint)value;
                        if (v == 0) {
                            return null;
                        }
                        return BitConverter.GetBytes(v);
                    }
                case FieldType.SFixed64:
                case FieldType.SInt64:
                case FieldType.Int64: {
                        var v = (long)value;
                        if (v == 0) {
                            return null;
                        }
                        return BitConverter.GetBytes(v);
                    }
                case FieldType.SFixed32:
                case FieldType.SInt32:
                case FieldType.Int32:
                case FieldType.Enum: {
                        var v = (int)value;
                        if (v == 0) {
                            return null;
                        }
                        return BitConverter.GetBytes(v);
                    }
                case FieldType.Bool: {
                        var v = (bool)value;
                        if (!v) {
                            return null;
                        }
                        var data = new byte[4];
                        BitConverter.TryWriteBytes(data, v);
                        return data;
                    }
            }
            throw new ArgumentException(string.Format("unsupported field: {0}", field.FullName));
        }

        private static byte[]? SerializeList(FieldDescriptor field, object value) {
            switch (field.FieldType) {
                case FieldType.Message:
                    return SerializeObjectList([..((IEnumerable)value).Cast<IMessage>()], (IMessage value) => {
                        return Serialize(value);
                    });
                case FieldType.Bytes:
                    return SerializeObjectList((IList<ByteString>)value, (ByteString value) => {
                        return Serialize(value);
                    });
                case FieldType.String:
                    return SerializeObjectList((IList<string>)value, (string value) => {
                        return Serialize(value);
                    });
                case FieldType.Double:
                    return SerializeScalarList((IList<double>)value, 2, (Span<byte> buffer, double value) => {
                        BitConverter.TryWriteBytes(buffer, value);
                    });
                case FieldType.Float:
                    return SerializeScalarList((IList<float>)value, 1, (Span<byte> buffer, float value) => {
                        BitConverter.TryWriteBytes(buffer, value);
                    });
                case FieldType.Fixed64:
                case FieldType.UInt64:
                    return SerializeScalarList((IList<ulong>)value, 2, (Span<byte> buffer, ulong value) => {
                        BitConverter.TryWriteBytes(buffer, value);
                    });
                case FieldType.Fixed32:
                case FieldType.UInt32:
                    return SerializeScalarList((IList<uint>)value, 1, (Span<byte> buffer, uint value) => {
                        BitConverter.TryWriteBytes(buffer, value);
                    });
                case FieldType.SFixed64:
                case FieldType.SInt64:
                case FieldType.Int64:
                    return SerializeScalarList((IList<long>)value, 2, (Span<byte> buffer, long value) => {
                        BitConverter.TryWriteBytes(buffer, value);
                    });
                case FieldType.SFixed32:
                case FieldType.SInt32:
                case FieldType.Int32:
                    return SerializeScalarList((IList<int>)value, 1, (Span<byte> buffer, int value) => {
                        BitConverter.TryWriteBytes(buffer, value);
                    });
                case FieldType.Bool: {
                        var v = (IList<bool>)value;
                        if (v.Count == 0) {
                            return null;
                        }
                        var vec = new byte[v.Count];
                        for (int i = 0; i < v.Count; i++) {
                            vec[i] = v[i] ? (byte)1 : (byte)0;
                        }
                        return Serialize(vec);
                    }
                case FieldType.Enum:
                    return SerializeScalarList([.. ((IEnumerable)value).Cast<int>()], 1, (Span<byte> buffer, int value) => {
                        BitConverter.TryWriteBytes(buffer, value);
                    });
            }
            throw new ArgumentException(string.Format("unsupported field: {0}", field.FullName));
        }

        private struct BestArray(long size, int width) {
            public long size = size;
            public int width = width;
        }

        private static BestArray DetectBestArray(byte[][] parts) {
            var sizes = new long[] { 0, 0, 0 };
            foreach (var one in parts) {
                sizes[0] += 1;
                sizes[1] += 2;
                sizes[2] += 3;
                if (one.Length / 4 <= 1) {
                    continue;
                }
                sizes[0] += one.Length / 4;
                if (one.Length / 4 <= 2) {
                    continue;
                }
                sizes[1] += one.Length / 4;
                if (one.Length / 4 <= 3) {
                    continue;
                }
                sizes[2] += one.Length / 4;
            }
            int mode = 0;
            for (int i = 1; i < 3; i++) {
                if (sizes[i] < sizes[mode]) {
                    mode = i;
                }
            }
            return new BestArray(sizes[mode], mode + 1);
        }

        private delegate void BufferFiller<T>(Span<byte> buffer, T value);

        private static byte[]? SerializeScalarList<T>(IList<T> list, int width, BufferFiller<T> fill) {
            if (list.Count == 0) {
                return null;
            }
            if (list.Count >= (1 << 28)) {
                throw new ArgumentException("array size overflow");
            }
            var data = new byte[4 + list.Count * width * 4];
            BitConverter.TryWriteBytes(data, (uint)((list.Count << 2) | width));
            var view = new Span<byte>(data);
            int off = 4;
            foreach (var one in list) {
                fill(view[off..], one);
                off += width * 4;
            }
            return data;
        }

        private delegate byte[] ObjectSerializer<T>(T obj);

        private static byte[]? SerializeObjectList<T>(IList<T> list, ObjectSerializer<T> serialize) {
            if (list.Count == 0) {
                return null;
            }
            var parts = new byte[list.Count][];
            for (int i = 0; i < list.Count; i++) {
                parts[i] = serialize(list[i]);
            }
            BestArray ret = DetectBestArray(parts);
            ret.size += 1;
            if (ret.size >= (1 << 30)) {
                throw new ArgumentException("array size overflow");
            }
            var data = new byte[(int)ret.size * 4];
            BitConverter.TryWriteBytes(data, (uint)((parts.Length << 2) | ret.width));
            ret.width *= 4;

            int off = 4;
            foreach (var one in parts) {
                if (one.Length <= ret.width) {
                    Array.Copy(one, 0, data, off, one.Length);
                }
                off += ret.width;
            }
            int tail = off;
            off = 4;
            foreach (var one in parts) {
                if (one.Length > ret.width) {
                    BitConverter.TryWriteBytes(data.AsSpan()[off..], (uint)(tail - off) | 3);
                    Array.Copy(one, 0, data, tail, one.Length);
                    tail += one.Length;
                }
                off += ret.width;
            }
            if (tail != data.Length) {
                throw new Exception("size mismatch");
            }
            return data;
        }

        private static byte[]? SerializeMap(FieldDescriptor field, object obj) {
            var dict = (IDictionary)obj;
            if (dict.Count == 0) {
                return null;
            }
            var fields = field.MessageType.Fields.InFieldNumberOrder();
            var keyField = fields[0];
            var valueField = fields[1];

            var keys = new byte[dict.Count][];
            PerfectHash.IKeySource reader;
            switch (keyField.FieldType) {
                case FieldType.String: {
                        var raw = dict.Keys.Cast<string>().ToList();
                        for (int i = 0; i < raw.Count; i++) {
                            keys[i] = Serialize(raw[i]);
                        }
                        reader = new StrReader(keys);
                        break;
                    }
                case FieldType.Fixed64:
                case FieldType.UInt64: {
                        var raw = dict.Keys.Cast<ulong>().ToList();
                        for (int i = 0; i < raw.Count; i++) {
                            keys[i] = BitConverter.GetBytes(raw[i]);
                        }
                        reader = new SimpleReader(keys);
                        break;
                    }
                case FieldType.SFixed64:
                case FieldType.SInt64:
                case FieldType.Int64: {
                        var raw = dict.Keys.Cast<long>().ToList();
                        for (int i = 0; i < raw.Count; i++) {
                            keys[i] = BitConverter.GetBytes(raw[i]);
                        }
                        reader = new SimpleReader(keys);
                        break;
                    }
                case FieldType.Fixed32:
                case FieldType.UInt32:
                case FieldType.SFixed32: {
                        var raw = dict.Keys.Cast<uint>().ToList();
                        for (int i = 0; i < raw.Count; i++) {
                            keys[i] = BitConverter.GetBytes(raw[i]);
                        }
                        reader = new SimpleReader(keys);
                        break;
                    }
                case FieldType.SInt32:
                case FieldType.Int32: {
                        var raw = dict.Keys.Cast<int>().ToList();
                        for (int i = 0; i < raw.Count; i++) {
                            keys[i] = BitConverter.GetBytes(raw[i]);
                        }
                        reader = new SimpleReader(keys);
                        break;
                    }
                default:
                    throw new ArgumentException(string.Format("unsupported map key type: {0}", keyField.FieldType));
            }

            var values = new byte[dict.Count][];
            switch (valueField.FieldType) {
                case FieldType.Message: {
                        var raw = dict.Values.Cast<IMessage>().ToList();
                        for (int i = 0; i < raw.Count; i++) {
                            values[i] = Serialize(raw[i]);
                        }
                        break;
                    }
                case FieldType.Bytes: {
                        var raw = dict.Values.Cast<ByteString>().ToList();
                        for (int i = 0; i < raw.Count; i++) {
                            values[i] = Serialize(raw[i]);
                        }
                        break;
                    }
                case FieldType.String: {
                        var raw = dict.Values.Cast<string>().ToList();
                        for (int i = 0; i < raw.Count; i++) {
                            values[i] = Serialize(raw[i]);
                        }
                        break;
                    }
                case FieldType.Double: {
                        var raw = dict.Values.Cast<double>().ToList();
                        for (int i = 0; i < raw.Count; i++) {
                            values[i] = BitConverter.GetBytes(raw[i]);
                        }
                        break;
                    }
                case FieldType.Float: {
                        var raw = dict.Values.Cast<float>().ToList();
                        for (int i = 0; i < raw.Count; i++) {
                            values[i] = BitConverter.GetBytes(raw[i]);
                        }
                        break;
                    }
                case FieldType.Fixed64:
                case FieldType.UInt64: {
                        var raw = dict.Values.Cast<ulong>().ToList();
                        for (int i = 0; i < raw.Count; i++) {
                            values[i] = BitConverter.GetBytes(raw[i]);
                        }
                        break;
                    }
                case FieldType.Fixed32:
                case FieldType.UInt32: {
                        var raw = dict.Values.Cast<uint>().ToList();
                        for (int i = 0; i < raw.Count; i++) {
                            values[i] = BitConverter.GetBytes(raw[i]);
                        }
                        break;
                    }
                case FieldType.SFixed64:
                case FieldType.SInt64:
                case FieldType.Int64: {
                        var raw = dict.Values.Cast<long>().ToList();
                        for (int i = 0; i < raw.Count; i++) {
                            values[i] = BitConverter.GetBytes(raw[i]);
                        }
                        break;
                    }
                case FieldType.SFixed32:
                case FieldType.SInt32:
                case FieldType.Int32:
                case FieldType.Enum: {
                        var raw = dict.Values.Cast<int>().ToList();
                        for (int i = 0; i < raw.Count; i++) {
                            values[i] = BitConverter.GetBytes(raw[i]);
                        }
                        break;
                    }
                case FieldType.Bool: {
                        var raw = dict.Values.Cast<bool>().ToList();
                        for (int i = 0; i < raw.Count; i++) {
                            values[i] = new byte[4];
                            BitConverter.TryWriteBytes(values[i], raw[i]);
                        }
                        break;
                    }
                default:
                    throw new ArgumentException(string.Format("unsupported map value type: {0}", valueField.FieldType));
            }

            var index = PerfectHash.Build(reader);
            var outKeys = new byte[dict.Count][];
            var outValues = new byte[dict.Count][];
            reader.Reset();
            for (int i = 0; i < dict.Count; i++) {
                int pos = index.Locate(reader.Next());
                outKeys[pos] = keys[i];
                outValues[pos] = values[i];
            }

            int indexSize = (index.ByteSize + 3) / 4;
            var k = DetectBestArray(outKeys);
            var v = DetectBestArray(outValues);

            long size = indexSize;
            size += k.size + v.size;
            if (size >= (1 << 30)) {
                throw new ArgumentException("map size overflow");
            }
            var data = new byte[(int)size * 4];
            index.Data.CopyTo(data);
            var mark = BitConverter.ToUInt32(data);
            mark |= ((uint)k.width << 30) | ((uint)v.width << 28);
            BitConverter.TryWriteBytes(data, mark);

            k.width *= 4;
            v.width *= 4;

            int off = indexSize * 4;
            for (int i = 0; i < dict.Count; i++) {
                var key = outKeys[i];
                var val = outValues[i];
                if (key.Length <= k.width) {
                    Array.Copy(key, 0, data, off, key.Length);
                }
                off += k.width;
                if (val.Length <= v.width) {
                    Array.Copy(val, 0, data, off, val.Length);
                }
                off += v.width;
            }
            int tail = off;
            off = indexSize * 4;
            for (int i = 0; i < dict.Count; i++) {
                var key = outKeys[i];
                var val = outValues[i];
                if (key.Length > k.width) {
                    BitConverter.TryWriteBytes(data.AsSpan()[off..], (uint)(tail - off) | 3);
                    Array.Copy(key, 0, data, tail, key.Length);
                    tail += key.Length;
                }
                off += k.width;
                if (val.Length > v.width) {
                    BitConverter.TryWriteBytes(data.AsSpan()[off..], (uint)(tail - off) | 3);
                    Array.Copy(val, 0, data, tail, val.Length);
                    tail += val.Length;
                }
                off += v.width;
            }
            if (tail != data.Length) {
                throw new Exception("size mismatch");
            }
            return data;
        }

        private class SimpleReader(byte[][] data) : PerfectHash.IKeySource {
            protected byte[][] data = data;
            protected int idx = 0;

            public void Reset() => idx = 0;
            public int Total() => data.Length;
            public virtual ReadOnlySpan<byte> Next() => data[idx++];
        }

        private class StrReader(byte[][] data) : SimpleReader(data) {
            public override ReadOnlySpan<byte> Next() => Bytes.ExtractRaw(data[idx++].AsSpan());
        }
    }
}
