using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Threading.BackgroundWorkers;
using Abp.Threading.Timers;
using Binance.Net;
using Binance.Net.Enums;
using Binance.Net.Objects.Spot;
using CryptoBot.Crypto.Enums;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Logging;
using Microsoft.Extensions.Logging;

namespace CryptoBot.Crypto.BackgroundWorker.QuoteHistory
{
    public class UpdateQuoteHistoryWorker : AsyncPeriodicBackgroundWorkerBase, ISingletonDependency
    {
        private const string BINANCE_KEY = "ogIoauli5sR4XhHUy0TdoUVLlpHjbQHXx283KW9ydYaalRkOg1cjxWBGVwMdPfKy";
        private const string BINANCE_SECRET = "HWSANdKZkBnXrfYfSDwnOWeCMddtKbGdk0KZb1sWycxDXkSPWLeFFk3RVUTQ1w2g";

        private readonly ILogger<UpdateQuoteHistoryWorker> _logger;
        private readonly IRepository<Entities.QuoteHistory, long> _quoteHistoryRepository;

        public UpdateQuoteHistoryWorker(
            AbpAsyncTimer timer, 
            IRepository<Entities.QuoteHistory, long> quoteHistoryRepository,
            ILogger<UpdateQuoteHistoryWorker> logger)
            : base(timer)
        {
            _quoteHistoryRepository = quoteHistoryRepository;
            Timer.Period = 60000; //60 seconds
            _logger = logger;
        }

        [UnitOfWork]
        protected override async Task DoWorkAsync()
        {
            SetBinanceClient();
            SetBinanceSocketClient();

            await MainAsync();
            //Test();

            //SocketExecute();
        }

        private async Task MainAsync()
        {
            var baseCoin = "USDT";
            var coins = Enum.GetValues(typeof(ECurrency)).Cast<ECurrency>();

            using (var client = new BinanceClient())
            {
                var momentRef = Guid.NewGuid();

                foreach (var coin in coins)
                {
                    Task task = AddQuoteHistory(baseCoin, client, momentRef, coin);
                }
            }
        }

        private async Task AddQuoteHistory(string baseCoin, BinanceClient client, Guid momentRef, ECurrency coin)
        {
            var curencyCoin = coin.ToString().ToUpper();
            var pair = $"{curencyCoin}{baseCoin}";

            var bookPrice = client.Spot.Market.GetBookPrice(pair);
            var bestAskPrice = bookPrice.Data.BestAskPrice;

            await _quoteHistoryRepository.InsertAsync(new Entities.QuoteHistory
            {
                Price = bestAskPrice,
                Currency = coin,
                CreatorUserId = 2,
                MomentReference = momentRef
            });
        }

