
namespace ProtoCache {
    public abstract class DictType<T> : IUnit.Object
        where T : IUnit, new() {
        private static readonly PerfectHash empty = new(new byte[4]);
        protected int keyWidth = 4;
        protected int valueWidth = 4;
        protected PerfectHash index = empty;
        protected ReadOnlyMemory<byte> body = null;

        public int Size {
            get { return index != null? index.Size : 0; }
        }

        protected ReadOnlyMemory<byte> KeyAt(int idx) {
            var offset = idx * (keyWidth + valueWidth);
            return body[offset..];
        }

        private ReadOnlyMemory<byte> ValueAt(int idx) {
            var offset = idx * (keyWidth + valueWidth) + keyWidth;
            return body[offset..];
        }

        public T Value(int idx) {
            return IUnit.NewByField<T>(ValueAt(idx));
        }

        public override void Init(ReadOnlyMemory<byte> data) {
            if (data.IsEmpty) {
                keyWidth = 4;
                valueWidth = 4;
                index = empty;
                body = null;
                return;
            }

            index = new PerfectHash(data);
            var bodyOffset = (int)((index.Data.Length + 3) & 0xfffffffc);
            uint mark = BitConverter.ToUInt32(data.Span);
            keyWidth = (int)((mark >> 30) & 3) * 4;
            valueWidth = (int)((mark >> 28) & 3) * 4;
            if (keyWidth == 0 || valueWidth == 0) {
                throw new ArgumentException("illegal map");
            }
            body = data[bodyOffset..];
        }
    }
}
