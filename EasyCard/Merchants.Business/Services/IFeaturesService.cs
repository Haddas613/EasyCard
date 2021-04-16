using Merchants.Business.Entities.Merchant;
using Merchants.Business.Entities.Terminal;
using Merchants.Shared.Enums;
using Shared.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merchants.Business.Services
{
    public interface IFeaturesService : IServiceBase<Feature, FeatureEnum>
    {
        IQueryable<Feature> GetQuery();
    }
}
