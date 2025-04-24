
namespace ProtoCache {
    public struct UInt64Value : IUnit {
        private ulong value;

        public readonly ulong Value {
            get { return value; }
        }

        public UInt64Value() {
            value = 0;
        }

        public void Init(ReadOnlyMemory<byte> data) {
            if (data.IsEmpty) {
                value = 0;
                return;
            }
            value = BitConverter.ToUInt64(data.Span);
        }

        public void InitByField(ReadOnlyMemory<byte> data) {
            Init(data);
        }
    }
}
