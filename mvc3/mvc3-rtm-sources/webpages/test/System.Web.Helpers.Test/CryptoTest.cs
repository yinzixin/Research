using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web.WebPages.TestUtils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Web.Helpers.Test {
    /// <summary>
    ///This is a test class for CryptoTest and is intended
    ///to contain all CryptoTest Unit Tests
    ///</summary>
    [TestClass]
    public class CryptoTest {
        [TestMethod]
        public void SHA256HashTest_ReturnsValidData() {
            string data = "foo bar";
            string expected = "FBC1A9F858EA9E177916964BD88C3D37B91A1E84412765E29950777F265C4B75";
            string actual;
            actual = Crypto.SHA256(data);
            Assert.AreEqual(expected, actual);

            actual = Crypto.Hash(Encoding.UTF8.GetBytes(data));
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GenerateSaltTest() {
            string salt = Crypto.GenerateSalt();
            salt = Crypto.GenerateSalt(64);
            Assert.AreEqual(24, Crypto.GenerateSalt().Length);
            Assert.AreEqual(12, Crypto.GenerateSalt(8).Length);
            Assert.AreEqual(88, Crypto.GenerateSalt(64).Length);
            Assert.AreEqual(44, Crypto.GenerateSalt(32).Length);
        }

        [TestMethod]
        public void HashPassword_PasswordGeneration() {
            // Act - call helper directly
            string generatedHash = Crypto.HashPassword("my-password");
            byte[] salt = new byte[16];
            Buffer.BlockCopy(Convert.FromBase64String(generatedHash), 1, salt, 0, 16); // extract salt from generated hash

            // Act - perform PBKDF2 directly
            string generatedHash2;
            using (var ms = new MemoryStream()) {
                using (var bw = new BinaryWriter(ms)) {
                    using (var deriveBytes = new Rfc2898DeriveBytes("my-password", salt, iterations: 1000)) {
                        bw.Write((byte)0x00); // version identifier
                        bw.Write(salt); // salt
                        bw.Write(deriveBytes.GetBytes(32)); // subkey
                    }

                    generatedHash2 = Convert.ToBase64String(ms.ToArray());
                }
            }

            // Assert
            Assert.AreEqual(generatedHash2, generatedHash, "Helper did not produce same hash as running PBKDF2 manually.");
        }

        [TestMethod]
        public void HashPassword_RoundTripping() {
            // Act & assert
            string password = "ImPepper";
            Assert.IsTrue(Crypto.VerifyHashedPassword(Crypto.HashPassword(password), password));
            Assert.IsFalse(Crypto.VerifyHashedPassword(Crypto.HashPassword(password), "ImSalt"));
            Assert.IsFalse(Crypto.VerifyHashedPassword(Crypto.HashPassword("Impepper"), password));
        }

        [TestMethod]
        public void VerifyHashedPassword_CorrectPassword_ReturnsTrue() {
            // Arrange
            string hashedPassword = "ALyuoraY/cIWD1hjo+K81/pf83qo6Q6T+UBYcXN9P3A9WHLvEY10f+lwW5qPG6h9xw=="; // this is for 'my-password'

            // Act
            bool retVal = Crypto.VerifyHashedPassword(hashedPassword, "my-password");

            // Assert
            Assert.IsTrue(retVal);
        }

        [TestMethod]
        public void VerifyHashedPassword_IncorrectPassword_ReturnsFalse() {
            // Arrange
            string hashedPassword = "ALyuoraY/cIWD1hjo+K81/pf83qo6Q6T+UBYcXN9P3A9WHLvEY10f+lwW5qPG6h9xw=="; // this is for 'my-password'

            // Act
            bool retVal = Crypto.VerifyHashedPassword(hashedPassword, "some-other-password");

            // Assert
            Assert.IsFalse(retVal);
        }

        [TestMethod]
        public void VerifyHashedPassword_InvalidPasswordHash_ReturnsFalse() {
            // Arrange
            string hashedPassword = "AAECAw=="; // this is an invalid password hash

            // Act
            bool retVal = Crypto.VerifyHashedPassword(hashedPassword, "hello-world");

            // Assert
            Assert.IsFalse(retVal);
        }

        [TestMethod]
        public void MD5HashTest_ReturnsValidData() {
            string data = "foo bar";
            string expected = "327B6F07435811239BC47E1544353273";
            string actual;
            actual = Crypto.Hash(data, algorithm: "md5");
            Assert.AreEqual(expected, actual);

            actual = Crypto.Hash(Encoding.UTF8.GetBytes(data), algorithm: "MD5");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SHA1HashTest_ReturnsValidData() {
            string data = "foo bar";
            string expected = "3773DEA65156909838FA6C22825CAFE090FF8030";
            string actual;
            actual = Crypto.SHA1(data);
            Assert.AreEqual(expected, actual);

            actual = Crypto.Hash(Encoding.UTF8.GetBytes(data), algorithm: "sha1");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SHA1HashTest_WithNull_ThrowsException() {
            ExceptionAssert.Throws<ArgumentNullException>(() => Crypto.SHA1((string)null));
            ExceptionAssert.Throws<ArgumentNullException>(() => Crypto.Hash((byte[])null, algorithm: "SHa1"));
        }

        [TestMethod]
        public void SHA256HashTest_WithNull_ThrowsException() {
            ExceptionAssert.Throws<ArgumentNullException>(() => Crypto.SHA256((string)null));
            ExceptionAssert.Throws<ArgumentNullException>(() => Crypto.Hash((byte[])null, algorithm: "sHa256"));
        }

        [TestMethod]
        public void MD5HashTest_WithNull_ThrowsException() {
            ExceptionAssert.Throws<ArgumentNullException>(() => Crypto.Hash((string)null, algorithm: "mD5"));
            ExceptionAssert.Throws<ArgumentNullException>(() => Crypto.Hash((byte[])null, algorithm: "mD5"));
        }

        [TestMethod]
        public void HashWithUnknownAlg_ThrowsException() {
            ExceptionAssert.Throws<InvalidOperationException>(() => Crypto.Hash("sdflksd", algorithm: "hao"), "The hash algorithm 'hao' is not supported, valid values are: sha256, sha1, md5");
        }

    }
}
