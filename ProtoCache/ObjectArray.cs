
namespace ProtoCache {
    public class ObjectArray<T> : ArrayType
                where T : class, IUnit, new() {
        private T[] list = [];

        public T Get(int idx) {
            if (list[idx] == null) {
                list[idx] = IUnit.NewByField<T>(At(idx));
            }
            return list[idx];
        }

        public override void Init(ReadOnlyMemory<byte> data) {
            base.Init(data);
            list = new T[Size];
        }
    }
}
