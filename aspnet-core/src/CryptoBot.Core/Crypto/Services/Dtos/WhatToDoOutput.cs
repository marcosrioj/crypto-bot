﻿using CryptoBot.Crypto.Enums;

namespace CryptoBot.Crypto.Services.Dtos
{
    public class WhatToDoOutput
    {
        public EWhatToDo WhatToDo { get; set; }
        public decimal Score { get; set; }
    }
}
