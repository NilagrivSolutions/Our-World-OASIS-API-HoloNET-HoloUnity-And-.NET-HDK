﻿using NextGenSoftware.OASIS.API.Providers.SOLANAOASIS.Infrastructure.Models.Common;

namespace NextGenSoftware.OASIS.API.Providers.SOLANAOASIS.Infrastructure.Models.Responses
{
    public class MintNftResult : BaseTransactionResult
    {
        public MintNftResult(string transactionResult) : base(transactionResult)
        {
        }
    }
}