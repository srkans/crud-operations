using Entities;
using ServiceContracts;
using ServiceContracts.DTO;

namespace Services
{
    public class CountriesService : ICountriesService
    {
        private readonly List<Country> _countries;

        public CountriesService(bool initialize = true)
        {
            _countries = new List<Country>();

            if(initialize)
            {
                _countries.AddRange(new List<Country>() {
                new Country() { CountryID = Guid.Parse("A5082F1A-8A91-480C-B709-E2824286F04F"), CountryName = "Türkiye" },
                new Country() { CountryID = Guid.Parse("B76053EB-EEC4-4258-8147-A3C514C03BC2"), CountryName = "Australia" },
                new Country() { CountryID = Guid.Parse("EF0BC4B1-EDF1-4598-8A5C-45EE61A74AF4"), CountryName = "China" },
                new Country() { CountryID = Guid.Parse("60A0F453-D0DE-44F8-BCDA-53BA837E0148"), CountryName = "USA" },
                new Country() { CountryID = Guid.Parse("3E3F9AC3-D1DE-4DF1-B1F0-F10A014AEC3D"), CountryName = "Japan" }
                });
            }
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

        public CountryResponse? GetCountryById(Guid? countryID)
        {
            if(countryID == null)
            {
                return null;
            }

            Country? countryFromList = _countries.FirstOrDefault(temp => temp.CountryID == countryID);

            if(countryFromList == null)
            { 
                return null;
            }

            return countryFromList.ToCountryResponse();
        }
    }
}