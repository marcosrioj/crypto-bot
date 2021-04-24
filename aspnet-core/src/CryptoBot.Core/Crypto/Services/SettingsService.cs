using Abp.Configuration;
using Abp.Domain.Services;
using CryptoBot.Crypto.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CryptoBot.Crypto.Services
{
    public class SettingsService : DomainService, ISettingsService
    {
        private readonly ISettingManager _settingManager;

        public SettingsService(ISettingManager settingManager)
        {
            _settingManager = settingManager;
        }

        //TODO Apply on strategies
        public float GetInvestorProfileFactor(EStrategy strategy, EInvestorProfile investorProfile = EInvestorProfile.UltraConservative)
        {
            //TODO Put in the Settings Table
            switch (strategy)
            {
                case EStrategy.SimpleMeanReversionStrategy:
                    return 20;

                case EStrategy.SimpleMicrotrendStrategy:
                    switch (investorProfile)
                    {
                        // Numbers of the last values has to be greater than next one
                        case EInvestorProfile.UltraAggressive:
                            return 3;
                        case EInvestorProfile.Aggressive:
                            return 4;
                        case EInvestorProfile.Moderate:
                            return 5;
                        case EInvestorProfile.Conservative:
                            return 6;
                        case EInvestorProfile.UltraConservative:
                            return 7;
                        default:
                            throw new ArgumentException("Invalid investor profile");
                    }

                case EStrategy.SimpleMlStrategy1:
                    switch (investorProfile)
                    {
                        //Score
                        case EInvestorProfile.UltraAggressive:
                            return 0;
                        case EInvestorProfile.Aggressive:
                            return 0.0025f;
                        case EInvestorProfile.Moderate:
                            return 0.005f;
                        case EInvestorProfile.Conservative:
                            return 0.0075f;
                        case EInvestorProfile.UltraConservative:
                            return 0.01f;
                        default:
                            throw new ArgumentException("Invalid investor profile");
                    }

                case EStrategy.NormalMlStrategy1:
                    switch (investorProfile)
                    {
                        //Percent of real value of prediction Sample: 1%, 5% etc.
                        case EInvestorProfile.UltraAggressive:
                            return 0;
                        case EInvestorProfile.Aggressive:
                            return 0.01f;
                        case EInvestorProfile.Moderate:
                            return 0.02f;
                        case EInvestorProfile.Conservative:
                            return 0.03f;
                        case EInvestorProfile.UltraConservative:
                            return 0.04f;
                        default:
                            throw new ArgumentException("Invalid investor profile");
                    }

                //TODO verify levels
                case EStrategy.NormalMlStrategy2:
                    switch (investorProfile)
                    {
                        //Percent of real value of prediction Sample: 1%, 5% etc.
                        case EInvestorProfile.UltraAggressive:
                            return 0;
                        case EInvestorProfile.Aggressive:
                            return 0.1f;
                        case EInvestorProfile.Moderate:
                            return 0.2f;
                        case EInvestorProfile.Conservative:
                            return 0.3f;
                        case EInvestorProfile.UltraConservative:
                            return 0.4f;
                        default:
                            throw new ArgumentException("Invalid investor profile");
                    }
                default:
                    throw new ArgumentException("Invalid strategy");
            }
        }
    }
}
