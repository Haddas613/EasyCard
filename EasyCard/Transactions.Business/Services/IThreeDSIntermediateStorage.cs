﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Transactions.Business.Entities;

namespace Transactions.Business.Services
{
    public interface IThreeDSIntermediateStorage
    {
        Task StoreIntermediateData(ThreeDSIntermediateData data);

        Task<ThreeDSIntermediateData> GetIntermediateData(string threeDSServerTransID);
    }
}
