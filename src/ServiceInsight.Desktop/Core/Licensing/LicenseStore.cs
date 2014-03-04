using System;
using Microsoft.Win32;

namespace NServiceBus.Profiler.Desktop.Core.Licensing
{
    using System.Globalization;
    using log4net;

    public class LicenseStore
    {
        public static DateTime GetTrialExpiration()
        {
            //If first time run, configure expire date
            try
            {
                using (var registryKey = Registry.CurrentUser.CreateSubKey(RegistryKey))
                {
                    //CreateSubKey does not return null http://stackoverflow.com/questions/19849870/under-what-circumstances-will-registrykey-createsubkeystring-return-null
                    // ReSharper disable once PossibleNullReferenceException
                    var trialStartDateString = (string)registryKey.GetValue("TrialStart", null);
                    if (trialStartDateString == null)
                    {
                        var trialStart = DateTime.UtcNow;
                        trialStartDateString = trialStart.ToString("yyyy-MM-dd");
                        registryKey.SetValue("TrialStart", trialStartDateString, RegistryValueKind.String);

                        Logger.DebugFormat("First time running the platform, setting trial license start.");
                        return trialStart.AddDays(TRIAL_DAYS);
                    }
                    else
                    {
                        var trialStartDate = DateTimeOffset.ParseExact(trialStartDateString, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);

                        return trialStartDate.Date.AddDays(TRIAL_DAYS);

                    }
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.Debug("Could not access registry to check trial expiration date. Because we didn't find a license file we assume the trial has expired.", ex);
                return DateTime.MinValue;
            }
        }

        public static string License
        {
            get
            {

                using (var registryKey = Registry.LocalMachine.OpenSubKey(RegistryKey))
                {
                    if (registryKey != null)
                    {
                        var licenseValue = (string)registryKey.GetValue("License", null);

                        if (!string.IsNullOrEmpty(licenseValue))
                        {
                            return licenseValue;
                        }
                    }
                }

                
                using (var registryKey = Registry.CurrentUser.OpenSubKey(RegistryKey))
                {
                    if (registryKey != null)
                    {
                        return (string) registryKey.GetValue("License", null);
                    }
                }

                return null;
            }
            set
            {
                using (var registryKey = Registry.CurrentUser.CreateSubKey(RegistryKey))
                {
                    if (registryKey != null)
                    {
                        registryKey.SetValue("License", value, RegistryValueKind.String);
                    }
                }
            }
        }

        static string RegistryKey = @"SOFTWARE\ParticularSoftware"; 
        


        static readonly ILog Logger = LogManager.GetLogger(typeof(LicenseStore));
        const int TRIAL_DAYS = 14;
    }
}