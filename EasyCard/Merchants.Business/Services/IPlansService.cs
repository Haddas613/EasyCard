﻿using Merchants.Business.Entities.Merchant;
using Shared.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merchants.Business.Services
{
    public interface IPlansService : IServiceBase<Plan, long>
    {
        IQueryable<Plan> GetQuery();
    }
}
