using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using System;
using TrustIdentity.Saml.Models;

namespace TrustIdentity.Saml.Serialization;

/// <summary>
/// SAML XML Serializer - Converts SAML objects to/from XML
/// </summary>
public class SamlSerializer
{
    /// <summary>
    /// Serializes a SAML Assertion
    /// </summary>
    public string SerializeAssertion(SamlAssertion assertion)
    {
        var xml = new XmlDocument();
        xml.PreserveWhitespace = true;

        var assertionElement = xml.CreateElement("saml", "Assertion", SamlConstants.Saml2Namespace);
        assertionElement.SetAttribute("ID", assertion.Id);
        assertionElement.SetAttribute("Version", assertion.Version);
        assertionElement.SetAttribute("IssueInstant", assertion.IssueInstant.ToString("o"));

        // Issuer
        var issuerElement = xml.CreateElement("saml", "Issuer", SamlConstants.Saml2Namespace);
        issuerElement.InnerText = assertion.Issuer;
        assertionElement.AppendChild(issuerElement);

        // Subject
        var subjectElement = CreateSubjectElement(xml, assertion.Subject);
        assertionElement.AppendChild(subjectElement);

        // Conditions
        var conditionsElement = CreateConditionsElement(xml, assertion.Conditions);
        assertionElement.AppendChild(conditionsElement);

        // Attribute Statements
        foreach (var attrStmt in assertion.AttributeStatements)
        {
            var attrStmtElement = CreateAttributeStatementElement(xml, attrStmt);
            assertionElement.AppendChild(attrStmtElement);
        }

        // Authn Statements
        foreach (var authnStmt in assertion.AuthnStatements)
        {
            var authnStmtElement = CreateAuthnStatementElement(xml, authnStmt);
            assertionElement.AppendChild(authnStmtElement);
        }

        xml.AppendChild(assertionElement);

        return xml.OuterXml;
    }

    /// <summary>
    /// Serializes a SAML Response
    /// </summary>
    public string SerializeResponse(SamlResponse response)
    {
        var xml = new XmlDocument();
        xml.PreserveWhitespace = true;

        var responseElement = xml.CreateElement("samlp", "Response", SamlConstants.Saml2ProtocolNamespace);
        responseElement.SetAttribute("xmlns:saml", SamlConstants.Saml2Namespace);
        responseElement.SetAttribute("ID", response.Id);
        responseElement.SetAttribute("Version", response.Version);
        responseElement.SetAttribute("IssueInstant", response.IssueInstant.ToString("o"));

        if (!string.IsNullOrEmpty(response.InResponseTo))
            responseElement.SetAttribute("InResponseTo", response.InResponseTo);

        if (!string.IsNullOrEmpty(response.Destination))
            responseElement.SetAttribute("Destination", response.Destination);

        // Issuer
        var issuerElement = xml.CreateElement("saml", "Issuer", SamlConstants.Saml2Namespace);
        issuerElement.InnerText = response.Issuer;
        responseElement.AppendChild(issuerElement);

        // Status
        var statusElement = CreateStatusElement(xml, response.Status);
        responseElement.AppendChild(statusElement);

        // Assertions
        foreach (var assertion in response.Assertions)
        {
            var assertionXml = new XmlDocument();
            assertionXml.LoadXml(SerializeAssertion(assertion));
            var importedNode = xml.ImportNode(assertionXml.DocumentElement!, true);
            responseElement.AppendChild(importedNode);
        }

        xml.AppendChild(responseElement);

        return xml.OuterXml;
    }

