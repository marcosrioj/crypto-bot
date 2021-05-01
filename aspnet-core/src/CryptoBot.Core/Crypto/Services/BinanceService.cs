using Abp.Configuration;
using Abp.Domain.Services;
using Binance.Net;
using Binance.Net.Enums;
using Binance.Net.Interfaces;
using Binance.Net.Objects.Spot;
using Binance.Net.Objects.Spot.MarketData;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Logging;
using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CryptoBot.Crypto.Enums;
using CryptoBot.Crypto.Helpers;

namespace CryptoBot.Crypto.Services
{
    public class BinanceService : DomainService, IBinanceService
    {
        private readonly ISettingManager _settingManager;
        private IBinanceClient _binanceClient;

        private bool _isBinanceClientSet = false;

        public BinanceService(ISettingManager settingManager)
        {
            _settingManager = settingManager;
        }

        public BinanceBookPrice GetBookPrice(long userId, string pair, ETradingType tradingType)
        {
            SetBinanceClients(userId);

            if (tradingType == ETradingType.Spot)
            {
                var bookPrice = _binanceClient.Spot.Market.GetBookPrice(pair);

                if (bookPrice.Data != null)
                {
                    return bookPrice.Data;
                }

                return null;
            } 
            else
            {
                var bookPrice = _binanceClient.FuturesUsdt.Market.GetBookPrices(pair);
                return bookPrice.Data.FirstOrDefault();
            }
        }

        public WebCallResult<IEnumerable<IBinanceKline>> GetKlines(string pair, KlineInterval interval, ETradingType tradingType, int limit = 100, long? userId = null, DateTime? startTime = null, DateTime? endTime = null)
        {
            SetBinanceClients(userId);

            if (tradingType == ETradingType.Spot)
            {
                return _binanceClient.Spot.Market.GetKlines(pair, interval, startTime, endTime, limit);
            }

            return _binanceClient.FuturesUsdt.Market.GetKlines(pair, interval, startTime, endTime, limit);
        }

        public IBinanceKline GetKline(string pair, ETradingType tradingType, long? userId = null)
        {
            SetBinanceClients(userId);

            WebCallResult<IEnumerable<IBinanceKline>> result = null;
            
            if (tradingType == ETradingType.Spot)
            {
                result = _binanceClient.Spot.Market.GetKlines(pair, KlineInterval.OneMinute, limit: 1);
            }
            else
            {
                result = _binanceClient.FuturesUsdt.Market.GetKlines(pair, KlineInterval.OneMinute, limit: 1);
            }

            return result.Data.FirstOrDefault();
        }

        public WebCallResult<BinanceOrderBook> GetBookOrders(long userId, string pair, ETradingType tradingType, int? limit = null)
        {
            SetBinanceClients(userId);

            using (var client = new BinanceClient())
            {
                WebCallResult<BinanceOrderBook> bookOrders = null;

                if (tradingType == ETradingType.Spot)
                {
                    bookOrders = client.Spot.Market.GetOrderBook(pair, limit);
                } 
                else
                {
                    bookOrders = client.FuturesUsdt.Market.GetOrderBook(pair, limit);
                }

                return bookOrders;
            }
        }

        public List<IBinanceKline> GetData(
            ECurrency currency,
            KlineInterval interval,
            ETradingType tradingType,
            DateTime? startTime,
            DateTime? endTime,
            int limitOfDetails,
            long? userId = null)
        {
            var inputData = new List<IBinanceKline>();
            var pair = $"{currency}{CryptoBotConsts.BaseCoinName}";

            var initialLimitOfDetails = limitOfDetails > CryptoBotConsts.BinanceApiItemLimit
                ? CryptoBotConsts.BinanceApiItemLimit
                : limitOfDetails;

            var klinesResult = GetKlines(pair, interval, tradingType, initialLimitOfDetails, userId, startTime, endTime);

            if (klinesResult.Success)
            {
                inputData.AddRange(klinesResult.Data);

                var itemsToDo = limitOfDetails - CryptoBotConsts.BinanceApiItemLimit;

                while (itemsToDo > 0)
                {
                    var newLimitOfDetails = itemsToDo > CryptoBotConsts.BinanceApiItemLimit
                        ? CryptoBotConsts.BinanceApiItemLimit
                        : itemsToDo;

                    var endTimeItem = inputData.First().OpenTime.AddSeconds(-1);
                    var startTimeItem = DatetimeHelper.GetDateMinusIntervalMultipliedByLimit(endTimeItem, interval, newLimitOfDetails);

                    var klinesResultItem = GetKlines(pair, interval, tradingType, newLimitOfDetails, userId, startTimeItem, endTimeItem);

                    if (klinesResultItem.Success)
                    {
                        inputData.InsertRange(0, klinesResultItem.Data);

                        itemsToDo = itemsToDo - CryptoBotConsts.BinanceApiItemLimit;
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            if (inputData.Last().CloseTime > DateTime.UtcNow) // Fix binance data. Sometimes come from future. Ex 5m: Now: 12:20:07 - DataFromBinance 12:24:59 
            {
                inputData.RemoveAt(inputData.Count - 1);
            }

            return inputData;
        }

        public void Samples()
        {
            SetBinanceClients(2);

            using (var client = new BinanceClient())
            {
                // Spot.Market | Spot market info endpoints
                var bookPrice = client.Spot.Market.GetBookPrice("BTCUSDT");

                // Spot.Order | Spot order info endpoints
                var orders = client.Spot.Order.GetAllOrders("BTCUSDT");

                // 
                var bookOrders = client.Spot.Market.GetOrderBook("BTCUSDT");

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

        private void SocketExecuteSample()
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

        private void SetBinanceClients(long? userId = null)
        {
            if (!_isBinanceClientSet)
            {
                var userIdValue = userId.HasValue
                    ? userId.Value
                    : CryptoBotConsts.DefaultUserId;

                var binanceKey = _settingManager.GetSettingValueForUser(CryptoBotNames.BinanceKey, null, userIdValue);
                var binanceSecret = _settingManager.GetSettingValueForUser(CryptoBotNames.BinanceSecret, null, userIdValue);

                BinanceClient.SetDefaultOptions(new BinanceClientOptions()
                {
                    ApiCredentials = new ApiCredentials(binanceKey, binanceSecret),
                    LogVerbosity = LogVerbosity.Debug,
                    LogWriters = new List<TextWriter> { Console.Out }
                });

                BinanceSocketClient.SetDefaultOptions(new BinanceSocketClientOptions()
                {
                    ApiCredentials = new ApiCredentials(binanceKey, binanceSecret),
                    LogVerbosity = LogVerbosity.Debug,
                    LogWriters = new List<TextWriter> { Console.Out }
                });
            }

            if (_binanceClient == null)
            {
                _binanceClient = new BinanceClient();
            }
        }
    }
}
