using ServiceContracts;
using ServiceContracts.DTO;
using Entities;
using System;
using Services;

namespace CRUDTests
{
    public class PersonServiceTests
    {
        private readonly IPersonsService _personsService;
        private readonly ICountriesService _countriesService;

        public PersonServiceTests()
        {
            _personsService = new PersonsService();
            _countriesService = new CountriesService();
        }

        #region AddPerson

        //when we supply null value as PersonAddRequest, it should throw ArgumentNullException
        [Fact]
        public void AddPerson_NullPerson_ReturnsArgumentNullException()
        {
            //Arrange
            PersonAddRequest personAddRequest = null;

            //Assert
            Assert.Throws<ArgumentNullException>(() =>
            {
                //Act
                _personsService.AddPerson(personAddRequest);
            });

        }

        //when we supply null value as PersonName, it should throw ArgumentException
        [Fact]
        public void AddPerson_NullPersonName_ReturnsArgumentException()
        {
            //Arrange
            PersonAddRequest personAddRequest = new PersonAddRequest() { Name = null};

            //Assert
            Assert.Throws<ArgumentException>(() =>
            {
                //Act
                _personsService.AddPerson(personAddRequest);
            });

        }

        //when we supply proper person details, it should insert the person into the persons list and it should return an object of PersonResponse, which include PersonID
        [Fact]
        public void AddPerson_ProperPersonObject_ReturnsPersonResponse()
        {
            //Arrange
            PersonAddRequest? personAddRequest = new PersonAddRequest() { Name = "Serkan" };

            //Act
            PersonResponse personResponseFromAdd = _personsService.AddPerson(personAddRequest);
            List<PersonResponse> personList = _personsService.GetAllPersons();

            //Assert
            Assert.True(personResponseFromAdd.PersonID != Guid.Empty);
            Assert.Contains(personResponseFromAdd, personList);

        }

        #endregion

        #region GetPersonByPersonID

        //if we supply null as personID, it should return null as personresponse
        [Fact]
        public void GetPersonByPersonID_NullPersonID_ReturnsNullPersonResponse()
        {
            //Arrange
            Guid? personID = null;

            //Act
            PersonResponse? personResponseFromGet = _personsService.GetPersonByPersonID(personID);

            //Assert
            Assert.Null(personResponseFromGet);
        }

        //if we supply proper personID
        [Fact]
        public void GetPersonByPersonID_ProperPersonID_ReturnsProperPersonResponse()
        {
            //Arrange
            CountryAddRequest countryAddRequest = new CountryAddRequest() { CountryName = "Canada" };
            CountryResponse countryResponse = _countriesService.AddCountry(countryAddRequest);

            //Act
            PersonAddRequest personAddRequest = new PersonAddRequest() { Name = "Serkan", Email="serkansacma@gmail.com", CountryID = countryResponse.CountryID};           
            PersonResponse personResponseFromAdd = _personsService.AddPerson(personAddRequest);

            PersonResponse? personResponseFromGet = _personsService.GetPersonByPersonID(personResponseFromAdd.PersonID);

            //Assert
            Assert.Equal(personResponseFromAdd,personResponseFromGet);
        }

        #endregion
    }
}
