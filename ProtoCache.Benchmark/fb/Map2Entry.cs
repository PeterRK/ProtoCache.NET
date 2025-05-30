// <auto-generated>
//  automatically generated by the FlatBuffers compiler, do not modify
// </auto-generated>

namespace ProtoCache.Tests.fb
{

using global::System;
using global::System.Collections.Generic;
using global::Google.FlatBuffers;

public struct Map2Entry : IFlatbufferObject
{
  private Table __p;
  public ByteBuffer ByteBuffer { get { return __p.bb; } }
  public static void ValidateVersion() { FlatBufferConstants.FLATBUFFERS_25_2_10(); }
  public static Map2Entry GetRootAsMap2Entry(ByteBuffer _bb) { return GetRootAsMap2Entry(_bb, new Map2Entry()); }
  public static Map2Entry GetRootAsMap2Entry(ByteBuffer _bb, Map2Entry obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public void __init(int _i, ByteBuffer _bb) { __p = new Table(_i, _bb); }
  public Map2Entry __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

  public int Key { get { int o = __p.__offset(4); return o != 0 ? __p.bb.GetInt(o + __p.bb_pos) : (int)0; } }
  public global::ProtoCache.Tests.fb.Small? Value { get { int o = __p.__offset(6); return o != 0 ? (global::ProtoCache.Tests.fb.Small?)(new global::ProtoCache.Tests.fb.Small()).__assign(__p.__indirect(o + __p.bb_pos), __p.bb) : null; } }

  public static Offset<global::ProtoCache.Tests.fb.Map2Entry> CreateMap2Entry(FlatBufferBuilder builder,
      int key = 0,
      Offset<global::ProtoCache.Tests.fb.Small> valueOffset = default(Offset<global::ProtoCache.Tests.fb.Small>)) {
    builder.StartTable(2);
    Map2Entry.AddValue(builder, valueOffset);
    Map2Entry.AddKey(builder, key);
    return Map2Entry.EndMap2Entry(builder);
  }

  public static void StartMap2Entry(FlatBufferBuilder builder) { builder.StartTable(2); }
  public static void AddKey(FlatBufferBuilder builder, int key) { builder.AddInt(0, key, 0); }
  public static void AddValue(FlatBufferBuilder builder, Offset<global::ProtoCache.Tests.fb.Small> valueOffset) { builder.AddOffset(1, valueOffset.Value, 0); }
  public static Offset<global::ProtoCache.Tests.fb.Map2Entry> EndMap2Entry(FlatBufferBuilder builder) {
    int o = builder.EndTable();
    return new Offset<global::ProtoCache.Tests.fb.Map2Entry>(o);
  }

  public static VectorOffset CreateSortedVectorOfMap2Entry(FlatBufferBuilder builder, Offset<Map2Entry>[] offsets) {
    Array.Sort(offsets,
      (Offset<Map2Entry> o1, Offset<Map2Entry> o2) =>
        new Map2Entry().__assign(builder.DataBuffer.Length - o1.Value, builder.DataBuffer).Key.CompareTo(new Map2Entry().__assign(builder.DataBuffer.Length - o2.Value, builder.DataBuffer).Key));
    return builder.CreateVectorOfTables(offsets);
  }

  public static Map2Entry? __lookup_by_key(int vectorLocation, int key, ByteBuffer bb) {
    Map2Entry obj_ = new Map2Entry();
    int span = bb.GetInt(vectorLocation - 4);
    int start = 0;
    while (span != 0) {
      int middle = span / 2;
      int tableOffset = Table.__indirect(vectorLocation + 4 * (start + middle), bb);
      obj_.__assign(tableOffset, bb);
      int comp = obj_.Key.CompareTo(key);
      if (comp > 0) {
        span = middle;
      } else if (comp < 0) {
        middle++;
        start += middle;
        span -= middle;
      } else {
        return obj_;
      }
    }
    return null;
  }
}


static public class Map2EntryVerify
{
  static public bool Verify(Google.FlatBuffers.Verifier verifier, uint tablePos)
  {
    return verifier.VerifyTableStart(tablePos)
      && verifier.VerifyField(tablePos, 4 /*Key*/, 4 /*int*/, 4, false)
      && verifier.VerifyTable(tablePos, 6 /*Value*/, global::ProtoCache.Tests.fb.SmallVerify.Verify, false)
      && verifier.VerifyTableEnd(tablePos);
  }
}

}
