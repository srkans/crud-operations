﻿using Entities;
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
            if(_dbContext.Countries.Where(temp=>temp.CountryName == countryAddRequest.CountryName).Count()>0)
            {
                throw new ArgumentException("Given country name already exist");
            }

            //Convert object from CountryAddRequest to Country type--from DTO to DomainModel
            Country country = countryAddRequest.ToCountry();

            //generate CountryID
            country.CountryID = Guid.NewGuid();

            //Add country object into _countries
            _dbContext.Add(country);
            _dbContext.SaveChanges();

            return country.ToCountryResponse(); //bu methoddaki yapı sayesinde domain model'i servis'in dışına göstermemiş oluyoruz.
        }

        public List<CountryResponse> GetAllCountries()
        {
            return _dbContext.Countries.Select(country=>country.ToCountryResponse()).ToList();
        }

        public CountryResponse? GetCountryById(Guid? countryID)
        {
            if(countryID == null)
            {
                return null;
            }

            Country? countryFromList = _dbContext.Countries.FirstOrDefault(temp => temp.CountryID == countryID);

            if(countryFromList == null)
            { 
                return null;
            }

            return countryFromList.ToCountryResponse();
        }
    }
}