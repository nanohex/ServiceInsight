namespace NServiceBus.Profiler.Desktop.Core.Licensing
{
    using System;
    using System.Collections.Generic;
    using log4net;
    using Rhino.Licensing;

    public class AppLicenseManager : ILicenseManager
    {
        public AppLicenseManager()
        {
            Initialize();
        }

        public void Initialize(string license = null)
        {
            validator = CreateValidator(license);
            Validate(license);
        }

        public ProfilerLicense CurrentLicense { get; private set; }

        public bool TrialExpired { get; private set; }


        public int GetRemainingTrialDays()
        {
            var now = DateTime.UtcNow.Date;

            var expiration = LicenseDescriptor.GetTrialExpirationFromRegistry();

            var remainingDays = (expiration - now).Days;

            return remainingDays > 0 ? remainingDays : 0;
        }

        void Validate(string license)
        {
            if (validator != null)
            {
                try
                {
                    validator.AssertValidLicense();

                    Logger.InfoFormat("Found a {0} license.", validator.LicenseType);
                    Logger.InfoFormat("Registered to {0}", validator.Name);
                    Logger.InfoFormat("Expires on {0}", validator.ExpirationDate);
                    if ((validator.LicenseAttributes != null) && (validator.LicenseAttributes.Count > 0))
                        foreach (var licenseAttribute in validator.LicenseAttributes)
                            Logger.InfoFormat("[{0}]: [{1}]", licenseAttribute.Key, licenseAttribute.Value);

                    ValidateLicenseVersion();
                    CreateLicense();
                    StoreLicense(license);
                }
                catch (InvalidLicenseException)
                {
                    TrialExpired = true;
                    Logger.Info("Suitable license was not found");
                }
                catch (LicenseExpiredException)
                {
                    TrialExpired = true;
                    Logger.Info("License has expired.");
                }
                catch (LicenseNotFoundException)
                {
                    Logger.Info("License could not be loaded.");
                }
                catch (LicenseFileNotFoundException)
                {
                    Logger.Info("License could not be loaded.");
                }
            }

            if (CurrentLicense == null)
            {
                Logger.Info("No valid license found.");
                CreateTrialLicense();
            }
        }

        void StoreLicense(string license)
        {
            if (!string.IsNullOrEmpty(license))
            {
                LicenseDescriptor.License = license;
            }
        }

        void CreateTrialLicense()
        {
            var trialExpirationDate = LicenseDescriptor.GetTrialExpirationFromRegistry();

            if (trialExpirationDate > DateTime.UtcNow.Date)
            {
                Logger.InfoFormat("Trial for ServiceInsight v{0} is still active, trial expires on {1}.",
                                   LicenseDescriptor.SoftwareVersion,
                                   trialExpirationDate.ToLocalTime().ToShortDateString());

                Logger.InfoFormat("Configuring ServiceInsight to run in trial mode.");

                CurrentLicense = new ProfilerLicense
                {
                    LicenseType = ProfilerLicenseTypes.Trial,
                    ExpirationDate = trialExpirationDate,
                    Version = LicenseDescriptor.SoftwareVersion,
                    RegisteredTo = ProfilerLicense.UnRegisteredUser
                };
            }
            else
            {
                Logger.WarnFormat("Trial for ServiceInsight v{0} has expired.", LicenseDescriptor.SoftwareVersion);

                TrialExpired = true;
            }
        }

     

        AbstractLicenseValidator CreateValidator(string license = null)
        {
            if (license == null && !string.IsNullOrEmpty(LicenseDescriptor.License))
            {
                Logger.InfoFormat(@"Using embeded license found in registry [{0}\License].", LicenseDescriptor.RegistryKey);
                license = LicenseDescriptor.License;
            }

            return new StringLicenseValidator(LicenseDescriptor.PublicKey, license);
        }

        void ValidateLicenseVersion()
        {
            if (validator.LicenseType == LicenseType.None)
                return;

            if (validator.LicenseAttributes.ContainsKey(LicenseVersionKey))
            {
                try
                {
                    var semver = LicenseDescriptor.ApplicationVersion;
                    var licenseVersion = Version.Parse(validator.LicenseAttributes[LicenseVersionKey]);
                    if (licenseVersion >= semver)
                        return;
                }
                catch (Exception exception)
                {
                    throw new InvalidLicenseException(InvalidLicenseVersionMessage, exception);
                }
            }

            throw new InvalidLicenseException(InvalidLicenseVersionMessage);
        }

        void CreateLicense()
        {
            CurrentLicense = new ProfilerLicense();

            switch (validator.LicenseType)
            {
                case LicenseType.None:
                    CurrentLicense.LicenseType = ProfilerLicenseTypes.Trial;
                    break;
                case LicenseType.Standard:
                    SetLicenseType(ProfilerLicenseTypes.Standard);
                    break;
                case LicenseType.Trial:
                    SetLicenseType(ProfilerLicenseTypes.Trial);
                    break;
                default:
                    Logger.Error(string.Format("Got unexpected license type [{0}], setting Basic1 free license type.", validator.LicenseType), null);
                    CurrentLicense.LicenseType = ProfilerLicenseTypes.Trial;
                    break;
            }

            CurrentLicense.ExpirationDate = validator.ExpirationDate;
            ConfigureLicenseBasedOnAttribute(validator.LicenseAttributes);
        }

        void ConfigureLicenseBasedOnAttribute(IDictionary<string, string> attributes)
        {
            CurrentLicense.Version = attributes[LicenseVersionKey];
            CurrentLicense.RegisteredTo = validator.Name;
        }

        void SetLicenseType(string defaultLicenseType)
        {
            if ((validator.LicenseAttributes == null) ||
                (!validator.LicenseAttributes.ContainsKey(LicenseTypeKey)) ||
                (string.IsNullOrEmpty(validator.LicenseAttributes[LicenseTypeKey])))
            {
                CurrentLicense.LicenseType = defaultLicenseType;
            }
            else
            {
                CurrentLicense.LicenseType = validator.LicenseAttributes[LicenseTypeKey];
            }
        }

        AbstractLicenseValidator validator;

        const string InvalidLicenseVersionMessage = "Your license is valid for an older version of ServiceInsight. If you are still within the 1 year upgrade protection period of your original license, you should have already received a new license and if you haven’t, please contact customer.care@particular.net If your upgrade protection has lapsed, you can renew it at http://particular.net/support";
        const string LicenseTypeKey = "LicenseType";
        const string LicenseVersionKey = "LicenseVersion";
        const string PublicKey = @"<RSAKeyValue><Modulus>5M9/p7N+JczIN/e5eObahxeCIe//2xRLA9YTam7zBrcUGt1UlnXqL0l/8uO8rsO5tl+tjjIV9bOTpDLfx0H03VJyxsE8BEpSVu48xujvI25+0mWRnk4V50bDZykCTS3Du0c8XvYj5jIKOHPtU//mKXVULhagT8GkAnNnMj9CvTc=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

        static readonly ILog Logger = LogManager.GetLogger(typeof(ILicenseManager));

    }
}