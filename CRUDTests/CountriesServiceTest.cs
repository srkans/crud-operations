
namespace CRUDTests
{
    
    public class CountriesServiceTest
    {
        private readonly ICountriesService _countriesService;
        private readonly IFixture _fixture;

        public CountriesServiceTest()
        {
            _fixture = new Fixture();

            var countriesInitialData = new List<Country>() { }; //empty collection as a data source

            DbContextMock<ApplicationDbContext> dbContextMock = new DbContextMock<ApplicationDbContext>(new DbContextOptionsBuilder<ApplicationDbContext>().Options);

            ApplicationDbContext dbContext = dbContextMock.Object; //ApplicationDbContext is mock but behaves like original

            dbContextMock.CreateDbSetMock(temp => temp.Countries, countriesInitialData); //mock for DbSet<Country>

            _countriesService = new CountriesService(null);
        }
        #region AddCountry
        //When CountryAddRequest is null, it should ArgumentNullException
        [Fact]
        public async Task AddCountry_NullCountry_ReturnsArgumentNullException()
        {
            //Arrange
            CountryAddRequest? request = null;

            //Act
            Func<Task> action = async () =>
            {              
                await _countriesService.AddCountry(request);
            };
            //Assert
            await action.Should().ThrowAsync<ArgumentNullException>();
        }

        //When the CountryName is null, it should throw ArgumentException
        [Fact]
        public async Task AddCountry_NullCountryName_ReturnsArgumentException()
        {
            //Arrange
            CountryAddRequest? request = _fixture.Build<CountryAddRequest>().With(temp => temp.CountryName, null as string).Create();

            //Act
            Func<Task> action = async () =>
            {
                await _countriesService.AddCountry(request);
            };
            //Assert
            await action.Should().ThrowAsync<ArgumentException>();
        }

        //When the CountryName is duplicate, it should throw ArgumentException
        [Fact]
        public async Task AddCountry_DuplicateCountryName_ReturnsArgumentException()
        {
            //Arrange
            CountryAddRequest? request1 = _fixture.Build<CountryAddRequest>().With(temp => temp.CountryName,"Türkiye").Create();
            CountryAddRequest? request2 = _fixture.Build<CountryAddRequest>().With(temp => temp.CountryName, "Türkiye").Create();

            //Act
            Func<Task> action = async () =>
            {                
                await _countriesService.AddCountry(request1);
                await _countriesService.AddCountry(request2);
            };
            //Assert
            await action.Should().ThrowAsync<ArgumentException>();
        }

        //When you supply proper CountryName, it should insert(add) the country to the existing list of countries
        [Fact]
        public async Task AddCountry_ProperCountryName_ReturnsListOfCountries()
        {
            //Arrange
            CountryAddRequest? request = _fixture.Create<CountryAddRequest>();

            //Act
            CountryResponse response = await _countriesService.AddCountry(request);
            List<CountryResponse> countriesFromGetAllCountries = await _countriesService.GetAllCountries();

           //Assert
            response.CountryID.Should().NotBe(Guid.Empty);
            countriesFromGetAllCountries.Should().Contain(response);


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
            countryResponseFromGetMethod.Should().BeNull();
        }

        //if we supply a valid CountryID, it should return the matching country details as CountryResponse object
        [Fact]
        public async Task GetCountryByCountryID_ValidCountryID_ReturnsValidCountryResponse()
        {
            //Arrange
            CountryAddRequest? countryAddRequest = _fixture.Create<CountryAddRequest>();
            CountryResponse countryResponseFromAddRequest = await _countriesService.AddCountry(countryAddRequest);
           

            //Act
            CountryResponse? countryResponseFromGetMethod = await _countriesService.GetCountryById(countryResponseFromAddRequest.CountryID);

            //Assert
            countryResponseFromGetMethod.Should().Be(countryResponseFromAddRequest);
        }

        #endregion
    }
}
