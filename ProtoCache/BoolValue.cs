
namespace ProtoCache {
    public struct BoolValue : IUnit {
        private bool value;

        public readonly bool Value {
            get { return value; }
        }

        public BoolValue() {
            value = false;
        }

        public void Init(ReadOnlyMemory<byte> data) {
            if (data.IsEmpty) {
                value = false;
                return;
            }
            value = BitConverter.ToBoolean(data.Span);
        }

        public void InitByField(ReadOnlyMemory<byte> data) {
            Init(data);
        }
    }
}
