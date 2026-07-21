namespace ProtoCache.Tests.pc {

public class Mode {
	public const int MODE_A = 0;
	public const int MODE_B = 1;
	public const int MODE_C = 2;
}

public class Small : global::ProtoCache.IUnit {
	public const ushort _i32 = 0;
	public const ushort _flag = 1;
	public const ushort _str = 3;

	private global::ProtoCache.Message _core_;
	public Small() {}
	public Small(byte[] data) => Init(new global::ProtoCache.DataView(data));
	public bool HasField(ushort id) => _core_.HasField(id);
	public void Init(global::ProtoCache.DataView data) {
		_core_.Init(data);
		str_ = null;
	}

	public int I32 => _core_.GetInt32(_i32);
	public bool Flag => _core_.GetBool(_flag);
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

public class ArrMap : global::ProtoCache.StringDict.ObjectValue<global::ProtoCache.Tests.pc.ArrMap.Array> {
	public class Array : global::ProtoCache.Float32Array {
	}

}

public class Main : global::ProtoCache.IUnit {
	public const ushort _i32 = 0;
	public const ushort _u32 = 1;
	public const ushort _i64 = 2;
	public const ushort _u64 = 3;
	public const ushort _flag = 4;
	public const ushort _mode = 5;
	public const ushort _str = 6;
	public const ushort _data = 7;
	public const ushort _f32 = 8;
	public const ushort _f64 = 9;
	public const ushort _object = 10;
	public const ushort _i32v = 11;
	public const ushort _u64v = 12;
	public const ushort _strv = 13;
	public const ushort _datav = 14;
	public const ushort _f32v = 15;
	public const ushort _f64v = 16;
	public const ushort _flags = 17;
	public const ushort _objectv = 18;
	public const ushort _t_u32 = 19;
	public const ushort _t_i32 = 20;
	public const ushort _t_s32 = 21;
	public const ushort _t_u64 = 22;
	public const ushort _t_i64 = 23;
	public const ushort _t_s64 = 24;
	public const ushort _index = 25;
	public const ushort _objects = 26;
	public const ushort _matrix = 27;
	public const ushort _vector = 28;
	public const ushort _arrays = 29;
	public const ushort _modev = 31;

	private global::ProtoCache.Message _core_;
	public Main() {}
	public Main(byte[] data) => Init(new global::ProtoCache.DataView(data));
	public bool HasField(ushort id) => _core_.HasField(id);
	public void Init(global::ProtoCache.DataView data) {
		_core_.Init(data);
		str_ = null;
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

	public int I32 => _core_.GetInt32(_i32);
	public uint U32 => _core_.GetUInt32(_u32);
	public long I64 => _core_.GetInt64(_i64);
	public ulong U64 => _core_.GetUInt64(_u64);
	public bool Flag => _core_.GetBool(_flag);
	public int Mode => _core_.GetInt32(_mode);
	private string? str_ = null;
	public string Str { get {
		str_ ??= _core_.GetString(_str);
		return str_;
	}}
	public global::System.ReadOnlySpan<byte> Data => _core_.GetBytes(_data);
	public float F32 => _core_.GetFloat32(_f32);
	public double F64 => _core_.GetFloat64(_f64);
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
	public uint TU32 => _core_.GetUInt32(_t_u32);
	public int TI32 => _core_.GetInt32(_t_i32);
	public int TS32 => _core_.GetInt32(_t_s32);
	public ulong TU64 => _core_.GetUInt64(_t_u64);
	public long TI64 => _core_.GetInt64(_t_i64);
	public long TS64 => _core_.GetInt64(_t_s64);
	private global::ProtoCache.StringDict.Int32Value? index_ = null;
	public global::ProtoCache.StringDict.Int32Value Index { get {
		index_ ??= _core_.GetObject<global::ProtoCache.StringDict.Int32Value>(_index);
		return index_;
	}}
	private global::ProtoCache.Int32Dict.ObjectValue<global::ProtoCache.Tests.pc.Small>? objects_ = null;
	public global::ProtoCache.Int32Dict.ObjectValue<global::ProtoCache.Tests.pc.Small> Objects { get {
		objects_ ??= _core_.GetObject<global::ProtoCache.Int32Dict.ObjectValue<global::ProtoCache.Tests.pc.Small>>(_objects);
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

public class MapCases : global::ProtoCache.IUnit {
	public const ushort _string_bool = 0;
	public const ushort _int32_int32 = 1;
	public const ushort _uint32_uint32 = 2;
	public const ushort _int64_int64 = 3;
	public const ushort _uint64_uint64 = 4;
	public const ushort _string_float = 5;
	public const ushort _string_double = 6;
	public const ushort _string_bytes = 7;
	public const ushort _string_string = 8;
	public const ushort _string_message = 9;
	public const ushort _string_enum = 10;

	private global::ProtoCache.Message _core_;
	public MapCases() {}
	public MapCases(byte[] data) => Init(new global::ProtoCache.DataView(data));
	public bool HasField(ushort id) => _core_.HasField(id);
	public void Init(global::ProtoCache.DataView data) {
		_core_.Init(data);
		string_bool_ = null;
		int32_int32_ = null;
		uint32_uint32_ = null;
		int64_int64_ = null;
		uint64_uint64_ = null;
		string_float_ = null;
		string_double_ = null;
		string_bytes_ = null;
		string_string_ = null;
		string_message_ = null;
		string_enum_ = null;
	}

	private global::ProtoCache.StringDict.BoolValue? string_bool_ = null;
	public global::ProtoCache.StringDict.BoolValue StringBool { get {
		string_bool_ ??= _core_.GetObject<global::ProtoCache.StringDict.BoolValue>(_string_bool);
		return string_bool_;
	}}
	private global::ProtoCache.Int32Dict.Int32Value? int32_int32_ = null;
	public global::ProtoCache.Int32Dict.Int32Value Int32Int32 { get {
		int32_int32_ ??= _core_.GetObject<global::ProtoCache.Int32Dict.Int32Value>(_int32_int32);
		return int32_int32_;
	}}
	private global::ProtoCache.UInt32Dict.UInt32Value? uint32_uint32_ = null;
	public global::ProtoCache.UInt32Dict.UInt32Value Uint32Uint32 { get {
		uint32_uint32_ ??= _core_.GetObject<global::ProtoCache.UInt32Dict.UInt32Value>(_uint32_uint32);
		return uint32_uint32_;
	}}
	private global::ProtoCache.Int64Dict.Int64Value? int64_int64_ = null;
	public global::ProtoCache.Int64Dict.Int64Value Int64Int64 { get {
		int64_int64_ ??= _core_.GetObject<global::ProtoCache.Int64Dict.Int64Value>(_int64_int64);
		return int64_int64_;
	}}
	private global::ProtoCache.UInt64Dict.UInt64Value? uint64_uint64_ = null;
	public global::ProtoCache.UInt64Dict.UInt64Value Uint64Uint64 { get {
		uint64_uint64_ ??= _core_.GetObject<global::ProtoCache.UInt64Dict.UInt64Value>(_uint64_uint64);
		return uint64_uint64_;
	}}
	private global::ProtoCache.StringDict.Float32Value? string_float_ = null;
	public global::ProtoCache.StringDict.Float32Value StringFloat { get {
		string_float_ ??= _core_.GetObject<global::ProtoCache.StringDict.Float32Value>(_string_float);
		return string_float_;
	}}
	private global::ProtoCache.StringDict.Float64Value? string_double_ = null;
	public global::ProtoCache.StringDict.Float64Value StringDouble { get {
		string_double_ ??= _core_.GetObject<global::ProtoCache.StringDict.Float64Value>(_string_double);
		return string_double_;
	}}
	private global::ProtoCache.StringDict.BytesValue? string_bytes_ = null;
	public global::ProtoCache.StringDict.BytesValue StringBytes { get {
		string_bytes_ ??= _core_.GetObject<global::ProtoCache.StringDict.BytesValue>(_string_bytes);
		return string_bytes_;
	}}
	private global::ProtoCache.StringDict.StringValue? string_string_ = null;
	public global::ProtoCache.StringDict.StringValue StringString { get {
		string_string_ ??= _core_.GetObject<global::ProtoCache.StringDict.StringValue>(_string_string);
		return string_string_;
	}}
	private global::ProtoCache.StringDict.ObjectValue<global::ProtoCache.Tests.pc.Small>? string_message_ = null;
	public global::ProtoCache.StringDict.ObjectValue<global::ProtoCache.Tests.pc.Small> StringMessage { get {
		string_message_ ??= _core_.GetObject<global::ProtoCache.StringDict.ObjectValue<global::ProtoCache.Tests.pc.Small>>(_string_message);
		return string_message_;
	}}
	private global::ProtoCache.StringDict.Int32Value? string_enum_ = null;
	public global::ProtoCache.StringDict.Int32Value StringEnum { get {
		string_enum_ ??= _core_.GetObject<global::ProtoCache.StringDict.Int32Value>(_string_enum);
		return string_enum_;
	}}
}

public class CyclicA : global::ProtoCache.IUnit {
	public const ushort _value = 0;
	public const ushort _cyclic = 1;

	private global::ProtoCache.Message _core_;
	public CyclicA() {}
	public CyclicA(byte[] data) => Init(new global::ProtoCache.DataView(data));
	public bool HasField(ushort id) => _core_.HasField(id);
	public void Init(global::ProtoCache.DataView data) {
		_core_.Init(data);
		cyclic_ = null;
	}

	public int Value => _core_.GetInt32(_value);
	private global::ProtoCache.Tests.pc.CyclicB? cyclic_ = null;
	public global::ProtoCache.Tests.pc.CyclicB Cyclic { get {
		cyclic_ ??= _core_.GetObject<global::ProtoCache.Tests.pc.CyclicB>(_cyclic);
		return cyclic_;
	}}
}

public class CyclicB : global::ProtoCache.IUnit {
	public const ushort _value = 0;
	public const ushort _cyclic = 1;

	private global::ProtoCache.Message _core_;
	public CyclicB() {}
	public CyclicB(byte[] data) => Init(new global::ProtoCache.DataView(data));
	public bool HasField(ushort id) => _core_.HasField(id);
	public void Init(global::ProtoCache.DataView data) {
		_core_.Init(data);
		cyclic_ = null;
	}

	public int Value => _core_.GetInt32(_value);
	private global::ProtoCache.Tests.pc.CyclicA? cyclic_ = null;
	public global::ProtoCache.Tests.pc.CyclicA Cyclic { get {
		cyclic_ ??= _core_.GetObject<global::ProtoCache.Tests.pc.CyclicA>(_cyclic);
		return cyclic_;
	}}
}

public class Deprecated : global::ProtoCache.IUnit {
	public class Valid : global::ProtoCache.IUnit {
		public const ushort _val = 0;

		private global::ProtoCache.Message _core_;
		public Valid() {}
		public Valid(byte[] data) => Init(new global::ProtoCache.DataView(data));
		public bool HasField(ushort id) => _core_.HasField(id);
		public void Init(global::ProtoCache.DataView data) {
			_core_.Init(data);
		}

		public int Val => _core_.GetInt32(_val);
	}

	public void Init(DataView data) => throw new NotImplementedException();
}

}