    /// <summary>
    /// Serializes a SAML Authentication Request
    /// </summary>
    public string SerializeAuthnRequest(SamlAuthnRequest request)
    {
        var xml = new XmlDocument();
        xml.PreserveWhitespace = true;

        var authnRequestElement = xml.CreateElement("samlp", "AuthnRequest", SamlConstants.Saml2ProtocolNamespace);
        authnRequestElement.SetAttribute("xmlns:saml", SamlConstants.Saml2Namespace);
        authnRequestElement.SetAttribute("ID", request.Id);
        authnRequestElement.SetAttribute("Version", request.Version);
        authnRequestElement.SetAttribute("IssueInstant", request.IssueInstant.ToString("o"));

        if (!string.IsNullOrEmpty(request.AssertionConsumerServiceURL))
            authnRequestElement.SetAttribute("AssertionConsumerServiceURL", request.AssertionConsumerServiceURL);

        if (!string.IsNullOrEmpty(request.ProtocolBinding))
            authnRequestElement.SetAttribute("ProtocolBinding", request.ProtocolBinding);

        if (request.ForceAuthn)
            authnRequestElement.SetAttribute("ForceAuthn", "true");

        if (request.IsPassive)
            authnRequestElement.SetAttribute("IsPassive", "true");

        // Issuer
        var issuerElement = xml.CreateElement("saml", "Issuer", SamlConstants.Saml2Namespace);
        issuerElement.InnerText = request.Issuer;
        authnRequestElement.AppendChild(issuerElement);

        // NameIDPolicy
        if (request.NameIdPolicy != null)
        {
            var nameIdPolicyElement = xml.CreateElement("samlp", "NameIDPolicy", SamlConstants.Saml2ProtocolNamespace);
            nameIdPolicyElement.SetAttribute("Format", request.NameIdPolicy.Format);
            nameIdPolicyElement.SetAttribute("AllowCreate", request.NameIdPolicy.AllowCreate.ToString().ToLower());
            authnRequestElement.AppendChild(nameIdPolicyElement);
        }

        xml.AppendChild(authnRequestElement);

        return xml.OuterXml;
    }

    /// <summary>
    /// Deserializes a SAML Authentication Request
    /// </summary>
    public SamlAuthnRequest? DeserializeAuthnRequest(string xml)
    {
        var doc = new XmlDocument();
        doc.PreserveWhitespace = true;

        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null
        };

        using (var stringReader = new System.IO.StringReader(xml))
        using (var reader = XmlReader.Create(stringReader, settings))
        {
            doc.Load(reader);
        }

        var nsmgr = new XmlNamespaceManager(doc.NameTable);
        nsmgr.AddNamespace("samlp", SamlConstants.Saml2ProtocolNamespace);
        nsmgr.AddNamespace("saml", SamlConstants.Saml2Namespace);

        var authnRequestElement = doc.SelectSingleNode("/samlp:AuthnRequest", nsmgr) as XmlElement;
        if (authnRequestElement == null) return null;

        var request = new SamlAuthnRequest
        {
            Id = authnRequestElement.GetAttribute("ID"),
            Version = authnRequestElement.GetAttribute("Version"),
            IssueInstant = DateTime.Parse(authnRequestElement.GetAttribute("IssueInstant")),
            AssertionConsumerServiceURL = authnRequestElement.GetAttribute("AssertionConsumerServiceURL"),
            ProtocolBinding = authnRequestElement.GetAttribute("ProtocolBinding")
        };

        var issuerNode = authnRequestElement.SelectSingleNode("saml:Issuer", nsmgr);
        if (issuerNode != null)
            request.Issuer = issuerNode.InnerText;

