# ProtoCache .NET

Alternative flat binary format for [Protobuf schema](https://protobuf.dev/programming-guides/proto3/). It' works like FlatBuffers, but it's usually smaller and surpports map. Flat means no deserialization overhead. [A benchmark](ProtoCache.Benchmark) shows the Protobuf has considerable deserialization overhead and significant reflection overhead. FlatBuffers is fast but wastes space. ProtoCache takes balance of data size and read speed, so it's useful in data caching.

|  | Protobuf | ProtoCache | FlatBuffers |
|:-------|----:|----:|----:|
| Data Size | 574B | 780B | 1296B |
| Decode + Traverse | 2805ns | 1139ns | 2353ns |

Without zero-copy technique, the C# version is slow. 

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
Serializing a protobuf message with `ProtoCache.Serialize` is the only way to create protocache binary at present. It's easy to access by wrapping the data with generated code.

## Reflection
TODO