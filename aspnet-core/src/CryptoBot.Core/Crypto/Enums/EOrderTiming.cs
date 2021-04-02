namespace CryptoBot.Crypto.Enums
{
    public enum EOrderTiming
    {
        //
        // Summary:
        //     The order is good for the day, and it will be canceled automatically at the end
        //     of market hours.
        Day = 0,
        //
        // Summary:
        //     The order is good until canceled.
        GoodTillCanceled = 1,
        //
        // Summary:
        //     The order is placed at the time the market opens.
        AtMarketOpen = 2,
        //
        // Summary:
        //     The order is immediately filled or canceled after being placed (may partial fill).
        PartialFillOrKill = 3,
        //
        // Summary:
        //     The order is immediately filled or canceled after being placed (may not partial
        //     fill).
        FillOrKill = 4,
        //
        // Summary:
        //     The order will become a limit order if a limit price is specified or a market
        //     order otherwise at market close.
        AtMarketClose = 5
    }
}