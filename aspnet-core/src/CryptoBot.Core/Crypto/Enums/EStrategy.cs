namespace CryptoBot.Crypto.Enums
{
    public enum EStrategy
    {
        //Risk: 🐲 🐲 Possible reward: 💰 💰
        SimpleMeanReversionStrategy = 1,

        // Risk: 🐲 Possible reward: 💰
        SimpleMicrotrendStrategy = 3,

        // Risk: 🐲 🐲 🐲 Possible reward: 💰
        SimpleMlStrategy1 = 5,

        NormalMlStrategy1 = 7,

        NormalMlStrategy2 = 9,

        SimpleRsiStrategy = 11
    }
}
