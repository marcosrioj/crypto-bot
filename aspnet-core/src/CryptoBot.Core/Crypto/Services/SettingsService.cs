﻿using Abp.Configuration;
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

        public float GetInvestorProfileFactor(EStrategy strategy, EProfitWay profitWay, EInvestorProfile investorProfile = EInvestorProfile.UltraConservative)
        {
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
                        //TODO LOSS PROFIT
                        //Score
                        case EInvestorProfile.UltraAggressive:
                            return 0;
                        case EInvestorProfile.Aggressive:
                            return profitWay == EProfitWay.ProfitFromGain ? 0.0025f : -0.005f;
                        case EInvestorProfile.Moderate:
                            return profitWay == EProfitWay.ProfitFromGain ? 0.005f : -0.01f;
                        case EInvestorProfile.Conservative:
                            return profitWay == EProfitWay.ProfitFromGain ? 0.0075f : -0.015f;
                        case EInvestorProfile.UltraConservative:
                            return profitWay == EProfitWay.ProfitFromGain ? 0.01f : -0.02f;
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
                            return profitWay == EProfitWay.ProfitFromGain ? 0.0003f : -0.0003f;
                        case EInvestorProfile.Moderate:
                            return profitWay == EProfitWay.ProfitFromGain ? 0.0006f : -0.0006f;
                        case EInvestorProfile.Conservative:
                            return profitWay == EProfitWay.ProfitFromGain ? 0.0009f : -0.0009f;
                        case EInvestorProfile.UltraConservative:
                            return profitWay == EProfitWay.ProfitFromGain ? 0.0012f : -0.0012f;
                        default:
                            throw new ArgumentException("Invalid investor profile");
                    }

                case EStrategy.NormalMlStrategy2:
                    switch (investorProfile)
                    {
                        //TODO LOSS PROFIT
                        //Percent of real value of prediction Sample: 1%, 5% etc.
                        case EInvestorProfile.UltraAggressive:
                            return 0;
                        case EInvestorProfile.Aggressive:
                            return profitWay == EProfitWay.ProfitFromGain ? 0.075f : -0.075f;
                        case EInvestorProfile.Moderate:
                            return profitWay == EProfitWay.ProfitFromGain ? 0.15f : -0.15f;
                        case EInvestorProfile.Conservative:
                            return profitWay == EProfitWay.ProfitFromGain ? 0.225f : -0.225f;
                        case EInvestorProfile.UltraConservative:
                            return profitWay == EProfitWay.ProfitFromGain ? 0.3f : -0.3f;
                        default:
                            throw new ArgumentException("Invalid investor profile");
                    }
                default:
                    throw new ArgumentException("Invalid strategy");
            }
        }
    }
}
