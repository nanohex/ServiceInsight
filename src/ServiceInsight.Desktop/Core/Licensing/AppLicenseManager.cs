namespace NServiceBus.Profiler.Desktop.Core.Licensing
{
    using System;
    using log4net;
    using XmlSigning;

    public class AppLicenseManager
    {
        public AppLicenseManager()
        {
            Validate(LicenseStore.License);
        }

        public bool TryInstallLicense(string license)
        {
            try
            {
                var licenseToInstall = ValidateStandardLicense(license);

                if (licenseToInstall.Expired)
                {
                    return false;
                }

                CurrentLicense = licenseToInstall;

                LicenseStore.License = license;

                return true;
            }
            catch (Exception ex)
            {
                Logger.WarnFormat("Can't install license: {0}", ex);
                return false;
            }
        }

        public License CurrentLicense { get; private set; }

        public int GetRemainingTrialDays()
        {
            var now = DateTime.UtcNow.Date;

            var expiration = LicenseStore.GetTrialExpiration();

            var remainingDays = (expiration - now).Days;

            return remainingDays > 0 ? remainingDays : 0;
        }

        void Validate(string license)
        {
            try
            {

                CurrentLicense = ValidateStandardLicense(license);

                return;
            }
            catch (Exception ex)
            {
                Logger.Info("No valid license found: {0}. Falling back to trial mode",ex);
            }

            

            CurrentLicense = CreateTrialLicense();
        }

        License ValidateStandardLicense(string licenseText)
        {
            if (string.IsNullOrEmpty(licenseText))
            {
                throw new Exception("Empty license string");
            }

            xmlVerifier.VerifyXml(licenseText);

            return LicenseDeserializer.Deserialize(licenseText);
        }

        License CreateTrialLicense()
        {
            var trialExpirationDate = LicenseStore.GetTrialExpiration();

            Logger.InfoFormat("Configuring ServiceInsight to run in trial mode.");

            return new License
            {
                ExpirationDate = trialExpirationDate,
                IsExtendedTrial = false
            };

        }

        SignedXmlVerifier xmlVerifier = new SignedXmlVerifier(PublicKey);

        public const string PublicKey = @"<RSAKeyValue><Modulus>5M9/p7N+JczIN/e5eObahxeCIe//2xRLA9YTam7zBrcUGt1UlnXqL0l/8uO8rsO5tl+tjjIV9bOTpDLfx0H03VJyxsE8BEpSVu48xujvI25+0mWRnk4V50bDZykCTS3Du0c8XvYj5jIKOHPtU//mKXVULhagT8GkAnNnMj9CvTc=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

        static readonly ILog Logger = LogManager.GetLogger(typeof(AppLicenseManager));


    }
}