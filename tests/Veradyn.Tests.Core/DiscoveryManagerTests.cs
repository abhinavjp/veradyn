using NUnit.Framework;
using Veradyn.Core.Managers;
using Veradyn.Core.Services;
using Veradyn.Core.InMemory.Data;
using Veradyn.Core.Interfaces.Services;
using Veradyn.Core.Models.Domain;
using Moq;
using System.Collections.Generic;

namespace Veradyn.Tests.Core
{
    [TestFixture]
    public class DiscoveryManagerTests
    {
        private Mock<ITenantService> _mockTenantService;
        private DiscoveryManager _manager;

        [SetUp]
        public void Setup()
        {
            _mockTenantService = new Mock<ITenantService>();
            _manager = new DiscoveryManager(_mockTenantService.Object);
        }

        [Test]
        public void GetOpenIdConfiguration_ReturnsValidJsonStructure()
        {
            // Arrange
            _mockTenantService.Setup(s => s.GetTenant("default"))
                .Returns(new Tenant { Id = "default", Issuer = "https://idp.example.com" });

            // Act
            var config = _manager.GetOpenIdConfiguration("https://idp.example.com/", "default");

            // Assert
            Assert.IsNotNull(config);
            Assert.AreEqual("https://idp.example.com/", config["issuer"]);
            Assert.AreEqual("https://idp.example.com/authorize", config["authorization_endpoint"]);
            Assert.AreEqual("https://idp.example.com/.well-known/jwks.json", config["jwks_uri"]);
            Assert.That((IEnumerable<string>)config["response_types_supported"], Does.Contain("code"));
            Assert.That((IEnumerable<string>)config["code_challenge_methods_supported"], Does.Contain("S256"));
        }

        [Test]
        public void GetOpenIdConfiguration_ReturnsNull_WhenTenantNotFound()
        {
            // Arrange
            _mockTenantService.Setup(s => s.GetTenant(It.IsAny<string>())).Returns((Tenant)null);

            // Act
            var config = _manager.GetOpenIdConfiguration("https://idp.example.com/", "unknown");

            // Assert
            Assert.IsNull(config);
        }
    }
}
