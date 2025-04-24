
namespace ProtoCache {
    public class Float32Array : IUnit.Object {
        private int size = 0;
        private ReadOnlyMemory<byte> body = null;

        public int Size {
            get { return size; }
        }

        public float Get(int idx) {
            return BitConverter.ToSingle(body.Span[(idx*4)..]);
        }

        public override void Init(ReadOnlyMemory<byte> data) {
            if (data.IsEmpty) {
                size = 0;
                body = null;
                return;
            }
            uint mark = BitConverter.ToUInt32(data.Span);
            if ((mark & 3) != 1) {
                throw new ArgumentException("illegal float array");
            }
            size = (int)(mark >> 2);
            body = data[4..];
        }
    }
}
