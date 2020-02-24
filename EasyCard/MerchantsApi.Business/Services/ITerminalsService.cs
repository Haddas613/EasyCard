using MerchantsApi.Business.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MerchantsApi.Business.Services
{
    public interface ITerminalsService
    {
        public IQueryable<Terminal> GetTerminals();
    }
}
