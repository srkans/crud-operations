
namespace CRUDTests
{
    public class PersonServiceTests
    {
        private readonly IPersonsService _personsService;
        private readonly ICountriesService _countriesService;
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly IFixture _fixture;

        public PersonServiceTests(ITestOutputHelper testOutputHelper)
        {
            _fixture = new Fixture();

            var countriesInitialData = new List<Country>() { }; //empty collection as a data source
            var personsInitialData = new List<Person>() { };

            DbContextMock<ApplicationDbContext> dbContextMock = new DbContextMock<ApplicationDbContext>(new DbContextOptionsBuilder<ApplicationDbContext>().Options);

            ApplicationDbContext dbContext = dbContextMock.Object; //ApplicationDbContext is mock but behaves like original

            dbContextMock.CreateDbSetMock(temp => temp.Countries, countriesInitialData); //mock for DbSet<Country>
            dbContextMock.CreateDbSetMock(temp => temp.Persons, personsInitialData);

            _countriesService = new CountriesService(null);
            _personsService = new PersonsService(null);

            _testOutputHelper = testOutputHelper;
        }

        #region AddPerson

        //when we supply null value as PersonAddRequest, it should throw ArgumentNullException
        [Fact]
        public async Task AddPerson_NullPerson_ReturnsArgumentNullException()
        {
            //Arrange
            PersonAddRequest personAddRequest = null;


            Func<Task> action = async () =>
            {
                //Act
                await _personsService.AddPerson(personAddRequest);
            };

            //Assert
            await action.Should().ThrowAsync<ArgumentNullException>();

        }

        //when we supply null value as PersonName, it should throw ArgumentException
        [Fact]
        public async Task AddPerson_NullPersonName_ReturnsArgumentException()
        {
            //Arrange
            PersonAddRequest personAddRequest = _fixture.Build<PersonAddRequest>().With(temp => temp.Name, null as string).Create();

            Func<Task> action = async () => //Func has return type Action has not (delegates)
            {
                //Act
                await _personsService.AddPerson(personAddRequest);
            };

            await action.Should().ThrowAsync<ArgumentException>();

        }

        //when we supply proper person details, it should insert the person into the persons list and it should return an object of PersonResponse, which include PersonID
        [Fact]
        public async Task AddPerson_ProperPersonObject_ReturnsPersonResponse()
        {
            //Arrange
            PersonAddRequest? personAddRequest = _fixture.Build<PersonAddRequest>().With(temp => temp.Email, "asd@ss.com").Create();

            //Act
            PersonResponse personResponseFromAdd = await _personsService.AddPerson(personAddRequest);
            List<PersonResponse> personList = await _personsService.GetAllPersons();

            //Assert
            // Assert.True(personResponseFromAdd.PersonID != Guid.Empty);
            personResponseFromAdd.PersonID.Should().NotBe(Guid.Empty);

            // Assert.Contains(personResponseFromAdd, personList);
            personList.Should().Contain(personResponseFromAdd);

        }

        #endregion

        #region GetPersonByPersonID

        //if we supply null as personID, it should return null as personresponse
        [Fact]
        public async Task GetPersonByPersonID_NullPersonID_ReturnsNullPersonResponse()
        {
            //Arrange
            Guid? personID = null;

            //Act
            PersonResponse? personResponseFromGet = await _personsService.GetPersonByPersonID(personID);

            //Assert
            // Assert.Null(personResponseFromGet);
            personResponseFromGet.Should().BeNull();
        }

        //if we supply proper personID
        [Fact]
        public async Task GetPersonByPersonID_ProperPersonID_ReturnsProperPersonResponse()
        {
            //Arrange
            CountryAddRequest countryAddRequest = _fixture.Create<CountryAddRequest>();
            CountryResponse countryResponse = await _countriesService.AddCountry(countryAddRequest);

            //Act
            PersonAddRequest personAddRequest = _fixture.Build<PersonAddRequest>().With(temp => temp.Email, "asd@ss.com").Create();
            PersonResponse personResponseFromAdd = await _personsService.AddPerson(personAddRequest);

            PersonResponse? personResponseFromGet = await _personsService.GetPersonByPersonID(personResponseFromAdd.PersonID);

            //Assert
            // Assert.Equal(personResponseFromAdd, personResponseFromGet);
            personResponseFromGet.Should().Be(personResponseFromAdd);
        }

        #endregion

        #region GetAllPersons

        //The GetAllPersons() should return an empty list by default
        [Fact]
        public async Task GetAllPersons_EmptyList()
        {
            //Act
            List<PersonResponse> personResponsesFromGet = await _personsService.GetAllPersons();

            //Assert
            // Assert.Empty(personResponsesFromGet);
            personResponsesFromGet.Should().BeEmpty();
        }

