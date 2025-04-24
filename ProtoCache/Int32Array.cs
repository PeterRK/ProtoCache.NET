
namespace ProtoCache {
    public class Int32Array : IUnit.Object {
        private int size = 0;
        private ReadOnlyMemory<byte> body = null;

        public int Size {
            get { return size; }
        }

        public int Get(int idx) {
            return BitConverter.ToInt32(body.Span[(idx*4)..]);
        }

        public override void Init(ReadOnlyMemory<byte> data) {
            if (data.IsEmpty) {
                size = 0;
                body = null;
                return;
            }
            uint mark = BitConverter.ToUInt32(data.Span);
            if ((mark & 3) != 1) {
                throw new ArgumentException("illegal int32 array");
            }
            size = (int)(mark >> 2);
            body = data[4..];
        }
    }
}
