using System;
using NUnit.Framework;

namespace FixedMathSharp.Unity.Tests
{
    public sealed class Fixed64PluginTests
    {
        [Test]
        public void Parse_LoadsBundledDependenciesAndPreservesRawValue()
        {
            Assert.That(Fixed64.Parse("1.25").m_rawValue, Is.EqualTo(5368709120L));
            Assert.That(Fixed64.ParseRaw("5368709120").m_rawValue, Is.EqualTo(5368709120L));
        }

        [Test]
        public void Parse_NullInputPreservesArgumentNullContract()
        {
            Assert.Throws<ArgumentNullException>(() => Fixed64.Parse(null));
            Assert.Throws<ArgumentNullException>(() => Fixed64.ParseRaw(null));
        }
    }
}
