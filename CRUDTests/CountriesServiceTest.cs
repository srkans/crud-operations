using System;
using ServiceContracts;
using ServiceContracts.DTO;
using Entities;
using Services;
using Microsoft.EntityFrameworkCore;
using EntityFrameworkCoreMock;
using Moq;

namespace CRUDTests
{
    
    public class CountriesServiceTest
    {
        private readonly ICountriesService _countriesService;

        public CountriesServiceTest()
        {
            var countriesInitialData = new List<Country>() { }; //empty collection as a data source

            DbContextMock<ApplicationDbContext> dbContextMock = new DbContextMock<ApplicationDbContext>(new DbContextOptionsBuilder<ApplicationDbContext>().Options);

            ApplicationDbContext dbContext = dbContextMock.Object; //ApplicationDbContext is mock but behaves like original

            dbContextMock.CreateDbSetMock(temp => temp.Countries, countriesInitialData); //mock for DbSet<Country>

            _countriesService = new CountriesService(dbContext);
        }
        #region AddCountry
        //When CountryAddRequest is null, it should ArgumentNullException
        [Fact]
        public async Task AddCountry_NullCountry_ReturnsArgumentNullException()
        {
            //Arrange
            CountryAddRequest? request = null;

            //Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                //Act
                await _countriesService.AddCountry(request);
            });
        }

        //When the CountryName is null, it should throw ArgumentException
        [Fact]
        public async Task AddCountry_NullCountryName_ReturnsArgumentException()
        {
            //Arrange
            CountryAddRequest? request = new CountryAddRequest() { CountryName = null };

            //Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                //Act
                await _countriesService.AddCountry(request);
            });
        }

        //When the CountryName is duplicate, it should throw ArgumentException
        [Fact]
        public async Task AddCountry_DuplicateCountryName_ReturnsArgumentException()
        {
            //Arrange
            CountryAddRequest? request1 = new CountryAddRequest() { CountryName = "USA" };
            CountryAddRequest? request2 = new CountryAddRequest() { CountryName = "USA" };

            //Assert
           await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                //Act
                await _countriesService.AddCountry(request1);
                await _countriesService.AddCountry(request2);
            });
        }

        //When you supply proper CountryName, it should insert(add) the country to the existing list of countries
        [Fact]
        public async Task AddCountry_ProperCountryName_ReturnsListOfCountries()
        {
            //Arrange
            CountryAddRequest? request = new CountryAddRequest() { CountryName = "Japan" };
           
           //Act
            CountryResponse response = await _countriesService.AddCountry(request);
            List<CountryResponse> countriesFromGetAllCountries = await _countriesService.GetAllCountries();

           //Assert
            Assert.True(response.CountryID != Guid.Empty);
            Assert.Contains(response, countriesFromGetAllCountries);
        
        }
        #endregion

        #region GetAllCountries
        //the list of countries should be empty by default(before adding any countries)
        [Fact]
        public async Task GetAllCountries_EmptyList_ReturnsEmptyList()
        {
            //Act
            List<CountryResponse> actualCountryResponseList = await _countriesService.GetAllCountries();

            //Assert
            Assert.Empty(actualCountryResponseList);

        }

        [Fact]
        public async Task GetAllCountries_AddFewCountries_ReturnsSameCountries()
        {
            //Arrange
            List<CountryAddRequest> countryRequestList = new List<CountryAddRequest>()
            {
                new CountryAddRequest() {CountryName = "USA"},
                new CountryAddRequest() {CountryName = "China"},
                new CountryAddRequest() {CountryName = "Türkiye"}
            };

            List<CountryResponse> countryListFromAddCountry = new List<CountryResponse>();

            //Act
            foreach(CountryAddRequest countryRequest in countryRequestList)
            {
                countryListFromAddCountry.Add(await _countriesService.AddCountry(countryRequest));
            }

             List<CountryResponse> actualCountryResponseList = await _countriesService.GetAllCountries();

            //read each element from countriesListFromAddCountry
            foreach(CountryResponse expectedCountry in countryListFromAddCountry)
            {
                Assert.Contains(expectedCountry, actualCountryResponseList);
            }
        }
        #endregion

        #region GetCountryByCountryID

        //if we supply null as a CountryID, it should return null as CountryResponse
        [Fact]
        public async Task GetCountryByCountryID_NullCountryID_ReturnsNull()
        {
            //Arrange
            Guid? countryID = null;

            //Act
            CountryResponse? countryResponseFromGetMethod = await _countriesService.GetCountryById(countryID);

            //Assert
            Assert.Null(countryResponseFromGetMethod);
        }

        //if we supply a valid CountryID, it should return the matching country details as CountryResponse object
        [Fact]
        public async Task GetCountryByCountryID_ValidCountryID_ReturnsValidCountryResponse()
        {
            //Arrange
            CountryAddRequest? countryAddRequest = new CountryAddRequest() { CountryName = "China" };
            CountryResponse countryResponseFromAddRequest = await _countriesService.AddCountry(countryAddRequest);
           

            //Act
            CountryResponse? countryResponseFromGetMethod = await _countriesService.GetCountryById(countryResponseFromAddRequest.CountryID);

            //Assert
            Assert.Equal(countryResponseFromAddRequest,countryResponseFromGetMethod);
        }

        #endregion
    }
}
