using ServiceContracts;
using ServiceContracts.DTO;
using Entities;
using System;
using Services;
using Xunit.Abstractions;
using ServiceContracts.Enums;
using Microsoft.EntityFrameworkCore;
using EntityFrameworkCoreMock;
using Moq;

namespace CRUDTests
{
    public class PersonServiceTests
    {
        private readonly IPersonsService _personsService;
        private readonly ICountriesService _countriesService;
        private readonly ITestOutputHelper _testOutputHelper;

        public PersonServiceTests(ITestOutputHelper testOutputHelper)
        {
            var countriesInitialData = new List<Country>() { }; //empty collection as a data source
            var personsInitialData = new List<Person>() { };

            DbContextMock<ApplicationDbContext> dbContextMock = new DbContextMock<ApplicationDbContext>(new DbContextOptionsBuilder<ApplicationDbContext>().Options);

            ApplicationDbContext dbContext = dbContextMock.Object; //ApplicationDbContext is mock but behaves like original

            dbContextMock.CreateDbSetMock(temp => temp.Countries, countriesInitialData); //mock for DbSet<Country>
            dbContextMock.CreateDbSetMock(temp => temp.Persons, personsInitialData);

            _countriesService = new CountriesService(dbContext);
            _personsService = new PersonsService(dbContext, _countriesService);

            _testOutputHelper = testOutputHelper;
        }

        #region AddPerson

