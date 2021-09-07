using Reporting.Shared.Models;
using Reporting.Shared.Models.Admin;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Reporting.Business.Services
{
    public interface IAdminService
    {
        public Task<AdminSmsTimelines> GetSmsTotals(DashboardQuery query);
    }
}
