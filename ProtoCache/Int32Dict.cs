
namespace ProtoCache {
    public class Int32Dict<T> : DictType<T>
        where T : IUnit, new() {
        public int Key(int idx) {
            return BitConverter.ToInt32(KeyAt(idx).Span);
        }

        public int Find(int key) {
            int idx = index.Locate(BitConverter.GetBytes(key));
            if (idx >= index.Size || key != Key(idx)) {
                return -1;
            }
            return idx;
        }
    }
}
