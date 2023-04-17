using Entities;
using ServiceContracts;
using ServiceContracts.DTO;

namespace Services
{
    public class CountriesService : ICountriesService
    {
        private readonly List<Country> _countries;

        public CountriesService()
        {
            _countries = new List<Country>();
        }

        public CountryResponse AddCountry(CountryAddRequest? countryAddRequest)
        {
            //Validation:countryAddRequest can't be null
            if(countryAddRequest == null)
            {
                throw new ArgumentNullException(nameof(countryAddRequest));
            }

            //Validation:countryName can't be null
            if(countryAddRequest.CountryName == null)
            {
                throw new ArgumentException(nameof(countryAddRequest.CountryName));
            }

            //Validation:countryName duplicate not allowed
            if(_countries.Where(temp=>temp.CountryName == countryAddRequest.CountryName).Count()>0)
            {
                throw new ArgumentException("Given country name already exist");
            }

            //Convert object from CountryAddRequest to Country type--from DTO to DomainModel
            Country country = countryAddRequest.ToCountry();

            //generate CountryID
            country.CountryID = Guid.NewGuid();

            //Add country object into _countries
            _countries.Add(country);

            return country.ToCountryResponse(); //bu methoddaki yapı sayesinde domain model'i servis'in dışına göstermemiş oluyoruz.
        }

        public List<CountryResponse> GetAllCountries()
        {
            return _countries.Select(country=>country.ToCountryResponse()).ToList();
        }
    }
}