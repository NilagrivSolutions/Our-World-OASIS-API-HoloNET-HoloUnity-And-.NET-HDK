﻿using Solnet.Wallet;

namespace NextGenSoftware.OASIS.API.Providers.SOLANAOASIS.Infrastructure.Models.Common
{
    public class BaseAccountRequest
    {
        public string PublicKey { get; set; }

        public int GetAccountIndex()
        {
            return 0;
        }
    }
}