using NUnit.Framework;
using Veradyn.Core.Managers;
using Veradyn.Core.Interfaces.Data;
using Veradyn.Core.Interfaces.Services;
using Veradyn.Core.Interfaces.Providers; // Add this
using Veradyn.Core.Models.Domain;
using Moq;

namespace Veradyn.Tests.Core
{
    [TestFixture]
    public class TokenManagerTests
    {
        private Mock<IUnitOfWork> _mockUow;
        private Mock<IPkceService> _mockPkce;
        private Mock<ITokenMintingService> _mockMinting;

        private Mock<IRepository<AuthorizationCode>> _mockAuthCodeRepo;
        private Mock<IRepository<Token>> _mockTokenRepo;

        private TokenManager _manager;

        [SetUp]
        public void Setup()
        {
            _mockUow = new Mock<IUnitOfWork>();
            _mockPkce = new Mock<IPkceService>();
            _mockMinting = new Mock<ITokenMintingService>();

            _mockAuthCodeRepo = new Mock<IRepository<AuthorizationCode>>();
            _mockTokenRepo = new Mock<IRepository<Token>>();

            _mockUow.Setup(u => u.AuthCodes).Returns(_mockAuthCodeRepo.Object);
            _mockUow.Setup(u => u.Tokens).Returns(_mockTokenRepo.Object);

            _manager = new TokenManager(_mockUow.Object, _mockPkce.Object, _mockMinting.Object);
        }

        [Test]
        public void ProcessTokenRequest_ReturnsError_WhenGrantTypeInvalid()
        {
            var result = _manager.ProcessTokenRequest("password", "code", "uri", "client", "verifier");
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("unsupported_grant_type", result.Error);
        }

        [Test]
        public void ProcessTokenRequest_ReturnsError_WhenCodeNotFound()
        {
            _mockAuthCodeRepo.Setup(r => r.GetById("bad-code")).Returns((AuthorizationCode)null);

            var result = _manager.ProcessTokenRequest("authorization_code", "bad-code", "uri", "client", "verifier");
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("invalid_grant", result.Error);
        }

        [Test]
        public void ProcessTokenRequest_ReturnsError_WhenCodeAlreadyUsed()
        {
            var code = new AuthorizationCode { Code = "c1", Used = true };
            _mockAuthCodeRepo.Setup(r => r.GetById("c1")).Returns(code);

            var result = _manager.ProcessTokenRequest("authorization_code", "c1", "uri", "client", "verifier");
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("invalid_grant", result.Error); // Should trigger revocation logic ideally
        }

        [Test]
        public void ProcessTokenRequest_ReturnsError_WhenPkceFails()
        {
            var code = new AuthorizationCode
            {
                Code = "c1",
                Used = false,
                ClientId = "client",
                RedirectUri = "uri",
                CodeChallenge = "chal",
                CodeChallengeMethod = "S256"
            };
            _mockAuthCodeRepo.Setup(r => r.GetById("c1")).Returns(code);

            _mockPkce.Setup(p => p.ValidateCodeVerifier("bad-verifier", "chal", "S256")).Returns(false);

            var result = _manager.ProcessTokenRequest("authorization_code", "c1", "uri", "client", "bad-verifier");
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("invalid_grant", result.Error);
        }

        [Test]
        public void ProcessTokenRequest_ReturnsTokens_WhenSuccess()
        {
            var code = new AuthorizationCode
            {
                Code = "c1",
                Used = false,
                ClientId = "client",
                RedirectUri = "uri",
                CodeChallenge = "chal",
                CodeChallengeMethod = "S256"
            };
            _mockAuthCodeRepo.Setup(r => r.GetById("c1")).Returns(code);
            _mockPkce.Setup(p => p.ValidateCodeVerifier("verifier", "chal", "S256")).Returns(true);

            _mockMinting.Setup(m => m.CreateAccessToken(code)).Returns(new Token { Value = "at" });
            _mockMinting.Setup(m => m.CreateIdToken(code, "at")).Returns(new Token { Value = "idt" });

            var result = _manager.ProcessTokenRequest("authorization_code", "c1", "uri", "client", "verifier");

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual("at", result.AccessToken);
            Assert.AreEqual("idt", result.IdToken);

            Assert.IsTrue(code.Used); // Code marked used
            _mockTokenRepo.Verify(r => r.Add(It.IsAny<Token>()), Times.Exactly(2));
            _mockUow.Verify(u => u.Commit(), Times.Once);
        }
    }
}