        return request;
    }

    private XmlElement CreateSubjectElement(XmlDocument xml, SamlSubject subject)
    {
        var subjectElement = xml.CreateElement("saml", "Subject", SamlConstants.Saml2Namespace);

        // NameID
        var nameIdElement = xml.CreateElement("saml", "NameID", SamlConstants.Saml2Namespace);
        nameIdElement.SetAttribute("Format", subject.NameIdFormat);
        nameIdElement.InnerText = subject.NameId;
        subjectElement.AppendChild(nameIdElement);

        // Subject Confirmation
        if (subject.SubjectConfirmation != null)
        {
            var subjectConfElement = xml.CreateElement("saml", "SubjectConfirmation", SamlConstants.Saml2Namespace);
            subjectConfElement.SetAttribute("Method", subject.SubjectConfirmation.Method);

            if (subject.SubjectConfirmation.SubjectConfirmationData != null)
            {
                var subjectConfDataElement = xml.CreateElement("saml", "SubjectConfirmationData", SamlConstants.Saml2Namespace);
                
                var data = subject.SubjectConfirmation.SubjectConfirmationData;
                if (data.NotBefore.HasValue)
                    subjectConfDataElement.SetAttribute("NotBefore", data.NotBefore.Value.ToString("o"));
                
                subjectConfDataElement.SetAttribute("NotOnOrAfter", data.NotOnOrAfter.ToString("o"));
                
                if (!string.IsNullOrEmpty(data.Recipient))
                    subjectConfDataElement.SetAttribute("Recipient", data.Recipient);
                
                if (!string.IsNullOrEmpty(data.InResponseTo))
                    subjectConfDataElement.SetAttribute("InResponseTo", data.InResponseTo);

                subjectConfElement.AppendChild(subjectConfDataElement);
            }

            subjectElement.AppendChild(subjectConfElement);
        }

        return subjectElement;
    }

    private XmlElement CreateConditionsElement(XmlDocument xml, SamlConditions conditions)
    {
        var conditionsElement = xml.CreateElement("saml", "Conditions", SamlConstants.Saml2Namespace);
        conditionsElement.SetAttribute("NotBefore", conditions.NotBefore.ToString("o"));
        conditionsElement.SetAttribute("NotOnOrAfter", conditions.NotOnOrAfter.ToString("o"));

        foreach (var audienceRestriction in conditions.AudienceRestrictions)
        {
            var audienceRestrictionElement = xml.CreateElement("saml", "AudienceRestriction", SamlConstants.Saml2Namespace);
            
            foreach (var audience in audienceRestriction.Audiences)
            {
                var audienceElement = xml.CreateElement("saml", "Audience", SamlConstants.Saml2Namespace);
                audienceElement.InnerText = audience;
                audienceRestrictionElement.AppendChild(audienceElement);
            }

            conditionsElement.AppendChild(audienceRestrictionElement);
        }

        return conditionsElement;
    }

    private XmlElement CreateAttributeStatementElement(XmlDocument xml, SamlAttributeStatement attrStmt)
    {
        var attrStmtElement = xml.CreateElement("saml", "AttributeStatement", SamlConstants.Saml2Namespace);

        foreach (var attr in attrStmt.Attributes)
        {
            var attrElement = xml.CreateElement("saml", "Attribute", SamlConstants.Saml2Namespace);
            attrElement.SetAttribute("Name", attr.Name);

            if (!string.IsNullOrEmpty(attr.NameFormat))
                attrElement.SetAttribute("NameFormat", attr.NameFormat);

            if (!string.IsNullOrEmpty(attr.FriendlyName))
                attrElement.SetAttribute("FriendlyName", attr.FriendlyName);

            foreach (var value in attr.AttributeValues)
            {
                var attrValueElement = xml.CreateElement("saml", "AttributeValue", SamlConstants.Saml2Namespace);
                attrValueElement.InnerText = value;
                attrElement.AppendChild(attrValueElement);
            }

            attrStmtElement.AppendChild(attrElement);
        }

        return attrStmtElement;
    }

    private XmlElement CreateAuthnStatementElement(XmlDocument xml, SamlAuthnStatement authnStmt)
    {
        var authnStmtElement = xml.CreateElement("saml", "AuthnStatement", SamlConstants.Saml2Namespace);
        authnStmtElement.SetAttribute("AuthnInstant", authnStmt.AuthnInstant.ToString("o"));

        if (!string.IsNullOrEmpty(authnStmt.SessionIndex))
            authnStmtElement.SetAttribute("SessionIndex", authnStmt.SessionIndex);

        if (authnStmt.SessionNotOnOrAfter.HasValue)
            authnStmtElement.SetAttribute("SessionNotOnOrAfter", authnStmt.SessionNotOnOrAfter.Value.ToString("o"));

        // AuthnContext
        var authnContextElement = xml.CreateElement("saml", "AuthnContext", SamlConstants.Saml2Namespace);
        var authnContextClassRefElement = xml.CreateElement("saml", "AuthnContextClassRef", SamlConstants.Saml2Namespace);
        authnContextClassRefElement.InnerText = authnStmt.AuthnContext.AuthnContextClassRef;
        authnContextElement.AppendChild(authnContextClassRefElement);
        authnStmtElement.AppendChild(authnContextElement);

        return authnStmtElement;
    }

    private XmlElement CreateStatusElement(XmlDocument xml, SamlStatus status)
    {
        var statusElement = xml.CreateElement("samlp", "Status", SamlConstants.Saml2ProtocolNamespace);

        var statusCodeElement = xml.CreateElement("samlp", "StatusCode", SamlConstants.Saml2ProtocolNamespace);
        statusCodeElement.SetAttribute("Value", status.StatusCode);
        statusElement.AppendChild(statusCodeElement);

        if (!string.IsNullOrEmpty(status.StatusMessage))
        {
            var statusMessageElement = xml.CreateElement("samlp", "StatusMessage", SamlConstants.Saml2ProtocolNamespace);
            statusMessageElement.InnerText = status.StatusMessage;
            statusElement.AppendChild(statusMessageElement);
        }

        return statusElement;
    }

    // Added for Single Logout support
    /// <summary>
    /// Serializes a SAML Logout Request
    /// </summary>
    public string SerializeLogoutRequest(SamlLogoutRequest request)
    {
        var xml = new XmlDocument();
        xml.PreserveWhitespace = true;

        var logoutRequestElement = xml.CreateElement("samlp", "LogoutRequest", SamlConstants.Saml2ProtocolNamespace);
        logoutRequestElement.SetAttribute("xmlns:saml", SamlConstants.Saml2Namespace);
        logoutRequestElement.SetAttribute("ID", request.Id);
        logoutRequestElement.SetAttribute("Version", request.Version);
        logoutRequestElement.SetAttribute("IssueInstant", request.IssueInstant.ToString("o"));

        // Issuer
        var issuerElement = xml.CreateElement("saml", "Issuer", SamlConstants.Saml2Namespace);
        issuerElement.InnerText = request.Issuer;
        logoutRequestElement.AppendChild(issuerElement);

        // NameID
        var nameIdElement = xml.CreateElement("saml", "NameID", SamlConstants.Saml2Namespace);
        nameIdElement.SetAttribute("Format", request.NameId.Format);
        nameIdElement.InnerText = request.NameId.Value;
        logoutRequestElement.AppendChild(nameIdElement);

        // SessionIndex (optional)
        if (!string.IsNullOrEmpty(request.SessionIndex))
        {
            var sessionIndexElement = xml.CreateElement("samlp", "SessionIndex", SamlConstants.Saml2ProtocolNamespace);
            sessionIndexElement.InnerText = request.SessionIndex;
            logoutRequestElement.AppendChild(sessionIndexElement);
        }

        xml.AppendChild(logoutRequestElement);
        return xml.OuterXml;
    }

    /// <summary>
    /// Deserializes a SAML Logout Request
    /// </summary>
    public SamlLogoutRequest? DeserializeLogoutRequest(string xml)
    {
        var doc = new XmlDocument();
        doc.PreserveWhitespace = true;

        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null
        };

        using (var stringReader = new System.IO.StringReader(xml))
        using (var reader = XmlReader.Create(stringReader, settings))
        {
            doc.Load(reader);
        }

        var nsmgr = new XmlNamespaceManager(doc.NameTable);
        nsmgr.AddNamespace("samlp", SamlConstants.Saml2ProtocolNamespace);
        nsmgr.AddNamespace("saml", SamlConstants.Saml2Namespace);

        var logoutRequestElement = doc.SelectSingleNode("/samlp:LogoutRequest", nsmgr) as XmlElement;
        if (logoutRequestElement == null) return null;

        var request = new SamlLogoutRequest
        {
            Id = logoutRequestElement.GetAttribute("ID"),
            Version = logoutRequestElement.GetAttribute("Version"),
            IssueInstant = DateTime.Parse(logoutRequestElement.GetAttribute("IssueInstant"))
        };

        var issuerNode = logoutRequestElement.SelectSingleNode("saml:Issuer", nsmgr);
        if (issuerNode != null)
            request.Issuer = issuerNode.InnerText;

        var nameIdNode = logoutRequestElement.SelectSingleNode("saml:NameID", nsmgr) as XmlElement;
        if (nameIdNode != null)
        {
            request.NameId = new SamlNameId
            {
                Value = nameIdNode.InnerText,
                Format = nameIdNode.GetAttribute("Format")
            };
        }

        var sessionIndexNode = logoutRequestElement.SelectSingleNode("samlp:SessionIndex", nsmgr);
        if (sessionIndexNode != null)
            request.SessionIndex = sessionIndexNode.InnerText;

        return request;
    }

    /// <summary>
    /// Serializes a SAML Logout Response
    /// </summary>
    public string SerializeLogoutResponse(SamlLogoutResponse response)
    {
        var xml = new XmlDocument();
        xml.PreserveWhitespace = true;

        var logoutResponseElement = xml.CreateElement("samlp", "LogoutResponse", SamlConstants.Saml2ProtocolNamespace);
        logoutResponseElement.SetAttribute("xmlns:saml", SamlConstants.Saml2Namespace);
        logoutResponseElement.SetAttribute("ID", response.Id);
        logoutResponseElement.SetAttribute("Version", response.Version);
        logoutResponseElement.SetAttribute("IssueInstant", response.IssueInstant.ToString("o"));

        if (!string.IsNullOrEmpty(response.InResponseTo))
            logoutResponseElement.SetAttribute("InResponseTo", response.InResponseTo);

        // Issuer
        var issuerElement = xml.CreateElement("saml", "Issuer", SamlConstants.Saml2Namespace);
        issuerElement.InnerText = response.Issuer;
        logoutResponseElement.AppendChild(issuerElement);

        // Status
        var statusElement = CreateStatusElement(xml, response.Status);
        logoutResponseElement.AppendChild(statusElement);

        xml.AppendChild(logoutResponseElement);
        return xml.OuterXml;
    }

    /// <summary>
    /// Deserializes a SAML Logout Response
    /// </summary>
    public SamlLogoutResponse? DeserializeLogoutResponse(string xml)
    {
        var doc = new XmlDocument();
        doc.PreserveWhitespace = true;

        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null
        };

        using (var stringReader = new System.IO.StringReader(xml))
        using (var reader = XmlReader.Create(stringReader, settings))
        {
            doc.Load(reader);
        }

        var nsmgr = new XmlNamespaceManager(doc.NameTable);
        nsmgr.AddNamespace("samlp", SamlConstants.Saml2ProtocolNamespace);
        nsmgr.AddNamespace("saml", SamlConstants.Saml2Namespace);

        var logoutResponseElement = doc.SelectSingleNode("/samlp:LogoutResponse", nsmgr) as XmlElement;
        if (logoutResponseElement == null) return null;

        var response = new SamlLogoutResponse
        {
            Id = logoutResponseElement.GetAttribute("ID"),
            Version = logoutResponseElement.GetAttribute("Version"),
            IssueInstant = DateTime.Parse(logoutResponseElement.GetAttribute("IssueInstant")),
            InResponseTo = logoutResponseElement.GetAttribute("InResponseTo")
        };

        var issuerNode = logoutResponseElement.SelectSingleNode("saml:Issuer", nsmgr);
        if (issuerNode != null)
            response.Issuer = issuerNode.InnerText;

        var statusNode = logoutResponseElement.SelectSingleNode("samlp:Status", nsmgr);
        if (statusNode != null)
        {
            var statusCodeNode = statusNode.SelectSingleNode("samlp:StatusCode", nsmgr) as XmlElement;
            if (statusCodeNode != null)
            {
                response.Status = new SamlStatus
                {
                    StatusCode = statusCodeNode.GetAttribute("Value")
                };

                var statusMessageNode = statusNode.SelectSingleNode("samlp:StatusMessage", nsmgr);
                if (statusMessageNode != null)
                    response.Status.StatusMessage = statusMessageNode.InnerText;
            }
        }

        return response;
    }
}