        //add some persons then it should return same persons
        [Fact]
        public async Task GetAllPersons_ReturnAllPersons()
        {
            //Arrange
            CountryAddRequest countryAddRequest1 = _fixture.Create<CountryAddRequest>();
            CountryAddRequest countryAddRequest2 = _fixture.Create<CountryAddRequest>();

            CountryResponse countryResponse1 = await _countriesService.AddCountry(countryAddRequest1);
            CountryResponse countryResponse2 = await _countriesService.AddCountry(countryAddRequest2);

            PersonAddRequest personAddRequest1 = _fixture.Build<PersonAddRequest>().With(temp => temp.Email, "asd@ss.com").Create();
            PersonAddRequest personAddRequest2 = _fixture.Build<PersonAddRequest>().With(temp => temp.Email, "asd@ss.com").Create();
            PersonAddRequest personAddRequest3 = _fixture.Build<PersonAddRequest>().With(temp => temp.Email, "asd@ss.com").Create();

            List<PersonAddRequest> personAddList = new List<PersonAddRequest> { personAddRequest1, personAddRequest2, personAddRequest3 };

            List<PersonResponse> personResponsesFromAdd = new List<PersonResponse>();

            foreach (PersonAddRequest personAddRequest in personAddList)
            {
                personResponsesFromAdd.Add(await _personsService.AddPerson(personAddRequest));
            }

            //Act
            List<PersonResponse> personResponsesFromGet = await _personsService.GetAllPersons();

            //Assert
            //foreach (PersonResponse personResponse in personResponsesFromAdd)
            //{
            //    Assert.Contains(personResponse, personResponsesFromGet);
            //}
            personResponsesFromGet.Should().BeEquivalentTo(personResponsesFromAdd);

            _testOutputHelper.WriteLine("Expected");
            //print personResponsesFrom Add
            foreach (PersonResponse personResponse in personResponsesFromAdd)
            {
                _testOutputHelper.WriteLine(personResponse.ToString());
            }

            _testOutputHelper.WriteLine("Actual");
            //print personResponsesFrom Get
            foreach (PersonResponse personResponse in personResponsesFromGet)
            {
                _testOutputHelper.WriteLine(personResponse.ToString());
            }
        }

        #endregion

        #region GetFilteredPersons
        //if the search text is empty and search by is "PersonName", it should return all persons
        [Fact]
        public async Task GetFilteredPersons_SearchTextIsEmpty_ReturnAllPersons()
        {
            //Arrange
            CountryAddRequest countryAddRequest1 = _fixture.Create<CountryAddRequest>();
            CountryAddRequest countryAddRequest2 = _fixture.Create<CountryAddRequest>();

            CountryResponse countryResponse1 = await _countriesService.AddCountry(countryAddRequest1);
            CountryResponse countryResponse2 = await _countriesService.AddCountry(countryAddRequest2);

            PersonAddRequest personAddRequest1 = _fixture.Build<PersonAddRequest>().With(temp => temp.Email, "asd@ss.com").Create();
            PersonAddRequest personAddRequest2 = _fixture.Build<PersonAddRequest>().With(temp => temp.Email, "asd@ss.com").Create();
            PersonAddRequest personAddRequest3 = _fixture.Build<PersonAddRequest>().With(temp => temp.Email, "asd@ss.com").Create();

            List<PersonAddRequest> personAddList = new List<PersonAddRequest> { personAddRequest1, personAddRequest2, personAddRequest3 };

            List<PersonResponse> personResponsesFromAdd = new List<PersonResponse>();

            foreach (PersonAddRequest personAddRequest in personAddList)
            {
                personResponsesFromAdd.Add(await _personsService.AddPerson(personAddRequest));
            }


            _testOutputHelper.WriteLine("Expected");
            //print personResponsesFrom Add
            foreach (PersonResponse personResponse in personResponsesFromAdd)
            {
                _testOutputHelper.WriteLine(personResponse.ToString());
            }


            //Act
            List<PersonResponse> personResponsesFromSearch = await _personsService.GetFilteredPersons(nameof(Person.Name), "");

            _testOutputHelper.WriteLine("Actual");
            //print personResponsesFrom Get
            foreach (PersonResponse personResponse in personResponsesFromSearch)
            {
                _testOutputHelper.WriteLine(personResponse.ToString());
            }

            //Assert
            personResponsesFromSearch.Should().BeEquivalentTo(personResponsesFromAdd);
        }

        //Search based on person name with some search string, it should return matching persons

