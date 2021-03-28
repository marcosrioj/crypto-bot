using Abp.Domain.Repositories;
using Abp.Domain.Services;
using Binance.Net.Enums;
using Binance.Net.Interfaces;
using CryptoBot.Crypto.Entities;
using CryptoBot.Crypto.Enums;
using CryptoBot.Crypto.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CryptoBot.Crypto.Services
{
    public class ComparativeHistoricalService : DomainService, IComparativeHistoricalService
    {
        private readonly IRepository<ComparativeHistorical, long> _chRepository;
        private readonly IComparativeHistoricalDetailRepository _chdRepository;
        private readonly IRepository<GroupComparativeHistorical, Guid> _gchRepository;
        private readonly IBinanceService _binanceService;
        private readonly ICurrencyService _currencyService;

        public ComparativeHistoricalService(
            IRepository<ComparativeHistorical, long> chRepository,
            IRepository<GroupComparativeHistorical, Guid> gchRepository,
            IComparativeHistoricalDetailRepository chdRepository,
            IBinanceService binanceService,
            ICurrencyService currencyService)
        {
            _chRepository = chRepository;
            _gchRepository = gchRepository;
            _chdRepository = chdRepository;
            _binanceService = binanceService;
            _currencyService = currencyService;
        }

        public async Task<Guid?> GenerateGroupComparativeHistorical(
            EApproachTrading approachTrading,
            ECurrency currency,
            KlineInterval interval,
            int limitOfDetails = 100,
            long? userId = null,
            IEnumerable<Tuple<DateTime, DateTime>> periods = null)
        {
            var userIdValue = userId.HasValue
                ? userId.Value
                : CryptoBotConsts.DefaultUserId;

            var gch = new GroupComparativeHistorical
            {
                ApproachTrading = approachTrading,
                Currency = currency,
                Interval = interval,
                LimitOfDetails = limitOfDetails,
                CreatorUserId = userIdValue
            };

            await _gchRepository.InsertAsync(gch);

            if (periods != null)
            {
                foreach (var period in periods)
                {
                    var startTime = period.Item1;
                    var endTime = period.Item2;

                    var result = await GenerateIndividualComparativeHistorical(gch, startTime, endTime);

                    if (!result)
                    {
                        return null;
                    }
                }
            } 
            else
            {
                var result = await GenerateIndividualComparativeHistorical(gch);

                if (!result)
                {
                    return null;
                }
            }

            return gch.Id;
        }

        private async Task<bool> GenerateIndividualComparativeHistorical(
            GroupComparativeHistorical groupCh,
            DateTime? startTime = null,
            DateTime? endTime = null)
        {
            IEnumerable<ECurrency> coins = await _currencyService.GetActiveCurrencies();

            var ch = new ComparativeHistorical
            {
                GroupId = groupCh.Id,
                StartDateTime = startTime,
                EndDateTime = endTime
            };

            var chId = await _chRepository.InsertAndGetIdAsync(ch);

            var currencyCoin = groupCh.Currency.ToString().ToUpper();
            var pair = $"{currencyCoin}{CryptoBotConsts.BaseCoinName}";

            var klinesResult = _binanceService.GetKlines(pair, groupCh.Interval, groupCh.LimitOfDetails, startTime, endTime);

            if (klinesResult.Success)
            {
                var details = new List<ComparativeHistoricalDetail>();
                IBinanceKline firstKline = null;
                IBinanceKline currentKline = null;

                foreach (var kline in klinesResult.Data)
                {
                    var percGainFromFirstDetail = firstKline == null
                        ? 0
                        : (1 * kline.Close / firstKline.Close) - 1;

                    var percGainFromPreviousDetail = firstKline == null
                        ? 0
                        : (1 * kline.Close / currentKline.Close) - 1;

                    var detail = new ComparativeHistoricalDetail
                    {
                        ComparativeHistoricalId = chId,
                        PercentageGainFromFirstDetail = percGainFromFirstDetail,
                        PercentageGainFromPreviousDetail = percGainFromPreviousDetail,
                        Amount = kline.Close,
                        Moment = kline.CloseTime
                    };

                    details.Add(detail);

                    currentKline = kline;

                    if (firstKline == null)
                    {
                        firstKline = kline;
                    }
                }

                return _chdRepository.BulkImport(details);
            }

            return false;
        }
    }
}
