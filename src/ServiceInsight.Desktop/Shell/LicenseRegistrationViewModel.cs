﻿namespace NServiceBus.Profiler.Desktop.Shell
{
    using System.IO;
    using Caliburn.PresentationFramework.Screens;
    using Core;
    using Core.Licensing;
    using ScreenManager;
    
    public class LicenseRegistrationViewModel : Screen, ILicenseRegistrationViewModel
    {
        private readonly AppLicenseManager licenseManager;
        private readonly IDialogManager _dialogManager;
        private readonly INetworkOperations _network;

        public const string LicensingPageUrl = "http://particular.net/licensing";

        public LicenseRegistrationViewModel(
            AppLicenseManager licenseManager, 
            IDialogManager dialogManager,
            INetworkOperations network)
        {
            this.licenseManager = licenseManager;
            _dialogManager = dialogManager;
            _network = network;
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            DisplayName = GetScreenTitle();
        }

        private string GetScreenTitle()
        {
            if (HasRemainingTrial) return string.Format("ServiceInsight - {0} day(s) left on your free trial", TrialDaysRemaining);
            if(HasFullLicense) return "ServiceInsight";

            return string.Format("ServiceInsight - {0} Trial Expired", licenseManager.CurrentLicense.IsExtendedTrial ? "Final" : "Initial");
        }


        public string LicenseType
        {
            get { return licenseManager.CurrentLicense.LicenseType; }
        }
        
        public string RegisteredTo
        {
            get { return licenseManager.CurrentLicense.RegisteredTo; }
        }

        public int TrialDaysRemaining
        {
            get { return licenseManager.GetRemainingTrialDays(); }
        }

        public bool HasTrialLicense
        {
            get { return licenseManager.CurrentLicense.IsTrialLicense; }
        }


        public bool HasFullLicense
        {
            get { return licenseManager.CurrentLicense.IsCommercialLicense; }
        }

        public bool HasRemainingTrial
        {
            get { return HasTrialLicense && TrialDaysRemaining > 0; }
        }

        public bool AllowedToUse
        {
            get { return HasRemainingTrial || HasFullLicense; }
        }

        public void OnLicenseChanged()
        {
            NotifyOfPropertyChange(() => LicenseType);
            NotifyOfPropertyChange(() => RegisteredTo);
            NotifyOfPropertyChange(() => TrialDaysRemaining);
        }

        public void LoadLicense()
        {
            var dialog = _dialogManager.OpenFileDialog(new FileDialogModel
            {
                Filter = "License files (*.xml)|*.xml|All files (*.*)|*.*",
                FilterIndex = 1
            });

            var validLicense = false;

            if (dialog.Result.GetValueOrDefault(false))
            {
                var licenseContent = ReadAllTextWithoutLocking(dialog.FileName);

                validLicense = licenseManager.TryInstallLicense(licenseContent);
            }

            if (validLicense && !licenseManager.CurrentLicense.Expired)
            {
                TryClose(true);
            }
            else
            {
                //todo: Display error saying that the license was invalid
            }
        }

        public void Close()
        {
            TryClose(AllowedToUse);
        }

        public void Purchase()
        {
            _network.Browse(LicensingPageUrl);
        }

        private string ReadAllTextWithoutLocking(string path)
        {
            using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var textReader = new StreamReader(fileStream))
            {
                return textReader.ReadToEnd();
            }
        }
    }

    public interface ILicenseRegistrationViewModel : IScreen
    {
        string LicenseType { get; }
        string RegisteredTo { get; }
        int TrialDaysRemaining { get; }
        bool HasTrialLicense { get; }
        void LoadLicense();
        void Close();
    }
}