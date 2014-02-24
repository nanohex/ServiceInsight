namespace XmlSigning
{
    using System;
    using System.Security.Cryptography;
    using System.Security.Cryptography.Xml;
    using System.Xml;

    class SignedXmlVerifier
    {
        readonly string publicKey;
        
        public SignedXmlVerifier(string publicKey)
        {
            this.publicKey = publicKey;
        }

        public void VerifyXml(string xml)
        {
            var doc = LoadXmlDoc(xml);

            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(publicKey);

                var nsMgr = new XmlNamespaceManager(doc.NameTable);
                nsMgr.AddNamespace("sig", "http://www.w3.org/2000/09/xmldsig#");

                var signedXml = new SignedXml(doc);
                var signature = (XmlElement)doc.SelectSingleNode("//sig:Signature", nsMgr);
                if (signature == null)
                {
                    throw new Exception("Xml is invalid as it has no XML signature");
                }
                signedXml.LoadXml(signature);

                if (!signedXml.CheckSignature(rsa))
                {
                    throw new Exception("Xml is invalid as it failed signature check.");
                }
            }
        }

        static XmlDocument LoadXmlDoc(string xml)
        {
            try
            {
                var doc = new XmlDocument();
                doc.LoadXml(xml);
                return doc;
            }
            catch (XmlException exception)
            {
                throw new Exception("The text provided could not be parsed as XML.", exception);
            }
        }
    }
}
