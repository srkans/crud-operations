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

        public override bool Equals(object? obj)
        {
            if(obj == null) return false;

            if(obj.GetType() != typeof(CountryResponse))
            {
                return false;
            }

            CountryResponse countryToCompare = obj as CountryResponse;

            return CountryID == countryToCompare.CountryID && CountryName == countryToCompare.CountryName;
        }

    }
 

    public static class CountryExtensions
    {
        public static CountryResponse ToCountryResponse(this Country country) 
        {
            return new CountryResponse() { CountryID = country.CountryID, CountryName = country.CountryName };
        }
    }
}
