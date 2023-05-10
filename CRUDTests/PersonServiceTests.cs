
using Moq;
using System.Linq.Expressions;

namespace CRUDTests
{
    public class PersonServiceTests
    {
        private readonly IPersonsService _personService;
        private readonly ICountriesService _countriesService;
        private readonly Mock<IPersonsRepository> _personRepositoryMock;
        private readonly IPersonsRepository _personsRepository;
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly IFixture _fixture;


        public PersonServiceTests(ITestOutputHelper testOutputHelper)
        {
            _fixture = new Fixture();
            _personRepositoryMock = new Mock<IPersonsRepository>();
            _personsRepository = _personRepositoryMock.Object;

            var countriesInitialData = new List<Country>() { }; //empty collection as a data source
            var personsInitialData = new List<Person>() { };

            DbContextMock<ApplicationDbContext> dbContextMock = new DbContextMock<ApplicationDbContext>(new DbContextOptionsBuilder<ApplicationDbContext>().Options);

            ApplicationDbContext dbContext = dbContextMock.Object; //ApplicationDbContext is mock but behaves like original

            dbContextMock.CreateDbSetMock(temp => temp.Countries, countriesInitialData); //mock for DbSet<Country>
            dbContextMock.CreateDbSetMock(temp => temp.Persons, personsInitialData);

            _countriesService = new CountriesService(null);
            _personService = new PersonsService(_personsRepository);

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
                await _personService.AddPerson(personAddRequest);
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

            Person person = personAddRequest.ToPerson();
            _personRepositoryMock.Setup(temp => temp.AddPerson(It.IsAny<Person>())).ReturnsAsync(person);

            Func<Task> action = async () => //Func has return type Action has not (delegates)
            {
                //Act
                await _personService.AddPerson(personAddRequest);
            };

            await action.Should().ThrowAsync<ArgumentException>();

        }

        //when we supply proper person details, it should insert the person into the persons list and it should return an object of PersonResponse, which include PersonID
        [Fact]
        public async Task AddPerson_ProperPersonObject_ReturnsPersonResponse()
        {
            //Arrange
            PersonAddRequest? personAddRequest = _fixture.Build<PersonAddRequest>().With(temp => temp.Email, "asd@ss.com").Create();

            Person person = personAddRequest.ToPerson();
            PersonResponse personResponseExpected = person.ToPersonResponse();

            //if we supply any argument value to the AddPerson method, it should return the same value
            _personRepositoryMock.Setup(temp => temp.AddPerson(It.IsAny<Person>())).ReturnsAsync(person);

            //Act
            PersonResponse personResponseFromAdd = await _personService.AddPerson(personAddRequest);

            personResponseExpected.PersonID = personResponseFromAdd.PersonID;

            //Assert
            // Assert.True(personResponseFromAdd.PersonID != Guid.Empty);
            personResponseFromAdd.PersonID.Should().NotBe(Guid.Empty);

            personResponseFromAdd.Should().Be(personResponseExpected);
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
            PersonResponse? personResponseFromGet = await _personService.GetPersonByPersonID(personID);

            //Assert
            // Assert.Null(personResponseFromGet);
            personResponseFromGet.Should().BeNull();
        }

        //if we supply proper personID
        [Fact]
        public async Task GetPersonByPersonID_ProperPersonID_ReturnsProperPersonResponse()
        {
            //Arrange
            Person person = _fixture.Build<Person>().With(temp => temp.Email, "asd@ss.com").With(temp => temp.Country, null as Country).Create();
            PersonResponse personResponseExpected = person.ToPersonResponse();

            _personRepositoryMock.Setup(temp => temp.GetPersonByPersonID(It.IsAny<Guid>())).ReturnsAsync(person);
            //Act
            PersonResponse? personResponseFromGet = await _personService.GetPersonByPersonID(person.PersonID);

            //Assert
            personResponseFromGet.Should().Be(personResponseExpected);
        }

        #endregion

        #region GetAllPersons

        //The GetAllPersons() should return an empty list by default
        [Fact]
        public async Task GetAllPersons_EmptyList()
        {
            //Arrange
            var persons = new List<Person>();
            _personRepositoryMock.Setup(temp => temp.GetAllPersons()).ReturnsAsync(persons);

            //Act
            List<PersonResponse> personResponsesFromGet = await _personService.GetAllPersons();

            //Assert
            personResponsesFromGet.Should().BeEmpty();
        }