        [Fact]
        public async Task GetFilteredPersons_SearchByPersonName_ReturnMatchingPersons()
        {
            //Arrange
            CountryAddRequest countryAddRequest1 = _fixture.Create<CountryAddRequest>();
            CountryAddRequest countryAddRequest2 = _fixture.Create<CountryAddRequest>();

            CountryResponse countryResponse1 = await _countriesService.AddCountry(countryAddRequest1);
            CountryResponse countryResponse2 = await _countriesService.AddCountry(countryAddRequest2);

            PersonAddRequest personAddRequest1 = _fixture.Build<PersonAddRequest>().With(temp => temp.Email, "asd@ss.com").With(temp => temp.Name, "Serkan").Create();
            PersonAddRequest personAddRequest2 = _fixture.Build<PersonAddRequest>().With(temp => temp.Email, "asd@ss.com").Create();
            PersonAddRequest personAddRequest3 = _fixture.Build<PersonAddRequest>().With(temp => temp.Email, "asd@ss.com").Create();

            List<PersonAddRequest> personAddList = new List<PersonAddRequest> { personAddRequest1, personAddRequest2, personAddRequest3 };

            List<PersonResponse> personResponsesFromAdd = new List<PersonResponse>();

            foreach (PersonAddRequest personAddRequest in personAddList)
            {
                personResponsesFromAdd.Add(await _personsService.AddPerson(personAddRequest));
            }

            _testOutputHelper.WriteLine("Expected");
            //print personResponsesFrom Add
            foreach (PersonResponse personResponse in personResponsesFromAdd)
            {
                if (personResponse.Name.Contains("erk", StringComparison.OrdinalIgnoreCase))
                {
                    _testOutputHelper.WriteLine(personResponse.ToString());
                }
            }

            //Act
            List<PersonResponse> personResponsesFromSearch = await _personsService.GetFilteredPersons(nameof(Person.Name), "erk");

            _testOutputHelper.WriteLine("Actual");
            //print personResponsesFrom Get
            foreach (PersonResponse personResponse in personResponsesFromSearch)
            {
                _testOutputHelper.WriteLine(personResponse.ToString());
            }
            //Assert
            personResponsesFromSearch.Should().OnlyContain(temp => temp.Name.Contains("erk", StringComparison.OrdinalIgnoreCase));
        }
        #endregion

        #region GetSortedPersons
        //when we sort based on PersonName in DESC, it should return the persons list in descending on PersonName
        [Fact]
        public async Task GetSortedPersons()
        {
            //Arrange
            CountryAddRequest countryAddRequest1 = _fixture.Create<CountryAddRequest>();
            CountryAddRequest countryAddRequest2 = _fixture.Create<CountryAddRequest>();

            CountryResponse countryResponse1 = await _countriesService.AddCountry(countryAddRequest1);
            CountryResponse countryResponse2 = await _countriesService.AddCountry(countryAddRequest2);

            PersonAddRequest personAddRequest1 = _fixture.Build<PersonAddRequest>().With(temp => temp.Email, "asd@ss.com").Create();
            PersonAddRequest personAddRequest2 = _fixture.Build<PersonAddRequest>().With(temp => temp.Email, "asd@ss.com").Create();
            PersonAddRequest personAddRequest3 = _fixture.Build<PersonAddRequest>().With(temp => temp.Email, "asd@ss.com").Create();

            List<PersonAddRequest> personAddList = new List<PersonAddRequest> { personAddRequest1, personAddRequest2, personAddRequest3 };

            List<PersonResponse> personResponseListFromAdd = new List<PersonResponse>();

            foreach (PersonAddRequest personAddRequest in personAddList)
            {
                personResponseListFromAdd.Add(await _personsService.AddPerson(personAddRequest));
            }
        

            _testOutputHelper.WriteLine("Expected");
            //print personResponsesFrom Add
            foreach (PersonResponse personResponse in personResponseListFromAdd)
            {
                    _testOutputHelper.WriteLine(personResponse.ToString());             
            }

            List<PersonResponse> allPersons = await _personsService.GetAllPersons();

            //Act
            List<PersonResponse> personResponseListFromSort = await _personsService.GetSortedPersons(allPersons, nameof(Person.Name), SortOrderOptions.DESC);

            _testOutputHelper.WriteLine("Actual");
            //print personResponsesFrom Get
            foreach (PersonResponse personResponse in personResponseListFromSort)
            {
                _testOutputHelper.WriteLine(personResponse.ToString());
            }

            //Assert
            personResponseListFromSort.Should().BeInDescendingOrder(temp => temp.Name);
        }

        #endregion

        #region UpdatePerson

