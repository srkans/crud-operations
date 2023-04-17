using System;
using Entities;

namespace ServiceContracts.DTO
{
    /// <summary>
    /// DTO class that is used as a return type for most of CountryResponse methods
    /// </summary>
    public class CountryResponse
    {
        public Guid CountryID { get; set; }
        public string? CountryName { get; set; }

        public Country ToCountry() { return new Country {  CountryID = CountryID, CountryName = CountryName }; }
    }
}
