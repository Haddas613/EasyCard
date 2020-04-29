using Shared.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MerchantProfileApi.Models
{
    public class CountriesFilter : FilterBase
    {
        public int? Id { get; set; }

        public string Name { get; set; }

        public int? AreaFrom { get; set; }

        public int? AreaTo { get; set; }
    }
}
