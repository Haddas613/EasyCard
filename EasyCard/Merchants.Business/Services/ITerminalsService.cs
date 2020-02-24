using Merchants.Business.Entities.Terminal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Merchants.Business.Services
{
    public interface ITerminalsService
    {
        public IQueryable<Terminal> GetTerminals();
    }
}
