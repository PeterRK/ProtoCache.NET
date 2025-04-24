namespace ProtoCache.Tests.pc {

public class Mode {
	public const int MODE_A = 0;
	public const int MODE_B = 1;
	public const int MODE_C = 2;
}

public class Small : global::ProtoCache.IUnit.Object {
	public const int _i32 = 0;
	public const int _flag = 1;
	public const int _str = 3;

	private global::ProtoCache.Message _core_;
	public Small() {}
	public Small(ReadOnlyMemory<byte> data) { Init(data); }
	public bool HasField(int id) { return _core_.HasField(id); }
	public override void Init(ReadOnlyMemory<byte> data) {
		_core_.Init(data);
		str_ = null;
	}

	public int I32 { get { return _core_.GetInt32(_i32); } }
	public bool Flag { get { return _core_.GetBool(_flag); } }
	private string? str_ = null;
	public string Str { get {
		str_ ??= _core_.GetString(_str);
		return str_;
	}}
}

public class Vec2D : global::ProtoCache.ObjectArray<global::ProtoCache.Tests.pc.Vec2D.Vec1D> {
	public class Vec1D : global::ProtoCache.Float32Array {
	}

}

public class ArrMap : global::ProtoCache.StringDict<global::ProtoCache.Tests.pc.ArrMap.Array> {
	public class Array : global::ProtoCache.Float32Array {
	}

}

public class Main : global::ProtoCache.IUnit.Object {
	public const int _i32 = 0;
	public const int _u32 = 1;
	public const int _i64 = 2;
	public const int _u64 = 3;
	public const int _flag = 4;
	public const int _mode = 5;
	public const int _str = 6;
	public const int _data = 7;
	public const int _f32 = 8;
	public const int _f64 = 9;
	public const int _object = 10;
	public const int _i32v = 11;
	public const int _u64v = 12;
	public const int _strv = 13;
	public const int _datav = 14;
	public const int _f32v = 15;
	public const int _f64v = 16;
	public const int _flags = 17;
	public const int _objectv = 18;
	public const int _t_u32 = 19;
	public const int _t_i32 = 20;
	public const int _t_s32 = 21;
	public const int _t_u64 = 22;
	public const int _t_i64 = 23;
	public const int _t_s64 = 24;
	public const int _index = 25;
	public const int _objects = 26;
	public const int _matrix = 27;
	public const int _vector = 28;
	public const int _arrays = 29;
	public const int _modev = 31;

	private global::ProtoCache.Message _core_;
	public Main() {}
	public Main(ReadOnlyMemory<byte> data) { Init(data); }
	public bool HasField(int id) { return _core_.HasField(id); }
	public override void Init(ReadOnlyMemory<byte> data) {
		_core_.Init(data);
		str_ = null;
		data_ = null;
		object_ = null;
		i32v_ = null;
		u64v_ = null;
		strv_ = null;
		datav_ = null;
		f32v_ = null;
		f64v_ = null;
		flags_ = null;
		objectv_ = null;
		index_ = null;
		objects_ = null;
		matrix_ = null;
		vector_ = null;
		arrays_ = null;
		modev_ = null;
	}

	public int I32 { get { return _core_.GetInt32(_i32); } }
	public uint U32 { get { return _core_.GetUInt32(_u32); } }
	public long I64 { get { return _core_.GetInt64(_i64); } }
	public ulong U64 { get { return _core_.GetUInt64(_u64); } }
	public bool Flag { get { return _core_.GetBool(_flag); } }
	public int Mode { get { return _core_.GetInt32(_mode); } }
	private string? str_ = null;
	public string Str { get {
		str_ ??= _core_.GetString(_str);
		return str_;
	}}
	private byte[]? data_ = null;
	public byte[] Data { get {
		data_ ??= _core_.GetBytes(_data);
		return data_;
	}}
	public float F32 { get { return _core_.GetFloat32(_f32); } }
	public double F64 { get { return _core_.GetFloat64(_f64); } }
	private global::ProtoCache.Tests.pc.Small? object_ = null;
	public global::ProtoCache.Tests.pc.Small Object { get {
		object_ ??= _core_.GetObject<global::ProtoCache.Tests.pc.Small>(_object);
		return object_;
	}}
	private global::ProtoCache.Int32Array? i32v_ = null;
	public global::ProtoCache.Int32Array I32V { get {
		i32v_ ??= _core_.GetObject<global::ProtoCache.Int32Array>(_i32v);
		return i32v_;
	}}
	private global::ProtoCache.UInt64Array? u64v_ = null;
	public global::ProtoCache.UInt64Array U64V { get {
		u64v_ ??= _core_.GetObject<global::ProtoCache.UInt64Array>(_u64v);
		return u64v_;
	}}
	private global::ProtoCache.StringArray? strv_ = null;
	public global::ProtoCache.StringArray Strv { get {
		strv_ ??= _core_.GetObject<global::ProtoCache.StringArray>(_strv);
		return strv_;
	}}
	private global::ProtoCache.BytesArray? datav_ = null;
	public global::ProtoCache.BytesArray Datav { get {
		datav_ ??= _core_.GetObject<global::ProtoCache.BytesArray>(_datav);
		return datav_;
	}}
	private global::ProtoCache.Float32Array? f32v_ = null;
	public global::ProtoCache.Float32Array F32V { get {
		f32v_ ??= _core_.GetObject<global::ProtoCache.Float32Array>(_f32v);
		return f32v_;
	}}
	private global::ProtoCache.Float64Array? f64v_ = null;
	public global::ProtoCache.Float64Array F64V { get {
		f64v_ ??= _core_.GetObject<global::ProtoCache.Float64Array>(_f64v);
		return f64v_;
	}}
	private global::ProtoCache.BoolArray? flags_ = null;
	public global::ProtoCache.BoolArray Flags { get {
		flags_ ??= _core_.GetObject<global::ProtoCache.BoolArray>(_flags);
		return flags_;
	}}
	private global::ProtoCache.ObjectArray<global::ProtoCache.Tests.pc.Small>? objectv_ = null;
	public global::ProtoCache.ObjectArray<global::ProtoCache.Tests.pc.Small> Objectv { get {
		objectv_ ??= _core_.GetObject<global::ProtoCache.ObjectArray<global::ProtoCache.Tests.pc.Small>>(_objectv);
		return objectv_;
	}}
	public uint TU32 { get { return _core_.GetUInt32(_t_u32); } }
	public int TI32 { get { return _core_.GetInt32(_t_i32); } }
	public int TS32 { get { return _core_.GetInt32(_t_s32); } }
	public ulong TU64 { get { return _core_.GetUInt64(_t_u64); } }
	public long TI64 { get { return _core_.GetInt64(_t_i64); } }
	public long TS64 { get { return _core_.GetInt64(_t_s64); } }
	private global::ProtoCache.StringDict<global::ProtoCache.Int32Value>? index_ = null;
	public global::ProtoCache.StringDict<global::ProtoCache.Int32Value> Index { get {
		index_ ??= _core_.GetObject<global::ProtoCache.StringDict<global::ProtoCache.Int32Value>>(_index);
		return index_;
	}}
	private global::ProtoCache.Int32Dict<global::ProtoCache.Tests.pc.Small>? objects_ = null;
	public global::ProtoCache.Int32Dict<global::ProtoCache.Tests.pc.Small> Objects { get {
		objects_ ??= _core_.GetObject<global::ProtoCache.Int32Dict<global::ProtoCache.Tests.pc.Small>>(_objects);
		return objects_;
	}}
	private global::ProtoCache.Tests.pc.Vec2D? matrix_ = null;
	public global::ProtoCache.Tests.pc.Vec2D Matrix { get {
		matrix_ ??= _core_.GetObject<global::ProtoCache.Tests.pc.Vec2D>(_matrix);
		return matrix_;
	}}
	private global::ProtoCache.ObjectArray<global::ProtoCache.Tests.pc.ArrMap>? vector_ = null;
	public global::ProtoCache.ObjectArray<global::ProtoCache.Tests.pc.ArrMap> Vector { get {
		vector_ ??= _core_.GetObject<global::ProtoCache.ObjectArray<global::ProtoCache.Tests.pc.ArrMap>>(_vector);
		return vector_;
	}}
	private global::ProtoCache.Tests.pc.ArrMap? arrays_ = null;
	public global::ProtoCache.Tests.pc.ArrMap Arrays { get {
		arrays_ ??= _core_.GetObject<global::ProtoCache.Tests.pc.ArrMap>(_arrays);
		return arrays_;
	}}
	private global::ProtoCache.Int32Array? modev_ = null;
	public global::ProtoCache.Int32Array Modev { get {
		modev_ ??= _core_.GetObject<global::ProtoCache.Int32Array>(_modev);
		return modev_;
	}}
}

public class CyclicA : global::ProtoCache.IUnit.Object {
	public const int _value = 0;
	public const int _cyclic = 1;

	private global::ProtoCache.Message _core_;
	public CyclicA() {}
	public CyclicA(ReadOnlyMemory<byte> data) { Init(data); }
	public bool HasField(int id) { return _core_.HasField(id); }
	public override void Init(ReadOnlyMemory<byte> data) {
		_core_.Init(data);
		cyclic_ = null;
	}

	public int Value { get { return _core_.GetInt32(_value); } }
	private global::ProtoCache.Tests.pc.CyclicB? cyclic_ = null;
	public global::ProtoCache.Tests.pc.CyclicB Cyclic { get {
		cyclic_ ??= _core_.GetObject<global::ProtoCache.Tests.pc.CyclicB>(_cyclic);
		return cyclic_;
	}}
}

public class CyclicB : global::ProtoCache.IUnit.Object {
	public const int _value = 0;
	public const int _cyclic = 1;

	private global::ProtoCache.Message _core_;
	public CyclicB() {}
	public CyclicB(ReadOnlyMemory<byte> data) { Init(data); }
	public bool HasField(int id) { return _core_.HasField(id); }
	public override void Init(ReadOnlyMemory<byte> data) {
		_core_.Init(data);
		cyclic_ = null;
	}

	public int Value { get { return _core_.GetInt32(_value); } }
	private global::ProtoCache.Tests.pc.CyclicA? cyclic_ = null;
	public global::ProtoCache.Tests.pc.CyclicA Cyclic { get {
		cyclic_ ??= _core_.GetObject<global::ProtoCache.Tests.pc.CyclicA>(_cyclic);
		return cyclic_;
	}}
}

public class Deprecated : global::ProtoCache.IUnit.Object {
        public class Valid : global::ProtoCache.IUnit.Object {
		public const int _val = 0;

		private global::ProtoCache.Message _core_;
		public Valid() {}
		public Valid(ReadOnlyMemory<byte> data) { Init(data); }
		public bool HasField(int id) { return _core_.HasField(id); }
		public override void Init(ReadOnlyMemory<byte> data) {
			_core_.Init(data);
		}

		public int Val { get { return _core_.GetInt32(_val); } }
	}

}

}
