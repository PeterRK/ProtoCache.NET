
namespace ProtoCache {
    public class UInt32Dict<T> : DictType<T>
        where T : IUnit, new() {
        public uint Key(int idx) {
            return BitConverter.ToUInt32(KeyAt(idx).Span);
        }

        public int Find(uint key) {
            int idx = index.Locate(BitConverter.GetBytes(key));
            if (idx >= index.Size || key != Key(idx)) {
                return -1;
            }
            return idx;
        }
    }
}
