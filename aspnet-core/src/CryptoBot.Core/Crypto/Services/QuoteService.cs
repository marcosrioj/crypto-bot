using Abp.Domain.Repositories;
using Abp.Domain.Services;
using CryptoBot.Crypto.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CryptoBot.Crypto.Services
{
    public class QuoteService : DomainService, IQuoteService
    {
        private readonly IRepository<Entities.QuoteHistory, long> _quoteHistoryRepository;
        private readonly IBinanceService _binanceService;
        private readonly ICurrencyService _currencyService;

        public QuoteService(
            IRepository<Entities.QuoteHistory, long> quoteHistoryRepository,
            IBinanceService binanceService,
            ICurrencyService currencyService)
        {
            _quoteHistoryRepository = quoteHistoryRepository;
            _binanceService = binanceService;
            _currencyService = currencyService;
        }

        public async Task AddCurrentQuoteHistoryAsync()
        {
            IEnumerable<ECurrency> coins = await _currencyService.GetActiveCurrencies();

            var momentRef = Guid.NewGuid();

            foreach (var coin in coins)
            {
                var curencyCoin = coin.ToString().ToUpper();
                var pair = $"{curencyCoin}{CryptoBotConsts.BaseCoinName}";

                var bookPrice = _binanceService.GetBookPrice(pair);
                var bestAskPrice = bookPrice.Data.BestAskPrice;

                await _quoteHistoryRepository.InsertAsync(new Entities.QuoteHistory
                {
                    Price = bestAskPrice,
                    Currency = coin,
                    CreatorUserId = 2,
                    MomentReference = momentRef
                });
            }
        }
    }
}
