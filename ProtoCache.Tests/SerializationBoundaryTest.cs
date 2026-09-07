// Copyright (c) 2026, Ruan Kunliang.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
using Google.Protobuf;

namespace ProtoCache.Tests;

public class SerializationBoundaryTest {
    [TestCase(0)]
    [TestCase(1)]
    [TestCase(3)]
    [TestCase(4)]
    [TestCase(7)]
    [TestCase(8)]
    [TestCase(11)]
    [TestCase(12)]
    [TestCase(31)]
    [TestCase(32)]
    [TestCase(4095)]
    [TestCase(4096)]
    [TestCase(524287)]
    [TestCase(524288)]
    public void StringAndBytesLengthBoundaryRoundTrip(int length) {
        var bytes = Enumerable.Range(0, length).Select(i => (byte)(i * 17)).ToArray();
        var message = new pb.Main { Str = new string('x', length), Data = ByteString.CopyFrom(bytes) };
        message.Strv.Add(message.Str);
        message.Strv.Add("汉字🙂");
        message.Datav.Add(message.Data);
        message.Datav.Add(ByteString.Empty);
        var root = new pc.Main(ProtoCache.Serialize(message));
        Assert.That(root.Str, Is.EqualTo(message.Str));
        Assert.That(root.Data.SequenceEqual(bytes), Is.True);
        Assert.That(root.Strv.Size, Is.EqualTo(2));
        Assert.That(root.Strv.Get(0), Is.EqualTo(message.Str));
        Assert.That(root.Strv.Get(1), Is.EqualTo("汉字🙂"));
        Assert.That(root.Datav.Size, Is.EqualTo(2));
        Assert.That(root.Datav.Get(0).SequenceEqual(bytes), Is.True);
        Assert.That(root.Datav.Get(1).IsEmpty, Is.True);
    }
}