        private static void Test()
        {
            using (var client = new BinanceClient())
            {
                // Spot.Market | Spot market info endpoints
                var bookPrice = client.Spot.Market.GetBookPrice("BTCUSDT");

                // Spot.Order | Spot order info endpoints
                var orders = client.Spot.Order.GetAllOrders("BTCUSDT");

                // Spot.System | Spot system endpoints
                var exchangeInfo = client.Spot.System.GetExchangeInfo();

                // Spot.UserStream | Spot user stream endpoints. Should be used to subscribe to a user stream with the socket client
                var startUserStream = client.Spot.UserStream.StartUserStream();

                // Spot.Futures | Transfer to/from spot from/to the futures account + cross-collateral endpoints
                var transferFuturesAccount = client.Spot.Futures.TransferFuturesAccount("ASSET", 1, FuturesTransferType.FromSpotToUsdtFutures);

                // FuturesCoin | Coin-M general endpoints
                var positionInformation = client.FuturesCoin.GetPositionInformation();

                // FuturesCoin.Market | Coin-M futures market endpoints
                var bookPrices = client.FuturesCoin.Market.GetBookPrices("BTCUSD");

                // FuturesCoin.Order | Coin-M futures order endpoints
                var myTrades = client.FuturesCoin.Order.GetMyTrades();

                // FuturesCoin.Account | Coin-M account info
                var accountInfo = client.FuturesCoin.Account.GetAccountInfo();

                // FuturesCoin.System | Coin-M system endpoints
                var exchangeInfoFuture = client.FuturesCoin.System.GetExchangeInfo();

                // FuturesCoin.UserStream | Coin-M user stream endpoints. Should be used to subscribe to a user stream with the socket client
                var startUserStreamFuture = client.FuturesCoin.UserStream.StartUserStream();

                // FuturesUsdt | USDT-M general endpoints
                var positionInformationFuture = client.FuturesUsdt.GetPositionInformation();

                // FuturesUsdt.Market | USDT-M futures market endpoints
                var bookPricesFuture = client.FuturesUsdt.Market.GetBookPrices("BTCUSDT");

                // FuturesUsdt.Order | USDT-M futures order endpoints
                var myTradesFuture = client.FuturesUsdt.Order.GetMyTrades("BTCUSDT");

                // FuturesUsdt.Account | USDT-M account info
                var accountInfoFutureUsdt = client.FuturesUsdt.Account.GetAccountInfo();

                // FuturesUsdt.System | USDT-M system endpoints
                var exchangeInfoFutureUsdt = client.FuturesUsdt.System.GetExchangeInfo();

                // FuturesUsdt.UserStream | USDT-M user stream endpoints. Should be used to subscribe to a user stream with the socket client
                var startUserStreamFutureUsdt = client.FuturesUsdt.UserStream.StartUserStream();

                // General | General/account endpoints
                var accountInfoGeneral = client.General.GetAccountInfo();

                // Lending | Lending endpoints
                var flexibleProductList = client.Lending.GetFlexibleProductList();

                // Margin | Margin general/account info
                var marginAccountInfo = client.Margin.GetMarginAccountInfo();

                // Margin.Market | Margin market endpoints
                var marginPairs = client.Margin.Market.GetMarginPairs();

                // Margin.Order | Margin order endpoints
                var marginAccountOrders = client.Margin.Order.GetAllMarginAccountOrders("BTCUSDT");

                // Margin.UserStream | Margin user stream endpoints. Should be used to subscribe to a user stream with the socket client
                var startUserStreamMargin = client.Margin.UserStream.StartUserStream();

                // Margin.IsolatedUserStream | Isolated margin user stream endpoints. Should be used to subscribe to a user stream with the socket client
                var startIsolatedMarginUserStream = client.Margin.IsolatedUserStream.StartIsolatedMarginUserStream("BTCUSDT");


                // Mining | Mining endpoints
                var miningCoinList = client.Mining.GetMiningCoinList();

                // SubAccount | Sub account management
                //var result1 = client.SubAccount.TransferSubAccount("fromEmail", "toEmail", "asset", 1);

                // Brokerage | Brokerage management
                //var result2 = client.Brokerage.CreateSubAccountAsync();

                // WithdrawDeposit | Withdraw and deposit endpoints
                //var result3 = client.WithdrawDeposit.GetWithdrawalHistory();
            }
        }

        private static void SocketExecute()
        {
            var socketClient = new BinanceSocketClient();
            // Spot | Spot market and user subscription methods
            socketClient.Spot.SubscribeToAllBookTickerUpdates(data =>
            {
                // Handle data
            });

            // FuturesCoin | Coin-M futures market and user subscription methods
            socketClient.FuturesCoin.SubscribeToAllBookTickerUpdates(data =>
            {
                // Handle data
            });

            // FuturesUsdt | USDT-M futures market and user subscription methods
            socketClient.FuturesUsdt.SubscribeToAllBookTickerUpdates(data =>
            {
                // Handle data
            });

            // Unsubscribe
            socketClient.UnsubscribeAll();
        }

        private static void SetBinanceSocketClient()
        {
            BinanceSocketClient.SetDefaultOptions(new BinanceSocketClientOptions()
            {
                ApiCredentials = new ApiCredentials(BINANCE_KEY, BINANCE_SECRET),
                LogVerbosity = LogVerbosity.Debug,
                LogWriters = new List<TextWriter> {Console.Out}
            });
        }

        private static void SetBinanceClient()
        {
            BinanceClient.SetDefaultOptions(new BinanceClientOptions()
            {
                ApiCredentials = new ApiCredentials(BINANCE_KEY, BINANCE_SECRET),
                LogVerbosity = LogVerbosity.Debug,
                LogWriters = new List<TextWriter> {Console.Out}
            });
        }
    }
}
