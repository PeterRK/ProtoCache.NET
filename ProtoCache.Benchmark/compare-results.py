#!/usr/bin/env python3
"""Summarize alternating process pairs; negative time/byte changes are improvements."""
import argparse
import json
import statistics
import sys
from pathlib import Path

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('directory', type=Path)
parser.add_argument('--baseline', default='baseline-harness')
parser.add_argument('--candidate', default='candidate')
parser.add_argument('--pairs', type=int, default=6)
parser.add_argument('--output', type=Path)
args = parser.parse_args()


def read(label, pair):
    data = json.loads((args.directory / f'{label}-{pair}.json').read_text())
    return {result['Name']: result['Samples'] for result in data['Results']}


pairs = [(read(args.baseline, i), read(args.candidate, i)) for i in range(1, args.pairs + 1)]
if any(a.keys() != b.keys() for a, b in pairs):
    raise ValueError('Paired workload names differ')

# Warn without dropping observations or selecting pairs based on optimization results.
controls = {'traverse.standard', 'protobuf.decode-traverse', 'compress.standard',
            'decompress.standard', 'lookup.string-hit-miss'}
for number, (a, b) in enumerate(pairs, 1):
    for name in controls & a.keys():
        ratio = statistics.median(s['NanosecondsPerOperation'] for s in b[name]) / statistics.median(
            s['NanosecondsPerOperation'] for s in a[name])
        if abs(ratio - 1) > 0.15:
            print(f'WARNING: pair {number}, unchanged control {name}: {100 * (ratio - 1):+.1f}%; '
                  'interpret timing cautiously (pair retained).', file=sys.stderr)


def describe(groups):
    samples = [sample for group in groups for sample in group]
    operations = sum(s['Operations'] for s in samples)
    return {
        'ns_per_op': statistics.median(statistics.median(s['NanosecondsPerOperation'] for s in group) for group in groups),
        'bytes_per_op': statistics.median(s['BytesPerOperation'] for s in samples),
        'gc_per_1000_ops': {g: 1000 * sum(s[g] for s in samples) / operations for g in ['Gen0', 'Gen1', 'Gen2']},
        'build_failures': sum(s['BuildFailures'] for s in samples),
        'operations': operations,
        'attempts_per_op': (sum(s['AttemptsPerOperation'] * s['Operations'] for s in samples) / operations
                           if samples[0]['AttemptsPerOperation'] is not None else None),
    }


results = []
print('| workload | baseline ns/op | candidate ns/op | paired change (min…max) | B/op change |')
print('|---|---:|---:|---:|---:|')
for name in pairs[0][0]:
    baseline = describe([a[name] for a, _ in pairs])
    candidate = describe([b[name] for _, b in pairs])
    changes = [100 * (statistics.median(s['NanosecondsPerOperation'] for s in b[name]) /
                      statistics.median(s['NanosecondsPerOperation'] for s in a[name]) - 1) for a, b in pairs]
    byte_change = (100 * (candidate['bytes_per_op'] / baseline['bytes_per_op'] - 1)
                   if baseline['bytes_per_op'] else 0)
    change = statistics.median(changes)
    results.append(dict(name=name, baseline=baseline, candidate=candidate,
                        paired_time_change_percent=change, paired_time_changes_percent=changes,
                        byte_change_percent=byte_change))
    print(f"| {name} | {baseline['ns_per_op']:,.1f} | {candidate['ns_per_op']:,.1f} | "
          f"{change:+.1f}% ({min(changes):+.1f}…{max(changes):+.1f}%) | {byte_change:+.1f}% |")
if args.output:
    args.output.write_text(json.dumps(results, indent=2) + '\n')
