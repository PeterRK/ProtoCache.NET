# ProtoCache .NET

ProtoCache is a flat binary cache format generated from
[Proto3 schemas](https://protobuf.dev/programming-guides/proto3/). It supports
direct field access without first materializing a complete object graph and
includes compact map support. The portable schema contract is defined by the
[ProtoCache schema specification](https://github.com/peterrk/protocache/blob/014dcd4e81c2a60121b8eefb2eb42c7eaceace90/schema.md).

|  | Protobuf | ProtoCache | FlatBuffers |
|:-------|----:|----:|----:|
| Data Size | 574B | 780B | 1296B |
| Decode + Traverse | 2805ns | 1139ns | 2353ns |

The benchmark is available in
[`ProtoCache.Benchmark`](https://github.com/peterrk/ProtoCache.NET/tree/main/ProtoCache.Benchmark).
Numeric and bytes access is zero-copy. Reading strings and constructing some
object-oriented wrappers may allocate, so the benchmark represents
low-deserialization-overhead access rather than universally zero-allocation
traversal.

## Requirements

- .NET 10 or later.
- `protoc` for Protobuf code generation.
- `protoc-gen-pc.net` from ProtoCache C++ v1.2.1 for typed ProtoCache accessors.

## Installation

```sh
dotnet add package ProtoCache --version 0.1.0
```

## Code generation

ProtoCache 0.1.0 is paired with `protoc-gen-pc.net` from
[ProtoCache C++ v1.2.1](https://github.com/peterrk/protocache/releases/tag/v1.2.1).
Follow the C++ repository [build and install instructions](https://github.com/peterrk/protocache/blob/v1.2.1/README.md#build-and-install),
then invoke the plugin through `protoc`:

```sh
protoc --pc.net_out=generated schema.proto
```

The schema must specify `option csharp_namespace`. Protobuf classes and
ProtoCache accessors must use different C# namespaces because both generators
emit the schema message names. Generate the accessors from a temporary schema
copy that changes only `csharp_namespace`.

## Basic API

```csharp
using Pb = Example.Protobuf;
using Pc = Example.ProtoCache;

Pb.Main message = Pb.Main.Parser.ParseFrom(protobufBytes);
byte[] cache = global::ProtoCache.ProtoCache.Serialize(message);

var root = new Pc.Main(cache);
int count = root.Count;
ReadOnlySpan<byte> data = root.Data;
byte[] ownedData = root.Data.ToArray();
```

`ProtoCache.Serialize(Google.Protobuf.IMessage)` is the supported write path.
Reading uses generated, strongly typed accessors. Numeric fields and bytes are
read directly from the source buffer; call `.ToArray()` when an independently
owned bytes value is required.

## Compatibility

Portable schema rules are defined by the C++ repository
[`schema.md`](https://github.com/peterrk/protocache/blob/014dcd4e81c2a60121b8eefb2eb42c7eaceace90/schema.md).
The binary layout is defined by
[`data-format.md`](https://github.com/peterrk/protocache/blob/014dcd4e81c2a60121b8eefb2eb42c7eaceace90/data-format.md).

The .NET binding targets .NET 10 and provides the binding-specific `_x_` alias
spelling described by the schema document. It does not guarantee Protobuf field
presence, oneof active-case semantics, or deterministic map serialization.
Generated accessors and producers in other languages must use compatible
schemas.

## Reflection

The writer uses the descriptor exposed by `Google.Protobuf.IMessage` internally.
The .NET runtime does not provide schema-reflection or dynamic-message reading
APIs; reading requires generated, strongly typed accessors.

## License

[BSD 3-Clause](https://github.com/peterrk/ProtoCache.NET/blob/main/LICENSE)
