# ProtoCache .NET

Alternative flat binary format for [Protobuf schema](https://protobuf.dev/programming-guides/proto3/). It works like FlatBuffers, but it's usually smaller and supports map. The format is flat, so numeric fields and nested structures can be accessed without full message deserialization. In the current C# implementation, some APIs still allocate when reading strings, byte arrays, or wrapper objects. [A benchmark](ProtoCache.Benchmark) shows that Protobuf has considerable deserialization overhead and significant reflection overhead. FlatBuffers is fast but wastes space. ProtoCache takes a balance between data size and read speed, so it's useful in data caching.

|  | Protobuf | ProtoCache | FlatBuffers |
|:-------|----:|----:|----:|
| Data Size | 574B | 780B | 1296B |
| Decode + Traverse | 2805ns | 1139ns | 2353ns |

The C# version is not fully zero-copy yet, so benchmark results should be interpreted as "low-deserialization-overhead access" rather than literal zero-allocation traversal.

See detail in [C++ version](https://github.com/peterrk/protocache).

## Code Gen
```sh
protoc --pc.net_out=. test.proto
```
A protobuf compiler plugin called `protoc-gen-pc.net` is [available](https://github.com/peterrk/protocache/blob/main/tools/protoc-gen-pc.net.cc) to generate java package. The generated files are short and human friendly. Don't mind to edit them if nessasery.

## Basic APIs
```csharp
var pb = pb.Main.Parser.ParseFrom(raw);
raw = ProtoCache.Serialize(pb);

var root = new pc.Main(raw);
```
Serializing a protobuf message with `ProtoCache.Serialize` is the only way to create protocache binary at present. It is easy to access by wrapping the data with generated code. For numeric fields this is direct flat access; for strings, bytes, and some object-oriented helpers, the current C# API may still allocate.

## Reflection
TODO
