# 性能评估

默认入口保留 Protobuf / ProtoCache / FlatBuffers 读取基准。运行前执行
`bash ProtoCache.Benchmark/generate-fixtures.sh`（需要 flatc），然后做 Release 构建。
启动时比较三种格式的遍历结果，并检查行长为 0、1、4 的矩阵；Protobuf bytes 直接使用 Span。

## A/B 复现

将基线和候选源码分别准备在临时目录，使用相同 Benchmark 源码、输入、SDK 和编译配置。
本轮基线为 `48df8f5249530c57834dd61ade6a70a30e3a4709`。
两组均须包含生成的 test.fb；Docker 中无需安装 flatc。

```bash
# 在准备好 SDK 的环境中执行，结果放在仓库外。
DOTNET_TieredCompilation=0 dotnet ProtoCache.Benchmark/bin/Release/net10.0/ProtoCache.Benchmark.dll \
  --evaluate --label candidate-1 --output /tmp/perf/candidate-1.json \
  --cache-directory /tmp/perf/fixtures --warmup-ms 1500 --sample-ms 500 --samples 3

# 六轮结束后统计；基线输出命名为 baseline-harness-N.json。
python3 ProtoCache.Benchmark/compare-results.py /tmp/perf --output /tmp/perf/summary.json
```

使用同一 CPU 串行运行，顺序 AB/BA 交替，每版六次独立进程。Docker 可用 `--cpuset-cpus=2`
固定 CPU（按机器调整）。基线先生成共享缓存，候选读取相同字节，避免随机 map 布局影响读取对照。
改变输入时使用新缓存目录。可用 `--filter serialize.standard,hash.build.100000` 选择场景。
统计脚本支持 `--baseline`、`--candidate` 和 `--pairs` 指定其他标签及配对数。

- 输入准备、构建、JSON 处理及正确性校验不计时；预热和采样走同一循环。
- 预热后、采样系列前做一次完整 GC，样本间不强制回收。记录 ns/op、当前线程 B/op、Gen0/1/2，
  B/op 不代表进程峰值内存。每进程取样本中位数，再计算配对变化；范围不是置信区间。
- 未改动的读取、压缩、Protobuf 场景作为噪声对照，不丢弃慢样本或异常配对。
- 评估工具记录 Build 预算耗尽，最多额外重试 100 次，成本计入成功操作。
  `BuildFailures` 是预算耗尽数；独立 hash 场景的 `AttemptsPerOperation` 统计内部尝试。
  这仅用于完成性能采样，生产失败率必须直接调用 Build，不能使用测试层重试。
- 18 个场景覆盖标准写入/往返/遍历、压缩/解压、Protobuf、UTF-8/string/bytes、
  1024/4096/65535/65536/100000 项整数 map、4096 项字符串 map、字符串查询，以及
  24/256/4096/100000 项独立 hash。成功索引检查全部槽位，map 检查全部值。

## 本轮结论（2026-09-08）

环境：Intel Core Ultra 7 155H，Docker SDK 10.0.302 / runtime 10.0.10，CPU 2，
workstation GC，`DOTNET_TieredCompilation=0`。镜像固定为
`mcr.microsoft.com/dotnet/sdk@sha256:ed034a8bf0b24ded0cbbac07e17825d8e9ebfe21e308191d0f7421eaf5ad4664`。
本轮一次性数据和探针已清理；以下为实测摘要，重新采样时应自行保存原始输出。

| 场景 | 原 B/op → 优化 B/op | 分配变化 |
|---|---:|---:|
| 标准消息写入 | 19,832 → 18,528 | −6.6% |
| UTF-8/string/bytes 写入 | 51,632 → 43,440 | −15.9% |
| 4096 项整数 map 写入 | 1,124,832 → 977,448 | −13.1% |
| 100000 项整数 map 写入 | 约 27.45 MB → 23.85 MB | −13.1% |
| 4096 项 hash 构建 | 333,984 → 186,600 | −44.1% |
| 100000 项 hash 构建 | 约 8.15 MB → 4.56 MB | −44.1% |

布局对照保留旧 40/16 次预算，隔离布局收益。100000 项 hash 六轮配对耗时中位数降低 29.0%，
范围降低 22.8%～56.7%；其他场景及未改动对照存在较大宿主机噪声，小幅耗时差异没有结论。
整块顶点数组曾使 4096 项 map 变慢并引入 Gen2 回收，因此中小索引采用每块 2048 条边，
更大索引采用单块。最终 4096 项 map 两版 Gen2 均为 0；大型 hash 的 Gen2 次数仍可能增加。

统一 40 次预算另做三轮配对：256 项 hash 耗时中位数增加 2.3%，100000 项 hash 增加 1.0%，
100000 项完整 map 增加 10.7%（范围 9.6%～11.9%），分配基本不变。
这组大 map 回退仍需在空闲机器上复核，不能认定最终版本所有场景都更快。

### 构建失败率

最终所有索引位宽统一最多尝试 **40 次**，成功即返回。原实现在 256 项时由 40 次降到 16 次，
导致失败率突增；此次不更换 PRNG、Hash128 或数据格式。
直接测试 C++ `5ed4a80`、Java `9708452`、C# `48df8f5`，256 项各编码各构建 100000 次，
三版失败率均约 0.24%～0.27%。输入为 0…n−1 的 8 字节 little-endian 整数及 ASCII 十进制字符串。

最终 40 次版本的 256 项两种编码共 2000000 次出现 1 次预算耗尽；24 项仍保留原预算，
共 200000 次出现 21 次耗尽。所有成功索引逐键检查范围和唯一性，均正确。
三语言与最终 C# 的 582 个固定 Hash128 向量、131072 次 Graph.Init/Tear 成败结果也逐项一致。
复测可复用各项目已有 key source，循环调用公开 Build，逐次统计失败并检查成功结果，不额外重试。
这些是固定输入集的观测，40 次不保证所有有效输入必定成功。

日常测试用重复 key 强制构图失败，在 24/256/65536 项分别断言恰好尝试 40 次，避免随机压力断言。
最终 Docker Release 构建、83 项测试、打包及默认 Benchmark 通过；原有 15 条编译警告仍在。
