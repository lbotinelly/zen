using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Encryption()
        {
            var probeValue = "test";

            Assert.AreNotEqual(Zen.Base.Current.Encryption.Encrypt(probeValue), probeValue);
        }
    }
}