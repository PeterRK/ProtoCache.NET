#!/bin/sh
set -eu

script_dir=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
flatc --binary -o "$script_dir" "$script_dir/test.fbs" "$script_dir/test-fb.json"
mv "$script_dir/test-fb.bin" "$script_dir/test.fb"
