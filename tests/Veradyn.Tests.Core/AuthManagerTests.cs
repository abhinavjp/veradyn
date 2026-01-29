using NUnit.Framework;
using Veradyn.Core.Managers;
using Veradyn.Core.Interfaces.Data;
using Veradyn.Core.Interfaces.Services;
using Veradyn.Core.Interfaces.Providers;
using Veradyn.Core.Models.Domain;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Veradyn.Tests.Core
{
    [TestFixture]
    public class AuthManagerTests
    {
        private Mock<IUnitOfWork> _mockUow;
        private Mock<IClientService> _mockClientService;
        private Mock<IValidationService> _mockValidation;
        private Mock<IPkceService> _mockPkce;
        private Mock<ICryptoProvider> _mockCrypto;
        private Mock<IClock> _mockClock;

        private Mock<IRepository<AuthorizationCode>> _mockAuthCodeRepo;

        private AuthManager _manager;

        [SetUp]
        public void Setup()
        {
            _mockUow = new Mock<IUnitOfWork>();
            _mockClientService = new Mock<IClientService>();
            _mockValidation = new Mock<IValidationService>();
            _mockPkce = new Mock<IPkceService>();
            _mockCrypto = new Mock<ICryptoProvider>();
            _mockClock = new Mock<IClock>();

            _mockAuthCodeRepo = new Mock<IRepository<AuthorizationCode>>();
            _mockUow.Setup(u => u.AuthCodes).Returns(_mockAuthCodeRepo.Object);

            _manager = new AuthManager(
                _mockUow.Object,
                _mockClientService.Object,
                _mockValidation.Object,
                _mockPkce.Object,
                _mockCrypto.Object,
                _mockClock.Object
            );
        }

        [Test]
        public void ProcessAuthorizeRequest_ReturnsError_WhenClientNotFound()
        {
            _mockClientService.Setup(s => s.GetClient("bad-client")).Returns((Client)null);

            var result = _manager.ProcessAuthorizeRequest("bad-client", "http://cb", "code", "openid", "state", "chal", "S256", "nonce");

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("invalid_client", result.Error);
        }

        [Test]
        public void ProcessAuthorizeRequest_ReturnsError_WhenRedirectUriInvalid()
        {
            var client = new Client { ClientId = "c1" };
            _mockClientService.Setup(s => s.GetClient("c1")).Returns(client);
            _mockValidation.Setup(v => v.ValidateRedirectUri(client, "http://bad")).Returns(false);

            var result = _manager.ProcessAuthorizeRequest("c1", "http://bad", "code", "openid", "state", "chal", "S256", "nonce");

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("invalid_request", result.Error);
        }

        [Test]
        public void ProcessAuthorizeRequest_ReturnsLoginRequired_WhenUserNotAuthenticated()
        {
            var client = new Client { ClientId = "c1", RequirePkce = false };
            _mockClientService.Setup(s => s.GetClient("c1")).Returns(client);
            _mockValidation.Setup(v => v.ValidateRedirectUri(client, "http://cb")).Returns(true);
            _mockValidation.Setup(v => v.ValidateScopes(client, It.IsAny<IEnumerable<string>>())).Returns(true);

            var result = _manager.ProcessAuthorizeRequest("c1", "http://cb", "code", "openid", "state", "chal", "S256", "nonce", currentUser: null);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.IsLoginRequired);
        }

        [Test]
        public void ProcessAuthorizeRequest_ReturnsRedirectWithCode_WhenSuccess()
        {
            var client = new Client { ClientId = "c1", TenantId = "t1", RequirePkce = true };
            var user = new User { Id = "u1" };

            _mockClientService.Setup(s => s.GetClient("c1")).Returns(client);
            _mockValidation.Setup(v => v.ValidateRedirectUri(client, "http://cb")).Returns(true);
            _mockValidation.Setup(v => v.ValidateScopes(client, It.IsAny<IEnumerable<string>>())).Returns(true);
            _mockPkce.Setup(p => p.ValidateCodeChallenge("chal", "S256")).Returns(true);
            _mockCrypto.Setup(c => c.GenerateSecureToken(32)).Returns("auth_code_123");
            _mockClock.Setup(c => c.UtcNow).Returns(new DateTime(2023, 1, 1));

            var result = _manager.ProcessAuthorizeRequest("c1", "http://cb", "code", "openid", "state", "chal", "S256", "nonce", currentUser: user);

            Assert.IsTrue(result.IsSuccess);
            Assert.That(result.RedirectUri, Does.Contain("code=auth_code_123"));
            Assert.That(result.RedirectUri, Does.Contain("state=state"));

            _mockAuthCodeRepo.Verify(r => r.Add(It.Is<AuthorizationCode>(c =>
               c.Code == "auth_code_123" &&
               c.ClientId == "c1" &&
               c.SubjectId == "u1"
            )), Times.Once);
            _mockUow.Verify(u => u.Commit(), Times.Once);
        }
    }
}
