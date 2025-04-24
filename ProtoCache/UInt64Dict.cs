using System.Text;

namespace ProtoCache {
    public class UInt64Dict<T> : DictType<T>
        where T : IUnit, new() {
        public ulong Key(int idx) {
            return BitConverter.ToUInt64(KeyAt(idx).Span);
        }

        public int Find(ulong key) {
            int idx = index.Locate(BitConverter.GetBytes(key));
            if (idx >= index.Size || key != Key(idx)) {
                return -1;
            }
            return idx;
        }
    }
}
