using System.Text;

namespace ProtoCache {
    public class Int64Dict<T> : DictType<T>
        where T : IUnit, new() {
        public long Key(int idx) {
            return BitConverter.ToInt64(KeyAt(idx).Span);
        }

        public int Find(long key) {
            int idx = index.Locate(BitConverter.GetBytes(key));
            if (idx >= index.Size || key != Key(idx)) {
                return -1;
            }
            return idx;
        }
    }
}
