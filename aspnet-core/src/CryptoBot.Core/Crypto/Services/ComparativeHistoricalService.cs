using Abp.Domain.Repositories;
using Abp.Domain.Services;
using CryptoBot.Crypto.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CryptoBot.Crypto.Services
{
    public class ComparativeHistoricalService : DomainService, IComparativeHistoricalService
    {
        private readonly IRepository<Entities.QuoteHistory, long> _quoteHistoryRepository;
        private readonly IBinanceService _binanceService;
        private readonly ICurrencyService _currencyService;

        public ComparativeHistoricalService(
            IRepository<Entities.QuoteHistory, long> quoteHistoryRepository,
            IBinanceService binanceService,
            ICurrencyService currencyService)
        {
            _quoteHistoryRepository = quoteHistoryRepository;
            _binanceService = binanceService;
            _currencyService = currencyService;
        }

        public async Task GenerateIndividualComparativeHistorical()
        {
            IEnumerable<ECurrency> coins = await _currencyService.GetActiveCurrencies();


        }

        public async Task GenerateGroupComparativeHistorical()
        {
            IEnumerable<ECurrency> coins = await _currencyService.GetActiveCurrencies();


        }

        
    }
}
