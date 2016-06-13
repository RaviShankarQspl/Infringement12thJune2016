using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using InfringementWeb.Helpers;
using NUnit.Framework;

namespace InfrigementWebTests
{
    [TestFixture]
    public class UnitTest1
    {
        [Test]
        public void ZeroMonth()
        {
            var result = InfringementNumberGenerator
                .Generate(new DateTime(2015, 10, 20, 0, 0, 1), "1");

            NUnit.Framework.Assert.AreEqual("HCM0001", result);
        }

        [Test]
        public void NotZeroMonth()
        {
            var result = InfringementNumberGenerator
                .Generate(new DateTime(2015, 10, 20, 0, 50, 1), "1");

            NUnit.Framework.Assert.AreEqual("HCM0501", result);
        }
    }
}
