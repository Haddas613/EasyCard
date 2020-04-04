using Merchants.Business.Data;
using Merchants.Business.Entities.Integration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Api.Data.Seeds
{
    public class ExternalSystemsSeed
    {
        public static void Seed(MerchantsContext context)
        {
            var expectedData = new List<ExternalSystem>
            {
                new ExternalSystem
                {
                    //TODO: replace with real data
                    InstanceTypeFullName = "ClearingHouse.ClearingHouseAggregator, ClearingHouse",
                    Name = "PayDay Clearing House",
                    Type = Shared.Enums.ExternalSystemTypeEnum.Aggregator
                },
                new ExternalSystem
                {
                    InstanceTypeFullName = "Shva.ShvaProcessor, Shva",
                    Name = "Shva",
                    Type = Shared.Enums.ExternalSystemTypeEnum.Processor
                }
            };

            var actualData = context.ExternalSystems.ToList();

            foreach (var expected in expectedData)
            {
                var actual = actualData.FirstOrDefault(d => d.Name == expected.Name);

                if (actual != null)
                {
                    if (actual.InstanceTypeFullName != expected.InstanceTypeFullName)
                    {
                        actual.InstanceTypeFullName = expected.InstanceTypeFullName;
                    }
                }
                else
                {
                    context.ExternalSystems.Add(expected);
                }
            }

            context.SaveChanges();
        }
    }
}
