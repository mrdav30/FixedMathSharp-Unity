using System;
using Chronicler.Hashing;
using Chronicler.Serialization;
using Chronicler.Timing;
using FixedMathSharp.Chronicler;
using NUnit.Framework;

namespace FixedMathSharp.Unity.Tests
{
    public sealed class FixedChroniclerIntegrationTests
    {
        [Test]
        public void HashExtensions_WriteRawValuesInDocumentedOrder()
        {
            var writer = new ChronicleHashWriter();
            writer.WriteFixed64(Fixed64.FromRaw(-123));
            writer.WriteVector3d(new Vector3d(
                Fixed64.FromRaw(111), Fixed64.FromRaw(-222), Fixed64.FromRaw(333)));

            var expected = new ChronicleHashWriter();
            expected.WriteInt64(-123);
            expected.WriteInt64(111);
            expected.WriteInt64(-222);
            expected.WriteInt64(333);

            Assert.That(writer.ToHash(), Is.EqualTo(expected.ToHash()));
            Assert.That(writer.ToHash(), Is.Not.EqualTo(new ChronicleHashWriter().ToHash()));
        }

        [Test]
        public void ClockJsonRestore_PreservesHashAndContinuesWithTheRecordedStep()
        {
            var clock = new ChronicleClock(FixedChronicleTime.FromFixed64(Fixed64.Quarter));
            clock.Advance();
            var hash = ChronicleHashSerializer.Compute(clock);
            string json = JsonRecordSerializer.Serialize(clock);

            clock.Reset();
            clock.SetStepDuration(FixedChronicleTime.FromFixed64(Fixed64.One));
            JsonRecordSerializer.Populate(clock, json);

            Assert.That(clock.FrameCount, Is.EqualTo(1L));
            Assert.That(ChronicleHashSerializer.Compute(clock), Is.EqualTo(hash));
            clock.Advance();
            Assert.That(FixedChronicleTime.ToFixed64(
                clock.ElapsedTime - ChronicleTimestamp.Zero).m_rawValue, Is.EqualTo(0x80000000L));
        }

        [Test]
        public void WideTimestamps_SubtractBeforeConvertingToFixed64()
        {
            var start = new ChronicleTimestamp(3_155_760_000L, 0);
            var end = start + FixedChronicleTime.FromFixed64(Fixed64.FromRaw(0x40000000));

            Assert.That(FixedChronicleTime.ToFixed64(end - start).m_rawValue,
                Is.EqualTo(0x40000000L));
        }

        [Test]
        public void OutOfRangeDuration_RejectsNarrowingWithoutSaturation()
        {
            var duration = new ChronicleDuration((long)int.MaxValue + 1, 0);

            Assert.That(FixedChronicleTime.TryToFixed64(duration, out var result), Is.False);
            Assert.That(result.m_rawValue, Is.Zero);
            Assert.Throws<OverflowException>(() => FixedChronicleTime.ToFixed64(duration));
        }

        [Test]
        public void FrameCount_UsesTheRepresentedStepDuration()
        {
            Assert.That(FixedChronicleTime.GetFrameCountForDuration(
                Fixed64.One, Fixed64.FromRaw(143165577)), Is.EqualTo(29L));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                FixedChronicleTime.GetFrameCountForDuration(Fixed64.One, Fixed64.Zero));
        }
    }
}