        //when we supply null as PersonUpdateRequest, it should throw ArgumentNullException
        [Fact]
        public async Task UpdatePerson_NullPerson_ReturnsArgumentNullException()
        {
            //Arrange
            PersonUpdateRequest? personUpdateRequest = null;

            Func<Task> action = async () =>
            {
                //Act
                await _personsService.UpdatePerson(personUpdateRequest);
            };
            //Assert
            await action.Should().ThrowAsync<ArgumentNullException>();
        }

        //when we supply invalid person id, it should throw ArgumentException
        [Fact]
        public async Task UpdatePerson_InvalidPersonId_ReturnsArgumentException()
        {
            //Arrange
            PersonUpdateRequest? personUpdateRequest = _fixture.Create<PersonUpdateRequest>();

            Func<Task> action = async () =>
            {
                //Act
                await _personsService.UpdatePerson(personUpdateRequest);
            };
            //Assert
            await action.Should().ThrowAsync<ArgumentException>();
        }

        //when we supply null person name, it should throw ArgumentException
        [Fact]
        public async Task UpdatePerson_NullPersonName_ReturnsArgumentException()
        {
            //Arrange
            CountryAddRequest countryAddRequest = _fixture.Create<CountryAddRequest>();
            CountryResponse countryResponseFromAdd = await _countriesService.AddCountry(countryAddRequest);

            PersonAddRequest personAddRequest = _fixture.Build<PersonAddRequest>().With(temp => temp.Email, "asd@ss.com").Create();
            PersonResponse personResponseFromAdd = await _personsService.AddPerson(personAddRequest);

            PersonUpdateRequest? personUpdateRequest = personResponseFromAdd.ToPersonUpdateRequest();
            personUpdateRequest.Name = null;

            Func<Task> action = async () =>
            {
                //Act
                await _personsService.UpdatePerson(personUpdateRequest);
            };
            //Assert
            await action.Should().ThrowAsync<ArgumentException>();
        }

        //First add a new person and try to update the person name and email
        [Fact]
        public async Task UpdatePerson_PersonFullDetails_ReturnsPersonResponse()
        {
            //Arrange
            CountryAddRequest countryAddRequest = _fixture.Create<CountryAddRequest>();
            CountryResponse countryResponseFromAdd = await _countriesService.AddCountry(countryAddRequest);

            PersonAddRequest personAddRequest = _fixture.Build<PersonAddRequest>().With(temp => temp.Email, "asd@ss.com").Create();
            PersonResponse personResponseFromAdd = await _personsService.AddPerson(personAddRequest);

            PersonUpdateRequest? personUpdateRequest = personResponseFromAdd.ToPersonUpdateRequest();
            personUpdateRequest.Name = "Harsha";
            personUpdateRequest.Email = "harsha@kral.com";

            //Act
            PersonResponse personResponseFromUpdate = await _personsService.UpdatePerson(personUpdateRequest);

            PersonResponse? personResponseFromGet = await _personsService.GetPersonByPersonID(personResponseFromUpdate.PersonID);

            //Assert
            personResponseFromUpdate.Should().Be(personResponseFromGet);


        }

        #endregion

        #region DeletePerson

        //if we supply and valid PersonID, it should return true
        [Fact]
        public async Task DeletePerson_ValidPerson_ReturnsTrue()
        {
            CountryAddRequest countryAddRequest = _fixture.Create<CountryAddRequest>();
            CountryResponse countryResponseFromAdd = await _countriesService.AddCountry(countryAddRequest);

            PersonAddRequest personAddRequest = _fixture.Build<PersonAddRequest>().With(temp => temp.Email, "asd@ss.com").Create();
            PersonResponse personResponseFromAdd = await _personsService.AddPerson(personAddRequest);

            //Act
            bool isDeleted = await _personsService.DeletePerson(personResponseFromAdd.PersonID);

            //Assert
            isDeleted.Should().BeTrue();

        }

        //if we supply and invalid PersonID, it should return false
        [Fact]
        public async Task DeletePerson_InvalidPerson_ReturnsFalse()
        {
            CountryAddRequest countryAddRequest = _fixture.Create<CountryAddRequest>();
            CountryResponse countryResponseFromAdd = await _countriesService.AddCountry(countryAddRequest);

            PersonAddRequest personAddRequest = _fixture.Build<PersonAddRequest>().With(temp => temp.Email, "asd@ss.com").Create();
            PersonResponse personResponseFromAdd = await _personsService.AddPerson(personAddRequest);

            //Act
            bool isDeleted = await _personsService.DeletePerson(Guid.NewGuid());

            //Assert
            isDeleted.Should().BeFalse();

        }

        #endregion
    }
}
