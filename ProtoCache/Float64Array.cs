
namespace ProtoCache {
    public class Float64Array : IUnit.Object {
        private int size = 0;
        private ReadOnlyMemory<byte> body = null;

        public int Size {
            get { return size; }
        }

        public double Get(int idx) {
            return BitConverter.ToDouble(body.Span[(idx*8)..]);
        }

        public override void Init(ReadOnlyMemory<byte> data) {
            if (data.IsEmpty) {
                size = 0;
                body = null;
                return;
            }
            uint mark = BitConverter.ToUInt32(data.Span);
            if ((mark & 3) != 2) {
                throw new ArgumentException("illegal double array");
            }
            size = (int)(mark >> 2);
            body = data[4..];
        }
    }
}
