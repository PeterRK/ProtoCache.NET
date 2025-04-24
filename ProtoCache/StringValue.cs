// Copyright (c) 2025, Ruan Kunliang.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

using System.Text;

namespace ProtoCache {
    public struct StringValue : IUnit {
        private string value;

        public readonly string Value => value;

        public StringValue() {
            value = "";
        }

        public static string Extract(ReadOnlyMemory<byte> data) {
            var view = BytesValue.ExtractRaw(data);
            return Encoding.UTF8.GetString(view.Span);
        }

        public void Init(ReadOnlyMemory<byte> data) {
            if (data.IsEmpty) {
                value = "";
                return;
            }
            value = Extract(data);
        }

        public void InitByField(ReadOnlyMemory<byte> data) {
            Init(IUnit.Jump(data));
        }
    }
}