        //add some persons then it should return same persons
        [Fact]
        public async Task GetAllPersons_WithFewPersons_ReturnAllPersons()
        {
            //Arrange
            List<Person> persons = new List<Person>()
            {
                _fixture.Build<Person>().With(temp => temp.Email, "asd@ss.com").With(temp => temp.Country,null as Country).Create(),
                _fixture.Build<Person>().With(temp => temp.Email, "asd2@ss.com").With(temp => temp.Country,null as Country).Create(),
                _fixture.Build<Person>().With(temp => temp.Email, "asd3@ss.com").With(temp => temp.Country,null as Country).Create()
            };

            List<PersonResponse> personResponseListExpected = persons.Select(temp => temp.ToPersonResponse()).ToList();

            _personRepositoryMock.Setup(temp => temp.GetAllPersons()).ReturnsAsync(persons);

            List<PersonResponse> personResponsesFromGet = await _personService.GetAllPersons();

            personResponsesFromGet.Should().BeEquivalentTo(personResponseListExpected);

            _testOutputHelper.WriteLine("Expected");
            //print personResponsesFrom Add
            foreach (PersonResponse personResponse in personResponseListExpected)
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
            List<Person> persons = new List<Person>()
            {
                _fixture.Build<Person>().With(temp => temp.Email, "asd@ss.com").With(temp => temp.Country,null as Country).Create(),
                _fixture.Build<Person>().With(temp => temp.Email, "asd2@ss.com").With(temp => temp.Country,null as Country).Create(),
                _fixture.Build<Person>().With(temp => temp.Email, "asd3@ss.com").With(temp => temp.Country,null as Country).Create()
            };

            List<PersonResponse> personResponseListExpected = persons.Select(temp => temp.ToPersonResponse()).ToList();


            _testOutputHelper.WriteLine("Expected");
            //print personResponses
            foreach (PersonResponse personResponse in personResponseListExpected)
            {
                _testOutputHelper.WriteLine(personResponse.ToString());
            }

            //Act
            _personRepositoryMock.Setup(temp => temp.GetFilteredPersons(It.IsAny<Expression<Func<Person, bool>>>())).ReturnsAsync(persons);

            List<PersonResponse> personResponsesFromSearch = await _personService.GetFilteredPersons(nameof(Person.Name), "");

            _testOutputHelper.WriteLine("Actual");
            //print personResponsesFrom Get
            foreach (PersonResponse personResponse in personResponsesFromSearch)
            {
                _testOutputHelper.WriteLine(personResponse.ToString());
            }

            //Assert
            personResponsesFromSearch.Should().BeEquivalentTo(personResponseListExpected);
        }

        //Search based on person name with some search string, it should return matching persons

        [Fact]
        public async Task GetFilteredPersons_SearchByPersonName_ReturnMatchingPersons()
        {
            //Arrange
            List<Person> persons = new List<Person>()
            {
                _fixture.Build<Person>().With(temp => temp.Email, "asd@ss.com").With(temp => temp.Country,null as Country).Create(),
                _fixture.Build<Person>().With(temp => temp.Email, "asd2@ss.com").With(temp => temp.Country,null as Country).Create(),
                _fixture.Build<Person>().With(temp => temp.Email, "asd3@ss.com").With(temp => temp.Country,null as Country).Create()
            };

            List<PersonResponse> personResponseListExpected = persons.Select(temp => temp.ToPersonResponse()).ToList();


            _testOutputHelper.WriteLine("Expected");
            //print personResponses
            foreach (PersonResponse personResponse in personResponseListExpected)
            {
                _testOutputHelper.WriteLine(personResponse.ToString());
            }

            //Act
            _personRepositoryMock.Setup(temp => temp.GetFilteredPersons(It.IsAny<Expression<Func<Person, bool>>>())).ReturnsAsync(persons);

            List<PersonResponse> personResponsesFromSearch = await _personService.GetFilteredPersons(nameof(Person.Name), "erk");

            _testOutputHelper.WriteLine("Actual");
            //print personResponsesFrom Get
            foreach (PersonResponse personResponse in personResponsesFromSearch)
            {
                _testOutputHelper.WriteLine(personResponse.ToString());
            }

            //Assert
            personResponsesFromSearch.Should().BeEquivalentTo(personResponseListExpected);
        }
        #endregion

