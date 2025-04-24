
namespace ProtoCache {
    public interface IUnit {
        void Init(ReadOnlyMemory<byte> data);

        void InitByField(ReadOnlyMemory<byte> data);

        static T NewByField<T>(ReadOnlyMemory<byte> data) where T : IUnit, new() {
            var unit = new T();
            unit.InitByField(data);
            return unit;
        }

        public static ReadOnlyMemory<byte> Jump(ReadOnlyMemory<byte> data) {
            if (data.IsEmpty) {
                return data;
            }
            var mark = BitConverter.ToUInt32(data.Span);
            if ((mark & 3) == 3) {
                var off = mark & 0xfffffffc;
                return data[(int)(mark & 0xfffffffc)..];
            }
            return data;
        }

        public abstract class Object : IUnit {
            public abstract void Init(ReadOnlyMemory<byte> data);
            public void InitByField(ReadOnlyMemory<byte> data) {
                Init(Jump(data));
            }
        }
    }
}
