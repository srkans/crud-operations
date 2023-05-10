using CsvHelper;
using Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using RepositoryContracts;
using ServiceContracts;
using ServiceContracts.DTO;

namespace Services
{
    public class CountriesService : ICountriesService
    {
        private readonly ICountriesRepository _countriesRepository;

        public CountriesService(ICountriesRepository countriesRepository)
        {
            _countriesRepository = countriesRepository;
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
            if(await _countriesRepository.GetCountryByCountryName(countryAddRequest.CountryName) != null)
            {
                throw new ArgumentException("Given country name already exist");
            }

            //Convert object from CountryAddRequest to Country type--from DTO to DomainModel
            Country country = countryAddRequest.ToCountry();

            //generate CountryID
            country.CountryID = Guid.NewGuid();

            //Add country object into _countries
            await _countriesRepository.AddCountry(country);
            
            return country.ToCountryResponse(); //bu methoddaki yapı sayesinde domain model'i servis'in dışına göstermemiş oluyoruz.
        }

        public async Task<List<CountryResponse>> GetAllCountries()
        {
            return (await _countriesRepository.GetAllCountries()).Select(country => country.ToCountryResponse()).ToList();
        }

        public async Task<CountryResponse?> GetCountryById(Guid? countryID)
        {
            if(countryID == null)
            {
                return null;
            }

            Country? countryFromList = await _countriesRepository.GetCountryById(countryID.Value);

            if(countryFromList == null)
            { 
                return null;
            }

            return countryFromList.ToCountryResponse();
        }

        public async Task<int> UploadCountriesFromExcelFile(IFormFile formFile)
        {
            MemoryStream memoryStream = new MemoryStream();
            await formFile.CopyToAsync(memoryStream);
            int countriesInserted = 0;

            using (ExcelPackage excelPackage = new ExcelPackage(memoryStream))
            {
                ExcelWorksheet worksheet= excelPackage.Workbook.Worksheets["Countries"];

                int rowCount = worksheet.Dimension.Rows;//kullanıcı tarafından kaç row girildi
                
                for(int row=2; row<=rowCount; row++) //1header 2den itibaren data excel
                {
                    string? cellValue = Convert.ToString(worksheet.Cells[row, 1].Value); //1==columnA

                    if (!string.IsNullOrEmpty(cellValue))
                    {
                        string? countryName = cellValue;

                      if (await _countriesRepository.GetCountryByCountryName(countryName) == null)
                        {
                            Country country = new Country() { CountryName = countryName };
                            await _countriesRepository.AddCountry(country);
                            countriesInserted++;
                        }
                    }
                }
            }

            return countriesInserted;
        }
    }
}