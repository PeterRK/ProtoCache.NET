
namespace ProtoCache {
    public class UInt64Array : IUnit.Object {
        private int size = 0;
        private ReadOnlyMemory<byte> body = null;

        public int Size {
            get { return size; }
        }

        public ulong Get(int idx) {
            return BitConverter.ToUInt64(body.Span[(idx*8)..]);
        }

        public override void Init(ReadOnlyMemory<byte> data) {
            if (data.IsEmpty) {
                size = 0;
                body = null;
                return;
            }
            uint mark = BitConverter.ToUInt32(data.Span);
            if ((mark & 3) != 2) {
                throw new ArgumentException("illegal uint64 array");
            }
            size = (int)(mark >> 2);
            body = data[4..];
        }
    }
}
