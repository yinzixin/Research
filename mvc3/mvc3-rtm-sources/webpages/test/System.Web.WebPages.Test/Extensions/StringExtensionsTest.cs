namespace System.Web.WebPages.Test {
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Web.WebPages;

    [TestClass]
    public class StringExtensionsTest {

        [TestMethod]
        public void IsIntTests() {
            Assert.IsFalse("1.3".IsInt());
            Assert.IsFalse(".13".IsInt());
            Assert.IsFalse("0.0".IsInt());
            Assert.IsFalse("12345678900123456".IsInt());
            Assert.IsFalse("gooblygook".IsInt());
            Assert.IsTrue("0".IsInt());
            Assert.IsTrue("123456".IsInt());
            Assert.IsTrue(Int32.MaxValue.ToString().IsInt());
            Assert.IsTrue(Int32.MinValue.ToString().IsInt());
            Assert.IsFalse(((string)null).IsInt());
        }

        [TestMethod]
        public void AsIntBasicTests() {
            Assert.AreEqual(-123, "-123".AsInt());
            Assert.AreEqual(12345, "12345".AsInt());
            Assert.AreEqual(0, "0".AsInt());
        }

        [TestMethod]
        public void AsIntDefaultTests() {
            // Illegal values default to 0
            Assert.AreEqual(0, "-100000000000000000000000".AsInt());

            // Illegal values default to 0
            Assert.AreEqual(0, "adlfkj".AsInt());

            Assert.AreEqual(-1, "adlfkj".AsInt(-1));
            Assert.AreEqual(-1, "-100000000000000000000000".AsInt(-1));
        }

        [TestMethod]
        public void IsDecimalTests() {
            Assert.IsTrue("1.3".IsDecimal());
            Assert.IsTrue(".13".IsDecimal());
            Assert.IsTrue("0.0".IsDecimal());
            Assert.IsTrue("12345678900123456".IsDecimal());
            Assert.IsTrue("0".IsDecimal());
            Assert.IsTrue("123456".IsDecimal());
            Assert.IsTrue(decimal.MaxValue.ToString().IsDecimal());
            Assert.IsTrue(decimal.MinValue.ToString().IsDecimal());
            Assert.IsFalse("gooblygook".IsDecimal());
            Assert.IsFalse("..0".IsDecimal());
            Assert.IsFalse(((string)null).IsDecimal());
        }

        [TestMethod]
        public void AsDecimalBasicTests() {
            Assert.AreEqual(-123m, "-123".AsDecimal());
            Assert.AreEqual(9.99m, "9.99".AsDecimal());
            Assert.AreEqual(0m, "0".AsDecimal());
            Assert.AreEqual(-1.1111m, "-1.1111".AsDecimal());
        }

        [TestMethod]
        public void AsDecimalDefaultTests() {
            // Illegal values default to 0
            Assert.AreEqual(0m, "abc".AsDecimal());

            Assert.AreEqual(-1.11m, "adlfkj".AsDecimal(-1.11m));
        }

        [TestMethod]
        public void IsFloatTests() {
            Assert.IsTrue("1.3".IsFloat());
            Assert.IsTrue(".13".IsFloat());
            Assert.IsTrue("0.0".IsFloat());
            Assert.IsTrue("12345678900123456".IsFloat());
            Assert.IsTrue("0".IsFloat());
            Assert.IsTrue("123456".IsFloat());
            Assert.IsTrue(float.MaxValue.ToString().IsFloat());
            Assert.IsTrue(float.MinValue.ToString().IsFloat());
            Assert.IsTrue(float.NegativeInfinity.ToString().IsFloat());
            Assert.IsTrue(float.PositiveInfinity.ToString().IsFloat());
            Assert.IsFalse("gooblygook".IsFloat());
            Assert.IsFalse(((string)null).IsFloat());
        }

        [TestMethod]
        public void AsFloatBasicTests() {
            Assert.AreEqual(-123f, "-123".AsFloat());
            Assert.AreEqual(9.99f, "9.99".AsFloat());
            Assert.AreEqual(0f, "0".AsFloat());
            Assert.AreEqual(-1.1111f, "-1.1111".AsFloat());
        }

        [TestMethod]
        public void AsFloatDefaultTests() {
            // Illegal values default to 0
            Assert.AreEqual(0f, "abc".AsFloat());

            Assert.AreEqual(-1.11f, "adlfkj".AsFloat(-1.11f));
        }

        [TestMethod]
        public void IsDateTimeTests() {
            Assert.IsTrue("Sat, 01 Nov 2008 19:35:00 GMT".IsDateTime());
            Assert.IsTrue("1/5/1979".IsDateTime());
            Assert.IsFalse("0".IsDateTime());
            Assert.IsTrue(DateTime.MaxValue.ToString().IsDateTime());
            Assert.IsTrue(DateTime.MinValue.ToString().IsDateTime());
            Assert.IsTrue(DateTime.UtcNow.ToString().IsDateTime());
            Assert.IsFalse("gooblygook".IsDateTime());
            Assert.IsFalse(((string)null).IsDateTime());
        }

        [TestMethod]
        public void AsDateTimeBasicTests() {
            Assert.AreEqual(DateTime.Parse("1/5/1979"), "1/5/1979".AsDateTime());
            Assert.AreEqual(DateTime.Parse("Sat, 01 Nov 2008 19:35:00 GMT"), "Sat, 01 Nov 2008 19:35:00 GMT".AsDateTime());
        }

        [TestMethod]
        public void AsDateTimeDefaultTests() {
            // Illegal values default to MinTime
            Assert.AreEqual(DateTime.MinValue, "1".AsDateTime());

            DateTime defaultV = DateTime.Parse("January 5, 1979");
            Assert.AreEqual(defaultV, "adlfkj".AsDateTime(defaultV));
            Assert.AreEqual(defaultV, "Jan 69".AsDateTime(defaultV));
        }

        [TestMethod]
        public void IsBoolTests() {
            Assert.IsTrue("TRUE".IsBool());
            Assert.IsTrue("TRUE   ".IsBool());
            Assert.IsTrue("false".IsBool());
            Assert.IsFalse("falsey".IsBool());
            Assert.IsFalse("gooblygook".IsBool());
            Assert.IsFalse("".IsBool());
            Assert.IsFalse(((string)null).IsBool());
        }

        [TestMethod]
        public void AsBoolTests() {
            Assert.IsTrue("TRuE".AsBool());
            Assert.IsFalse("False".AsBool());
            Assert.IsFalse("Die".AsBool(false));
            Assert.IsTrue("true!".AsBool(true));
            Assert.IsFalse("".AsBool());
            Assert.IsFalse(((string)null).AsBool());
            Assert.IsTrue("".AsBool(true));
            Assert.IsTrue(((string)null).AsBool(true));
        }

    }
}
