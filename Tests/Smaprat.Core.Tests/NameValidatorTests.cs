using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Smaprat.Core.Tests
{
    [TestClass]
    public class NameValidatorTests
    {
        private static readonly string[] disallowedNames = new string[]
        {
            "me",
            "admin",
            "administrator",
            "server",
            "host"
        };

        [TestMethod]
        public void EnsureBadNamesAreDisallowed()
        {
            NameValidator validator = new NameValidator();

            foreach (string disallowedName in disallowedNames)
                Assert.IsFalse(validator.IsValid(disallowedName));
        }

        [TestMethod]
        public void EnsureBlankNamesAreDisallowed()
        {
            NameValidator validator = new NameValidator();

            Assert.IsFalse(validator.IsValid(""));
        }
    }
}
