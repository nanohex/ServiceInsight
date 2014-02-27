namespace NServiceBus.Profiler.Desktop.Core.Licensing
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Xml;

    static class LicenseDeserializer
    {
        public static PlatformLicense Deserialize(string licenseText)
        {
            var license = new PlatformLicense();
            var doc = new XmlDocument();
            doc.LoadXml(licenseText);


            var applications = doc.SelectSingleNode("/license/@Applications");


            if (applications == null || !applications.Value.Contains("SI"))
            {
                throw new Exception("ServicInsight not included in the license");
            }


            var expirationDate = doc.SelectSingleNode("/license/@expiration");


            if (expirationDate != null)
            {
                license.ExpirationDate = Parse(expirationDate.Value);  
 
            }
            
            var upgradeProtectionExpiration = doc.SelectSingleNode("/license/@UpgradeProtectionExpiration");
            
            if (upgradeProtectionExpiration != null)
            {
                license.UpgradeProtectionExpiration = Parse(upgradeProtectionExpiration.Value);
            }

            var licenseType = doc.SelectSingleNode("/license/@LicenseType");

            if (licenseType != null)
            {
                license.LicenseType = licenseType.Value;
            }




            var name = doc.SelectSingleNode("/license/name");

            if (name != null)
            {
                license.RegisteredTo = name.InnerText;
            }

            return license;
        }

        static DateTime Parse(string dateStringFromLicense)
        {
            if (string.IsNullOrEmpty(dateStringFromLicense))
            {
                throw new Exception("Invalid datestring found in xml");
            }

            return DateTime.ParseExact(dateStringFromLicense.Split('T').First(), "yyyy-MM-dd", null, DateTimeStyles.AssumeUniversal).ToUniversalTime();
        }

    }
}