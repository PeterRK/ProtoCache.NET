
namespace ProtoCache {
    public struct UInt32Value : IUnit {
        private uint value;

        public readonly uint Value {
            get { return value; }
        }

        public UInt32Value() {
            value = 0;
        }

        public void Init(ReadOnlyMemory<byte> data) {
            if (data.IsEmpty) {
                value = 0;
                return;
            }
            value = BitConverter.ToUInt32(data.Span);
        }

        public void InitByField(ReadOnlyMemory<byte> data) {
            Init(data);
        }
    }
}
