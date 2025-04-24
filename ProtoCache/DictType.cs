// Copyright (c) 2025, Ruan Kunliang.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace ProtoCache {
    public abstract class DictType : IUnit {
        public abstract void Init(DataView data);
        private static readonly PerfectHash empty = new(new byte[4]);
        private DataView body = DataView.Empty;
        protected PerfectHash index = empty;
        private int keyWidth = 0;
        private int valueWidth = 0;

        public int Size => index.Size;

        protected DataView KeyAt(int idx) => body.Forward(idx * (keyWidth + valueWidth));

        protected DataView ValueAt(int idx) => body.Forward(idx * (keyWidth + valueWidth) + keyWidth);

        protected void Init(DataView data, int keyWord, int valueWord) {
            if (!data.IsValid) {
                body = DataView.Empty;
                index = empty;
                keyWidth = keyWord * 4;
                valueWidth = valueWord * 4;
                return;
            }

            index = new PerfectHash(data);
            var bodyOffset = (int)((index.ByteSize + 3) & 0xfffffffc);
            var mark = data.GetUInt32();
            keyWidth = (int)((mark >> 30) & 3) * 4;
            valueWidth = (int)((mark >> 28) & 3) * 4;
            if ((keyWord != 0 && keyWidth != keyWord * 4) || keyWidth == 0
                || (valueWord != 0 && valueWidth != valueWord * 4) || valueWidth == 0) {
                throw new ArgumentException("illegal map");
            }
            body = data.Forward(bodyOffset);
        }
    }
}
