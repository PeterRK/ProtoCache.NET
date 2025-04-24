namespace ProtoCache.Tests.pc {
    public class Small : global::ProtoCache.IUnit.Object {
        public const int _i32 = 0;
        public const int _flag = 1;
        public const int _str = 3;

        private Message _core_;
        public Small() { }
        public Small(ReadOnlyMemory<byte> data) { Init(data); }
        public bool HasField(int id) { return _core_.HasField(id); }
        public override void Init(ReadOnlyMemory<byte> data) {
            _core_.Init(data);
            str_ = null;
        }

        public int I32 { get { return _core_.GetInt32(_i32); } }
        public bool Flag { get { return _core_.GetBool(_flag); } }
        private string? str_ = null;
        public string Str {
            get {
                str_ ??= _core_.GetString(_str);
                return str_;
            }
        }
    }
}
