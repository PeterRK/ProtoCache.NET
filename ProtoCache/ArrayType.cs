// Copyright (c) 2025, Ruan Kunliang.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace ProtoCache {
    public abstract class ArrayType : IUnit {
        public abstract void Init(DataView data);
        private DataView body = DataView.Empty;
        private int size = 0;
        private int width = 0;

        public int Size => size;

        protected DataView At(int idx) => body.Forward(idx*width);

        protected void Init(DataView data, int word) {
            if (!data.IsValid) {
                size = 0;
                width = word * 4;
                body = DataView.Empty;
                return;
            }
            var mark = data.GetUInt32();
            if ((word != 0 && (mark & 3) != word) || (mark & 3) == 0) {
                throw new ArgumentException("illegal array");
            }
            size = (int)(mark >> 2);
            width = (int)(mark & 3) * 4;
            body = data.Forward(4);
        }
    }
}