        //when we supply null value as PersonAddRequest, it should throw ArgumentNullException
        [Fact]
        public async Task AddPerson_NullPerson_ReturnsArgumentNullException()
        {
            //Arrange
            PersonAddRequest personAddRequest = null;

            //Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                //Act
                await _personsService.AddPerson(personAddRequest);
            });

        }

        //when we supply null value as PersonName, it should throw ArgumentException
        [Fact]
        public async Task AddPerson_NullPersonName_ReturnsArgumentException()
        {
            //Arrange
            PersonAddRequest personAddRequest = new PersonAddRequest() { Name = null };

            //Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                //Act
                await _personsService.AddPerson(personAddRequest);
            });

        }

        //when we supply proper person details, it should insert the person into the persons list and it should return an object of PersonResponse, which include PersonID
        [Fact]
        public async Task AddPerson_ProperPersonObject_ReturnsPersonResponse()
        {
            //Arrange
            PersonAddRequest? personAddRequest = new PersonAddRequest() { Name = "Serkan", Email = "aa@ss.com", Address = "asdasd", CountryID = Guid.NewGuid() };

            //Act
            PersonResponse personResponseFromAdd = await _personsService.AddPerson(personAddRequest);
            List<PersonResponse> personList = await _personsService.GetAllPersons();

            //Assert
            Assert.True(personResponseFromAdd.PersonID != Guid.Empty);
            Assert.Contains(personResponseFromAdd, personList);

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
            Assert.Null(personResponseFromGet);
        }

        //if we supply proper personID
        [Fact]
        public async Task GetPersonByPersonID_ProperPersonID_ReturnsProperPersonResponse()
        {
            //Arrange
            CountryAddRequest countryAddRequest = new CountryAddRequest() { CountryName = "Canada" };
            CountryResponse countryResponse = await _countriesService.AddCountry(countryAddRequest);

            //Act
            PersonAddRequest personAddRequest = new PersonAddRequest() { Name = "Serkan", Email = "serkansacma@gmail.com", CountryID = countryResponse.CountryID, Address = "asdasd" };
            PersonResponse personResponseFromAdd = await _personsService.AddPerson(personAddRequest);

            PersonResponse? personResponseFromGet = await _personsService.GetPersonByPersonID(personResponseFromAdd.PersonID);

            //Assert
            Assert.Equal(personResponseFromAdd, personResponseFromGet);
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
            Assert.Empty(personResponsesFromGet);
        }

        //add some persons then it should return same persons
        [Fact]
        public async Task GetAllPersons_ReturnAllPersons()
        {
            //Arrange
            CountryAddRequest countryAddRequest1 = new CountryAddRequest() { CountryName = "Türkiye" };
            CountryAddRequest countryAddRequest2 = new CountryAddRequest() { CountryName = "Australia" };

            CountryResponse countryResponse1 = await _countriesService.AddCountry(countryAddRequest1);
            CountryResponse countryResponse2 = await _countriesService.AddCountry(countryAddRequest2);

            PersonAddRequest personAddRequest1 = new PersonAddRequest() { Name = "Serkan", Email = "ss@asd.com", CountryID = countryResponse1.CountryID, Address = "asdasd" };
            PersonAddRequest personAddRequest2 = new PersonAddRequest() { Name = "Ahmet", Email = "as@asd.com", CountryID = countryResponse1.CountryID, Address = "asdasd" };
            PersonAddRequest personAddRequest3 = new PersonAddRequest() { Name = "Ali", Email = "bs@asd.com", CountryID = countryResponse2.CountryID, Address = "asdasd" };

            List<PersonAddRequest> personAddList = new List<PersonAddRequest> { personAddRequest1, personAddRequest2, personAddRequest3 };

            List<PersonResponse> personResponsesFromAdd = new List<PersonResponse>();

            foreach (PersonAddRequest personAddRequest in personAddList)
            {
                personResponsesFromAdd.Add(await _personsService.AddPerson(personAddRequest));
            }

            //Act
            List<PersonResponse> personResponsesFromGet = await _personsService.GetAllPersons();

            //Assert
            foreach (PersonResponse personResponse in personResponsesFromAdd)
            {
                Assert.Contains(personResponse, personResponsesFromGet);
            }

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
            CountryAddRequest countryAddRequest1 = new CountryAddRequest() { CountryName = "Türkiye" };
            CountryAddRequest countryAddRequest2 = new CountryAddRequest() { CountryName = "Australia" };

            CountryResponse countryResponse1 = await _countriesService.AddCountry(countryAddRequest1);
            CountryResponse countryResponse2 = await _countriesService.AddCountry(countryAddRequest2);

            PersonAddRequest personAddRequest1 = new PersonAddRequest() { Name = "Serkan", Email = "ss@asd.com", CountryID = countryResponse1.CountryID, Address = "asdasd" };
            PersonAddRequest personAddRequest2 = new PersonAddRequest() { Name = "Ahmet", Email = "as@asd.com", CountryID = countryResponse1.CountryID, Address = "asdasd" };
            PersonAddRequest personAddRequest3 = new PersonAddRequest() { Name = "Ali", Email = "bs@asd.com", CountryID = countryResponse2.CountryID, Address = "asdasd" };

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
            foreach (PersonResponse personResponse in personResponsesFromAdd)
            {
                Assert.Contains(personResponse, personResponsesFromSearch);
            }
        }

        //Search based on person name with some search string, it should return matching persons

        [Fact]
        public async Task GetFilteredPersons_SearchByPersonName_ReturnMatchingPersons()
        {
            //Arrange
            CountryAddRequest countryAddRequest1 = new CountryAddRequest() { CountryName = "Türkiye" };
            CountryAddRequest countryAddRequest2 = new CountryAddRequest() { CountryName = "Australia" };

            CountryResponse countryResponse1 = await _countriesService.AddCountry(countryAddRequest1);
            CountryResponse countryResponse2 = await _countriesService.AddCountry(countryAddRequest2);

            PersonAddRequest personAddRequest1 = new PersonAddRequest() { Name = "Serkan", Email = "ss@asd.com", CountryID = countryResponse1.CountryID , Address = "asdasd" };
            PersonAddRequest personAddRequest2 = new PersonAddRequest() { Name = "Ahmet", Email = "as@asd.com", CountryID = countryResponse1.CountryID, Address = "asdasd" };
            PersonAddRequest personAddRequest3 = new PersonAddRequest() { Name = "Ali", Email = "bs@asd.com", CountryID = countryResponse2.CountryID, Address = "asdasd" };

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
            foreach (PersonResponse personResponse in personResponsesFromAdd)
            {
                if(personResponse.Name != null)
                {
                    if (personResponse.Name.Contains("erk", StringComparison.OrdinalIgnoreCase))
                    {
                        Assert.Contains(personResponse, personResponsesFromSearch);
                    }
                }
            }
        }
        #endregion

        #region GetSortedPersons
        //when we sort based on PersonName in DESC, it should return the persons list in descending on PersonName
        [Fact]
        public async Task GetSortedPersons()
        {
            //Arrange
            CountryAddRequest countryAddRequest1 = new CountryAddRequest() { CountryName = "Türkiye" };
            CountryAddRequest countryAddRequest2 = new CountryAddRequest() { CountryName = "Australia" };

            CountryResponse countryResponse1 = await _countriesService.AddCountry(countryAddRequest1);
            CountryResponse countryResponse2 = await _countriesService.AddCountry(countryAddRequest2);

            PersonAddRequest personAddRequest1 = new PersonAddRequest() { Name = "Serkan", Email = "ss@asd.com", CountryID = countryResponse1.CountryID, Address = "asdasd" };
            PersonAddRequest personAddRequest2 = new PersonAddRequest() { Name = "Ahmet", Email = "as@asd.com", CountryID = countryResponse1.CountryID, Address = "asdasd" };
            PersonAddRequest personAddRequest3 = new PersonAddRequest() { Name = "Ali", Email = "bs@asd.com", CountryID = countryResponse2.CountryID, Address = "asdasd" };

            List<PersonAddRequest> personAddList = new List<PersonAddRequest> { personAddRequest1, personAddRequest2, personAddRequest3 };

            List<PersonResponse> personResponseListFromAdd = new List<PersonResponse>();

            foreach (PersonAddRequest personAddRequest in personAddList)
            {
                personResponseListFromAdd.Add(await _personsService.AddPerson(personAddRequest));
            }

            personResponseListFromAdd = personResponseListFromAdd.OrderByDescending(temp => temp.Name).ToList();

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
            for(int i = 0;i < personResponseListFromAdd.Count;i++)
            {
                Assert.Equal(personResponseListFromAdd[i], personResponseListFromSort[i]);
            }
        }

        #endregion

        #region UpdatePerson

        //when we supply null as PersonUpdateRequest, it should throw ArgumentNullException
        [Fact]
        public async Task UpdatePerson_NullPerson_ReturnsArgumentNullException()
        {
            //Arrange
            PersonUpdateRequest? personUpdateRequest = null;

            //Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                //Act
                await _personsService.UpdatePerson(personUpdateRequest);
            });
        }

        //when we supply invalid person id, it should throw ArgumentException
        [Fact]
        public async Task UpdatePerson_InvalidPersonId_ReturnsArgumentException()
        {
            //Arrange
            PersonUpdateRequest? personUpdateRequest = new PersonUpdateRequest() { PersonID = Guid.NewGuid() }; 

            //Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                //Act
               await _personsService.UpdatePerson(personUpdateRequest);
            });
        }

        //when we supply null person name, it should throw ArgumentException
        [Fact]
        public async Task UpdatePerson_NullPersonName_ReturnsArgumentException()
        {
            //Arrange
            CountryAddRequest countryAddRequest = new CountryAddRequest() { CountryName = "UK" };
            CountryResponse countryResponseFromAdd = await _countriesService.AddCountry(countryAddRequest);

            PersonAddRequest personAddRequest = new PersonAddRequest() { Name = "Serkan", CountryID = countryResponseFromAdd.CountryID, Email = "ss@aa.com", Gender = GenderOptions.Male, Address = "asdasd" };
            PersonResponse personResponseFromAdd = await _personsService.AddPerson(personAddRequest);

            PersonUpdateRequest? personUpdateRequest = personResponseFromAdd.ToPersonUpdateRequest();
            personUpdateRequest.Name = null;

            //Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                //Act
               await _personsService.UpdatePerson(personUpdateRequest);
            });
        }

        //First add a new person and try to update the person name and email
        [Fact]
        public async Task UpdatePerson_PersonFullDetails_ReturnsPersonResponse()
        {
            //Arrange
            CountryAddRequest countryAddRequest = new CountryAddRequest() { CountryName = "UK" };
            CountryResponse countryResponseFromAdd = await _countriesService.AddCountry(countryAddRequest);

            PersonAddRequest personAddRequest = new PersonAddRequest() { Name = "Serkan", CountryID = countryResponseFromAdd.CountryID, Email = "ss@aa.com", Gender = GenderOptions.Male, Address = "asdasd" };
            PersonResponse personResponseFromAdd = await _personsService.AddPerson(personAddRequest);

            PersonUpdateRequest? personUpdateRequest = personResponseFromAdd.ToPersonUpdateRequest();
            personUpdateRequest.Name = "Harsha";
            personUpdateRequest.Email = "harsha@kral.com";

            //Act
            PersonResponse personResponseFromUpdate = await _personsService.UpdatePerson(personUpdateRequest);

            PersonResponse? personResponseFromGet = await _personsService.GetPersonByPersonID(personResponseFromUpdate.PersonID);

            //Assert
            Assert.Equal(personResponseFromGet, personResponseFromUpdate);
   
        }

        #endregion

        #region DeletePerson

        //if we supply and valid PersonID, it should return true
        [Fact]
        public async Task DeletePerson_ValidPerson_ReturnsTrue()
        {
            CountryAddRequest countryAddRequest = new CountryAddRequest() { CountryName = "Türkiye" };
            CountryResponse countryResponseFromAdd = await _countriesService.AddCountry(countryAddRequest);

            PersonAddRequest personAddRequest = new PersonAddRequest() { Name = "Serkan", Email = "ss@aa.com", Gender = GenderOptions.Male, CountryID = countryResponseFromAdd.CountryID, Address = "asdasd" };
            PersonResponse personResponseFromAdd = await _personsService.AddPerson(personAddRequest);

            //Act
            bool isDeleted = await _personsService.DeletePerson(personResponseFromAdd.PersonID);

            //Assert
            Assert.True(isDeleted);

        }

        //if we supply and invalid PersonID, it should return false
        [Fact]
        public async Task DeletePerson_InvalidPerson_ReturnsFalse()
        {
            CountryAddRequest countryAddRequest = new CountryAddRequest() { CountryName = "Türkiye" };
            CountryResponse countryResponseFromAdd = await _countriesService.AddCountry(countryAddRequest);

            PersonAddRequest personAddRequest = new PersonAddRequest() { Name = "Serkan", Email = "ss@aa.com", Gender = GenderOptions.Male, CountryID = countryResponseFromAdd.CountryID, Address = "asdasd" };
            PersonResponse personResponseFromAdd = await _personsService.AddPerson(personAddRequest);

            //Act
            bool isDeleted = await _personsService.DeletePerson(Guid.NewGuid());

            //Assert
            Assert.False(isDeleted);

        }

        #endregion
    }
}
