namespace ProtoCache.Tests.pc {

public class BoolAlias : global::ProtoCache.BoolArray {
}

public class LongAlias : global::ProtoCache.Int64Array {
}

public class LongMapAlias : global::ProtoCache.Int64Dict.Int64Value {
}

public class AliasHolder : global::ProtoCache.IUnit {
	public const ushort _flags = 0;
	public const ushort _rows = 1;
	public const ushort _entries = 2;
	public const ushort _longs = 3;
	public const ushort _long_map = 4;
	public const ushort _long_rows = 5;
	public const ushort _map_rows = 6;

	private global::ProtoCache.Message _core_;
	public AliasHolder() {}
	public AliasHolder(byte[] data) => Init(new global::ProtoCache.DataView(data));
	public bool HasField(ushort id) => _core_.HasField(id);
	public void Init(global::ProtoCache.DataView data) {
		_core_.Init(data);
		flags_ = null;
		rows_ = null;
		entries_ = null;
		longs_ = null;
		long_map_ = null;
		long_rows_ = null;
		map_rows_ = null;
	}

	private global::ProtoCache.Tests.pc.BoolAlias? flags_ = null;
	public global::ProtoCache.Tests.pc.BoolAlias Flags { get {
		flags_ ??= _core_.GetObject<global::ProtoCache.Tests.pc.BoolAlias>(_flags);
		return flags_;
	}}
	private global::ProtoCache.ObjectArray<global::ProtoCache.Tests.pc.BoolAlias>? rows_ = null;
	public global::ProtoCache.ObjectArray<global::ProtoCache.Tests.pc.BoolAlias> Rows { get {
		rows_ ??= _core_.GetObject<global::ProtoCache.ObjectArray<global::ProtoCache.Tests.pc.BoolAlias>>(_rows);
		return rows_;
	}}
	private global::ProtoCache.Int32Dict.ObjectValue<global::ProtoCache.Tests.pc.BoolAlias>? entries_ = null;
	public global::ProtoCache.Int32Dict.ObjectValue<global::ProtoCache.Tests.pc.BoolAlias> Entries { get {
		entries_ ??= _core_.GetObject<global::ProtoCache.Int32Dict.ObjectValue<global::ProtoCache.Tests.pc.BoolAlias>>(_entries);
		return entries_;
	}}
	private global::ProtoCache.Tests.pc.LongAlias? longs_ = null;
	public global::ProtoCache.Tests.pc.LongAlias Longs { get {
		longs_ ??= _core_.GetObject<global::ProtoCache.Tests.pc.LongAlias>(_longs);
		return longs_;
	}}
	private global::ProtoCache.Tests.pc.LongMapAlias? long_map_ = null;
	public global::ProtoCache.Tests.pc.LongMapAlias LongMap { get {
		long_map_ ??= _core_.GetObject<global::ProtoCache.Tests.pc.LongMapAlias>(_long_map);
		return long_map_;
	}}
	private global::ProtoCache.ObjectArray<global::ProtoCache.Tests.pc.LongAlias>? long_rows_ = null;
	public global::ProtoCache.ObjectArray<global::ProtoCache.Tests.pc.LongAlias> LongRows { get {
		long_rows_ ??= _core_.GetObject<global::ProtoCache.ObjectArray<global::ProtoCache.Tests.pc.LongAlias>>(_long_rows);
		return long_rows_;
	}}
	private global::ProtoCache.ObjectArray<global::ProtoCache.Tests.pc.LongMapAlias>? map_rows_ = null;
	public global::ProtoCache.ObjectArray<global::ProtoCache.Tests.pc.LongMapAlias> MapRows { get {
		map_rows_ ??= _core_.GetObject<global::ProtoCache.ObjectArray<global::ProtoCache.Tests.pc.LongMapAlias>>(_map_rows);
		return map_rows_;
	}}
}

}
