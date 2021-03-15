using Merchants.Business.Entities.Merchant;
using Merchants.Shared.Enums;
using Shared.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Merchants.Business.Services
{
    public interface IFeaturesService : IServiceBase<Feature, FeatureEnum>
    {
        IQueryable<Feature> GetQuery();
    }
}
