using System;

namespace NServiceBus.Profiler.Desktop.Core.Licensing
{
    public class ProfilerLicense
    {
        public ProfilerLicense()
        {
            LicenseType = ProfilerLicenseTypes.Standard;
            RegisteredTo = "Unregistered User";
        }

        public DateTime? ExpirationDate { get; set; }

        public bool Expired { get; set; }

        public string LicenseType { get; set; }
        public string RegisteredTo { get; set; }
    }
}