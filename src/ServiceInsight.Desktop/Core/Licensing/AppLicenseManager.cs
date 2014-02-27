﻿namespace NServiceBus.Profiler.Desktop.Core.Licensing
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
                CurrentLicense = ValidateStandardLicense(license);

                LicenseStore.License = license;

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public PlatformLicense CurrentLicense { get; private set; }

        public int GetRemainingTrialDays()
        {
            var now = DateTime.UtcNow.Date;

            var expiration = LicenseStore.GetTrialExpiration();

            var remainingDays = (expiration - now).Days;

            return remainingDays > 0 ? remainingDays : 0;
        }

        bool Validate(string license)
        {
            try
            {

                CurrentLicense = ValidateStandardLicense(license);

                return true;
            }
            catch (Exception ex)
            {
                Logger.Info("Suitable license was not found: {0}", ex);
            }

            Logger.Info("No valid license found. Falling back to trial mode");

            CurrentLicense = CreateTrialLicense();

            return !CurrentLicense.Expired;
        }

        PlatformLicense ValidateStandardLicense(string licenseText)
        {
            if (string.IsNullOrEmpty(licenseText))
            {
                throw new Exception("Empty license string");
            }

            xmlVerifier.VerifyXml(licenseText);

            var license = LicenseDeserializer.Deserialize(licenseText);

            if (license.Expired)
            {
                throw new Exception("License has expired");
            }

            return license;
        }

        PlatformLicense CreateTrialLicense()
        {
            var trialExpirationDate = LicenseStore.GetTrialExpiration();

            Logger.InfoFormat("Configuring ServiceInsight to run in trial mode.");

            return new PlatformLicense
            {
                ExpirationDate = trialExpirationDate,
            };

        }

        SignedXmlVerifier xmlVerifier = new SignedXmlVerifier(PublicKey);

        public const string PublicKey = @"<RSAKeyValue><Modulus>5M9/p7N+JczIN/e5eObahxeCIe//2xRLA9YTam7zBrcUGt1UlnXqL0l/8uO8rsO5tl+tjjIV9bOTpDLfx0H03VJyxsE8BEpSVu48xujvI25+0mWRnk4V50bDZykCTS3Du0c8XvYj5jIKOHPtU//mKXVULhagT8GkAnNnMj9CvTc=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

        static readonly ILog Logger = LogManager.GetLogger(typeof(AppLicenseManager));


    }
}