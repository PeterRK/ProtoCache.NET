
namespace ProtoCache {
    public class BoolArray : IUnit.Object {
        private bool[] value = [];

        public int Size {
            get { return value.Length; }
        }

        public bool Get(int idx) {
            return value[idx];
        }

        public override void Init(ReadOnlyMemory<byte> data) {
            if (data.IsEmpty) {
                value = [];
                return;
            }
            var view = BytesValue.Extract(data);
            value = new bool[view.Length];
            for (int i = 0; i < view.Length; i++) {
                value[i] = view[i] != 0;
            }
        }
    }
}
