using System;
using System.Collections.Generic;
using System.Text;

namespace RapidOne.Models
{
    public class CustomerDto
    {
        public string NationalIDNumber { get; set; }
        public string CellPhone { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
 

        public IEnumerable<CustomerAddressDto> AddressesList { get; set; }
   
    }

    public class CustomerAddressDto
    {
        public string AddressName { get; set; }
        public int AddressTypeId { get; set; }
        public string CustomerCardCode { get; set; }
        public string Street { get; set; }
        public string HouseNumber { get; set; }
        public string AppartmentNumber { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
        public CountryDto Country { get; set; }
        public string TypeOfAddress { get; set; }
    }

    public class CountryDto
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }

    public class CreateCustomerResult
    {
        public bool Succeeded { get; set; }
        public string ErrorMessage { get; set; }
        public string CardCode { get; set; }
    }
}
