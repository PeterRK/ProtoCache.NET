using System.Text;

namespace ProtoCache {
    public struct StringValue : IUnit {
        private string value;

        public readonly string Value {
            get { return value; }
        }

        public StringValue() {
            value = "";
        }

        public static string Extract(ReadOnlyMemory<byte> data) {
            var view = BytesValue.ExtractRaw(data);
            return Encoding.UTF8.GetString(view.Span);
        }

        public void Init(ReadOnlyMemory<byte> data) {
            if (data.IsEmpty) {
                value = "";
                return;
            }
            value = Extract(data);
        }

        public void InitByField(ReadOnlyMemory<byte> data) {
            Init(IUnit.Jump(data));
        }
    }
}
