using Shared.Api.Models;
using Shared.Api.Models.Enums;
using System;

namespace Transactions.Api.Models.External.Bit
{
    public class InitialBitOperationResponse : OperationResponse
    {
        public string BitPaymentInitiationId { get; set; }

        public string BitTransactionSerialId { get; set; }

        public string RedirectURL { get; set; }

        public InitialBitOperationResponse()
        {
        }

        public InitialBitOperationResponse(string message, StatusEnum status, Guid? entityUid = null)
            : base(message, status, entityUid)
        {
        }

        public InitialBitOperationResponse(string message, StatusEnum status, string entityReference)
            : base(message, status, entityReference)
        {
        }
    }
}
