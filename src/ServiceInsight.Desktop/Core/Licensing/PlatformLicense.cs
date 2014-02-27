using System;

namespace NServiceBus.Profiler.Desktop.Core.Licensing
{
    public class PlatformLicense
    {
        public PlatformLicense()
        {
            LicenseType = "Trial";
        }

        public DateTime? ExpirationDate { get; set; }

        public bool Expired
        {
            get
            {
                if (ExpirationDate.HasValue)
                {
                    return DateTime.Today > ExpirationDate.Value;
                }

                return true;
            }
        }

        public bool IsTrialLicense
        {
            get { return !IsCommercialLicense; }
        }

        public bool IsCommercialLicense
        {
            get { return LicenseType.ToLower() != "trial"; }
        }

        public string LicenseType { get; set; }

        public string RegisteredTo { get; set; }

        public DateTime? UpgradeProtectionExpiration { get; set; }
    }
}