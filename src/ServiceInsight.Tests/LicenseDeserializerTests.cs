namespace NServiceBus.Profiler.Tests
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using Desktop.Core.Licensing;
    using NUnit.Framework;

    [TestFixture]
    public class LicenseDeserializerTests
    {

        [Test]
        [Explicit]
        public void ParseAllTheLicenses()
        {
            //generate the licenses by running the unit tests in: https://github.com/Particular/Operations.LicenseGenerator (private repo)
            foreach (var licensePath in Directory.EnumerateFiles(allTheLicensesDir, "*.xml", SearchOption.AllDirectories))
            {
                Debug.WriteLine(licensePath);

                LicenseDeserializer.Deserialize(File.ReadAllText(licensePath));
            }
        }

        [Test]
        public void WithAllProperties()
        {
            var license = LicenseDeserializer.Deserialize(File.ReadAllText(@".\licensing\SI_Basic_expired.xml"));

            var dateTimeOffset = new DateTime(2010, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            Assert.AreEqual(dateTimeOffset, license.ExpirationDate);
            Assert.AreEqual("SI_Basic_expired", license.RegisteredTo);

            Assert.AreEqual("Basic", license.LicenseType);
            
            Assert.IsFalse(license.UpgradeProtectionExpiration.HasValue);
            Assert.IsTrue(license.Expired);
            Assert.IsTrue(license.IsCommercialLicense);
            Assert.IsFalse(license.IsTrialLicense);
        }

        [Test]
        public void WithServiceInsightNotIncluded()
        {
            Assert.Throws<Exception>(()=>LicenseDeserializer.Deserialize(File.ReadAllText(@".\licensing\SI_not_included.xml")));

        }

        [Test]
        public void WithInvalidKey()
        {
            Assert.Throws<Exception>(() => new XmlSigning.SignedXmlVerifier(AppLicenseManager.PublicKey).VerifyXml(File.ReadAllText(@".\licensing\Invalid_key.xml")));
        }



        [Test]
        public void WithTamperedData()
        {
            Assert.Throws<Exception>(() => new XmlSigning.SignedXmlVerifier(AppLicenseManager.PublicKey).VerifyXml(File.ReadAllText(@".\licensing\SI_Basic_tampered.xml")));
        }

        [Test]
        [Ignore("Not implemented yet")]
        public void WithNoUpgradeProtection()
        {
            var license = LicenseDeserializer.Deserialize(File.ReadAllText(@".\licensing\SI_not_included.xml"));
            Assert.IsNull(license.UpgradeProtectionExpiration);
        }


        //Set an environment variable to a path containing all the licenses to run this test
        static string allTheLicensesDir = Environment.GetEnvironmentVariable("NServiceBusLicensesPath");
     
       
    }
}