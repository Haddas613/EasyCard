using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MerchantProfileApi
{
    public class Country
    {
        public string key { get; set; }

        public int id { get; set; }

        public string name { get; set; }

        public string flag { get; set; }

        public int area { get; set; }

        public int population { get; set; }

        public static IEnumerable<Country> GetCountries()
        {
            return new Country[] {
    new Country {
            id = 1,
        name = "Russia",
        flag = "https://upload.wikimedia.org/wikipedia/commons/f/f3/Flag_of_Russia.svg",
        area = 17075200,
        population = 146989754,
    },
    new Country {
            id = 2,
        name = "France",
        flag = "https://upload.wikimedia.org/wikipedia/commons/c/c3/Flag_of_France.svg",
        area = 640679,
        population = 64979548,
    },
    new Country {
            id = 3,
        name = "Germany",
        flag = "https://upload.wikimedia.org/wikipedia/commons/b/ba/Flag_of_Germany.svg",
        area = 357114,
        population = 82114224,
    },
    new Country {
            id = 4,
        name = "Portugal",
        flag = "https://upload.wikimedia.org/wikipedia/commons/5/5c/Flag_of_Portugal.svg",
        area = 92090,
        population = 10329506,
    },
    new Country {
            id = 5,
        name = "Canada",
        flag = "https://upload.wikimedia.org/wikipedia/commons/c/cf/Flag_of_Canada.svg",
        area = 9976140,
        population = 36624199,
    },
    new Country {
            id = 6,
        name = "Vietnam",
        flag = "https://upload.wikimedia.org/wikipedia/commons/2/21/Flag_of_Vietnam.svg",
        area = 331212,
        population = 95540800,
    },
    new Country {
            id = 7,
        name = "Brazil",
        flag = "https://upload.wikimedia.org/wikipedia/commons/0/05/Flag_of_Brazil.svg",
        area = 8515767,
        population = 209288278,
    },
    new Country {
            id = 8,
        name = "Mexico",
        flag = "https://upload.wikimedia.org/wikipedia/commons/f/fc/Flag_of_Mexico.svg",
        area = 1964375,
        population = 129163276,
    },
    new Country {
            id = 9,
        name = "United States",
        flag = "https://upload.wikimedia.org/wikipedia/commons/a/a4/Flag_of_the_United_States.svg",
        area = 9629091,
        population = 324459463,
    },
    new Country {
            id = 10,
        name = "India",
        flag = "https://upload.wikimedia.org/wikipedia/commons/4/41/Flag_of_India.svg",
        area = 3287263,
        population = 1324171354,
    },
    new Country {
            id = 11,
        name = "Indonesia",
        flag = "https://upload.wikimedia.org/wikipedia/commons/9/9f/Flag_of_Indonesia.svg",
        area = 1910931,
        population = 263991379,
    },
    new Country {
            id = 12,
        name = "Tuvalu",
        flag = "https://upload.wikimedia.org/wikipedia/commons/3/38/Flag_of_Tuvalu.svg",
        area = 26,
        population = 11097,
    },
    new Country {
            id = 13,
        name = "China",
        flag = "https://upload.wikimedia.org/wikipedia/commons/f/fa/Flag_of_the_People%27s_Republic_of_China.svg",
        area = 9596960,
        population = 1409517397,
    }
            };
        }
    }
}