        #region GetSortedPersons
        //when we sort based on PersonName in DESC, it should return the persons list in descending on PersonName
        [Fact]
        public async Task GetSortedPersons_ToBeSuccessful()
        {
            //Arrange
            List<Person> persons = new List<Person>()
            {
                _fixture.Build<Person>().With(temp => temp.Email, "asd@ss.com").With(temp => temp.Country,null as Country).Create(),
                _fixture.Build<Person>().With(temp => temp.Email, "asd2@ss.com").With(temp => temp.Country,null as Country).Create(),
                _fixture.Build<Person>().With(temp => temp.Email, "asd3@ss.com").With(temp => temp.Country,null as Country).Create()
            };

            List<PersonResponse> personResponseListExpected = persons.Select(temp => temp.ToPersonResponse()).ToList();

            _personRepositoryMock.Setup(temp => temp.GetAllPersons()).ReturnsAsync(persons);

            _testOutputHelper.WriteLine("Expected");
            //print personResponses
            foreach (PersonResponse personResponse in personResponseListExpected)
            {
                _testOutputHelper.WriteLine(personResponse.ToString());
            }

            List<PersonResponse> allPersons = await _personService.GetAllPersons();

            //Act
            List<PersonResponse> personResponseListFromSort = await _personService.GetSortedPersons(allPersons, nameof(Person.Name), SortOrderOptions.DESC);

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
                await _personService.UpdatePerson(personUpdateRequest);
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
                await _personService.UpdatePerson(personUpdateRequest);
            };
            //Assert
            await action.Should().ThrowAsync<ArgumentException>();
        }

        //when we supply null person name, it should throw ArgumentException
        [Fact]
        public async Task UpdatePerson_NullPersonName_ReturnsArgumentException()
        {
            //Arrange
            Person person = _fixture.Build<Person>()
             .With(temp => temp.Name, null as string)
             .With(temp => temp.Email, "someone@example.com")
             .With(temp => temp.Country, null as Country)
             .With(temp => temp.Gender, "Male")
             .Create();

            PersonResponse person_response_from_add = person.ToPersonResponse();

            PersonUpdateRequest person_update_request = person_response_from_add.ToPersonUpdateRequest();

            //Act
            var action = async () =>
            {
                await _personService.UpdatePerson(person_update_request);
            };

            //Assert
            await action.Should().ThrowAsync<ArgumentException>();
        }

        //First add a new person and try to update the person name and email
        [Fact]
        public async Task UpdatePerson_PersonFullDetails_ReturnsPersonResponse()
        {
            //Arrange
            Person person = _fixture.Build<Person>()
             .With(temp => temp.Email, "someone@example.com")
             .With(temp => temp.Country, null as Country)
             .With(temp => temp.Gender, "Male")
             .Create();

            PersonResponse person_response_expected = person.ToPersonResponse();

            PersonUpdateRequest person_update_request = person_response_expected.ToPersonUpdateRequest();

            _personRepositoryMock
             .Setup(temp => temp.UpdatePerson(It.IsAny<Person>()))
             .ReturnsAsync(person);

            _personRepositoryMock
             .Setup(temp => temp.GetPersonByPersonID(It.IsAny<Guid>()))
             .ReturnsAsync(person);

            //Act
            PersonResponse person_response_from_update = await _personService.UpdatePerson(person_update_request);

            //Assert
            person_response_from_update.Should().Be(person_response_expected);
        }

        #endregion

        #region DeletePerson

        //if we supply and valid PersonID, it should return true
        [Fact]
        public async Task DeletePerson_ValidPerson_ReturnsTrue()
        {
            //Arrange
            Person person = _fixture.Build<Person>()
             .With(temp => temp.Email, "someone@example.com")
             .With(temp => temp.Country, null as Country)
             .With(temp => temp.Gender, "Female")
             .Create();


            _personRepositoryMock
             .Setup(temp => temp.DeletePersonByPersonID(It.IsAny<Guid>()))
             .ReturnsAsync(true);

            _personRepositoryMock
             .Setup(temp => temp.GetPersonByPersonID(It.IsAny<Guid>()))
             .ReturnsAsync(person);

            //Act
            bool isDeleted = await _personService.DeletePerson(person.PersonID);

            //Assert
            isDeleted.Should().BeTrue();

        }

        //if we supply and invalid PersonID, it should return false
        [Fact]
        public async Task DeletePerson_InvalidPerson_ReturnsFalse()
        {
            //Act
            bool isDeleted = await _personService.DeletePerson(Guid.NewGuid());

            //Assert
            isDeleted.Should().BeFalse();
        }

        #endregion
    }
}
