using CsvHelper;
using Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using ServiceContracts;
using ServiceContracts.DTO;

namespace Services
{
    public class CountriesService : ICountriesService
    {
        private readonly ApplicationDbContext _dbContext;

        public CountriesService(ApplicationDbContext countriesDbContext)
        {
            _dbContext = countriesDbContext;
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

                      if (_dbContext.Countries.Where(temp => temp.CountryName == countryName).Count() == 0)
                        {
                            Country country = new Country() { CountryName = countryName };
                            _dbContext.Countries.Add(country);
                            await _dbContext.SaveChangesAsync();
                            countriesInserted++;
                        }
                    }
                }
            }

            return countriesInserted;
        }
    }
}