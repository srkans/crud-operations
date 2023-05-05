using Entities;
using Microsoft.EntityFrameworkCore;
using ServiceContracts;
using ServiceContracts.DTO;

namespace Services
{
    public class CountriesService : ICountriesService
    {
        private readonly PersonsDbContext _dbContext;

        public CountriesService(PersonsDbContext personsDbContext)
        {
            _dbContext = personsDbContext;
        }

        public async Task<CountryResponse> AddCountry(CountryAddRequest? countryAddRequest)
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
            if(await _dbContext.Countries.CountAsync(temp=>temp.CountryName == countryAddRequest.CountryName)>0)
            {
                throw new ArgumentException("Given country name already exist");
            }

            //Convert object from CountryAddRequest to Country type--from DTO to DomainModel
            Country country = countryAddRequest.ToCountry();

            //generate CountryID
            country.CountryID = Guid.NewGuid();

            //Add country object into _countries
            _dbContext.Add(country);
            await _dbContext.SaveChangesAsync();

            return country.ToCountryResponse(); //bu methoddaki yapı sayesinde domain model'i servis'in dışına göstermemiş oluyoruz.
        }

        public async Task<List<CountryResponse>> GetAllCountries()
        {
            return await _dbContext.Countries.Select(country=>country.ToCountryResponse()).ToListAsync();
        }

        public async Task<CountryResponse?> GetCountryById(Guid? countryID)
        {
            if(countryID == null)
            {
                return null;
            }

            Country? countryFromList = await _dbContext.Countries.FirstOrDefaultAsync(temp => temp.CountryID == countryID);

            if(countryFromList == null)
            { 
                return null;
            }

            return countryFromList.ToCountryResponse();
        }
    }
}