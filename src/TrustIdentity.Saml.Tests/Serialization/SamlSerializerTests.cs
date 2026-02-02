using System;
using TrustIdentity.Saml.Models;
using TrustIdentity.Saml.Serialization;
using Xunit;

namespace TrustIdentity.Saml.Tests.Serialization
{
    public class SamlSerializerTests
    {
        private readonly SamlSerializer _serializer;

        public SamlSerializerTests()
        {
            _serializer = new SamlSerializer();
        }

        [Fact]
        public void SerializeAuthnRequest_ShouldReturnValidXml()
        {
            // Arrange
            var request = new SamlAuthnRequest
            {
                Id = "req1",
                Version = "2.0",
                IssueInstant = DateTime.UtcNow,
                Issuer = "https://sp.example.com",
                AssertionConsumerServiceURL = "https://sp.example.com/acs"
            };

            // Act
            var xml = _serializer.SerializeAuthnRequest(request);

            // Assert
            Assert.NotNull(xml);
            Assert.Contains("<samlp:AuthnRequest", xml);
            Assert.Contains("ID=\"req1\"", xml);
            Assert.Contains("https://sp.example.com/acs", xml);
            Assert.Contains("saml:Issuer", xml);
        }

        [Fact]
        public void DeserializeAuthnRequest_ShouldReturnObject()
        {
            // Arrange
            var xml = @"<samlp:AuthnRequest xmlns:samlp=""urn:oasis:names:tc:SAML:2.0:protocol"" xmlns:saml=""urn:oasis:names:tc:SAML:2.0:assertion"" ID=""req1"" Version=""2.0"" IssueInstant=""2023-01-01T00:00:00Z"" AssertionConsumerServiceURL=""https://sp.example.com/acs"">
                          <saml:Issuer>https://sp.example.com</saml:Issuer>
                        </samlp:AuthnRequest>";

            // Act
            var request = _serializer.DeserializeAuthnRequest(xml);

            // Assert
            Assert.NotNull(request);
            Assert.Equal("req1", request.Id);
            Assert.Equal("2.0", request.Version);
            Assert.Equal("https://sp.example.com", request.Issuer);
            Assert.Equal("https://sp.example.com/acs", request.AssertionConsumerServiceURL);
        }
    }
}
