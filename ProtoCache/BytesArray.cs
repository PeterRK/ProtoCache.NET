
namespace ProtoCache {
    public class BytesArray : ArrayType {
        private byte[][] list = [];
        public byte[] Get(int idx) {
            if (list[idx] == null) {
                list[idx] = BytesValue.Extract(IUnit.Jump(At(idx)));
            }
            return list[idx];
        }

        public override void Init(ReadOnlyMemory<byte> data) {
            base.Init(data);
            list = new byte[Size][];
        }
    }
}
