using NUnit.Framework;
using Veradyn.Core.Services;
using Veradyn.Core.Interfaces.Providers;
using Moq;

namespace Veradyn.Tests.Core
{
    [TestFixture]
    public class PkceServiceTests
    {
        private Mock<ICryptoProvider> _mockCrypto;
        private PkceService _service;

        [SetUp]
        public void Setup()
        {
            _mockCrypto = new Mock<ICryptoProvider>();
            _service = new PkceService(_mockCrypto.Object);
        }

        [Test]
        public void ValidateCodeChallenge_ReturnsTrue_ForValidInput()
        {
            Assert.IsTrue(_service.ValidateCodeChallenge("any-challenge", "S256"));
            Assert.IsTrue(_service.ValidateCodeChallenge("any-challenge", "plain"));
        }

        [Test]
        public void ValidateCodeChallenge_ReturnsFalse_ForInvalidMethod()
        {
            Assert.IsFalse(_service.ValidateCodeChallenge("any-challenge", "md5"));
        }

        [Test]
        public void ValidateCodeVerifier_Plain_ReturnsTrue_WhenMatch()
        {
            Assert.IsTrue(_service.ValidateCodeVerifier("secret", "secret", "plain"));
        }

        [Test]
        public void ValidateCodeVerifier_Plain_ReturnsFalse_WhenMismatch()
        {
            Assert.IsFalse(_service.ValidateCodeVerifier("secret", "wrong", "plain"));
        }

        [Test]
        public void ValidateCodeVerifier_S256_ReturnsTrue_WhenHashMatches()
        {
            // Arrange
            var verifier = "secret";
            var hashed = "E9Melhoa2OwvFrEMTJguCHaoeK1t8URWbuGJSstw-cM"; // Known hash of "secret"

            // Mock Crypto: PkceService calls ComputeHash(verifier)
            // It expects ComputeHash to return the Base64 of the hash
            // The service then converts it to Base64Url

            // Wait, SystemCryptoProvider.ComputeHash returns Base64.
            // "E9Melhoa2OwvFrEMTJguCHaoeK1t8URWbuGJSstw+cM=" (Standard B64) -> "E9Melhoa2OwvFrEMTJguCHaoeK1t8URWbuGJSstw-cM" (B64Url)

            // Let's mock ComputeHash to return Standard Base64
            _mockCrypto.Setup(c => c.ComputeHash(verifier)).Returns("E9Melhoa2OwvFrEMTJguCHaoeK1t8URWbuGJSstw+cM=");

            // Act
            var result = _service.ValidateCodeVerifier(verifier, hashed, "S256");

            // Assert
            Assert.IsTrue(result);
        }
    }
}
