﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MerchantProfileApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProfileApi;
using Shared.Api.Models;
using Shared.Api.Models.Enums;
using Shared.Api.Models.Metadata;

namespace MerchantProfileApi.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.TerminalOrMerchantFrontend)]
    [Route("api/[controller]")]
    [ApiController]
    public class CountriesController : ControllerBase
    {
        // GET: api/Countries
        [HttpGet]
        public SummariesResponse<Country> Get([FromQuery]CountriesFilter filter)
        {
            var countries = Country.GetCountries();

            if (!string.IsNullOrWhiteSpace(filter.Name))
            {
                countries = countries.Where(d => d.name.Contains(filter.Name, StringComparison.InvariantCultureIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(filter.SortBy))
            {
                switch (filter.SortBy.ToLower())
                {
                    case "name":
                        countries = filter.OrderByDirection == OrderByDirectionEnum.ASC ? countries.OrderBy(d => d.name) : countries.OrderByDescending(d => d.name);
                        break;
                    case "id":
                        countries = filter.OrderByDirection == OrderByDirectionEnum.ASC ? countries.OrderBy(d => d.id) : countries.OrderByDescending(d => d.id);
                        break;
                    case "area":
                        countries = filter.OrderByDirection == OrderByDirectionEnum.ASC ? countries.OrderBy(d => d.area) : countries.OrderByDescending(d => d.area);
                        break;
                    case "population":
                        countries = filter.OrderByDirection == OrderByDirectionEnum.ASC ? countries.OrderBy(d => d.population) : countries.OrderByDescending(d => d.population);
                        break;
                    default:
                        break;
                }
            }

            var numberOfRecords = countries.Count();

            if (filter.Skip.HasValue)
            {
                countries = countries.Skip(filter.Skip.Value);
            }

            if (filter.Take.HasValue)
            {
                countries = countries.Take(filter.Take.Value);
            }

            return new SummariesResponse<Country> { Data = countries, NumberOfRecords = numberOfRecords };
        }

        // GET: api/Countries/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
        }

        [HttpGet]
        [Route("$meta")]
        public TableMeta GetMetadata()
        {
            return new TableMeta
            {
                Columns = typeof(Country).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).Select(d => new ColMeta { Key = d.Name, Name = d.Name, DataType = d.PropertyType.Name }).ToDictionary(d => d.Key)
            };
        }

        // POST: api/Countries
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT: api/Countries/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
