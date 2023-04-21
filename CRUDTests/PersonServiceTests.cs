﻿using ServiceContracts;
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
            PersonAddRequest? personAddRequest = new PersonAddRequest() { Name = "Serkan", Email = "aa@ss.com" };

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

        #region GetAllPersons

        //The GetAllPersons() should return an empty list by default
        [Fact]
        public void GetAllPersons_EmptyList()
        {
            //Act
            List<PersonResponse> personResponsesFromGet = _personsService.GetAllPersons();

            //Assert
            Assert.Empty(personResponsesFromGet);
        }

        //add some persons then it should return same persons
        [Fact]
        public void GetAllPersons_ReturnAllPersons()
        {
            //Arrange
            CountryAddRequest countryAddRequest1 = new CountryAddRequest() { CountryName = "Türkiye" };
            CountryAddRequest countryAddRequest2 = new CountryAddRequest() { CountryName = "Australia" };

            CountryResponse countryResponse1 = _countriesService.AddCountry(countryAddRequest1);
            CountryResponse countryResponse2 = _countriesService.AddCountry(countryAddRequest2);

            PersonAddRequest personAddRequest1 = new PersonAddRequest() { Name = "Serkan", Email = "ss@asd.com", CountryID = countryResponse1.CountryID };
            PersonAddRequest personAddRequest2 = new PersonAddRequest() { Name = "Ahmet", Email = "as@asd.com", CountryID = countryResponse1.CountryID };
            PersonAddRequest personAddRequest3 = new PersonAddRequest() { Name = "Ali", Email = "bs@asd.com", CountryID = countryResponse2.CountryID };

            List<PersonAddRequest> personAddList = new List<PersonAddRequest> { personAddRequest1, personAddRequest2, personAddRequest3 };

            List<PersonResponse> personResponsesFromAdd = new List<PersonResponse>();

            foreach(PersonAddRequest personAddRequest in personAddList)
            {
                personResponsesFromAdd.Add(_personsService.AddPerson(personAddRequest));  
            }

            //Act
            List<PersonResponse> personResponsesFromGet = _personsService.GetAllPersons();

            //Assert
            foreach(PersonResponse personResponse in personResponsesFromAdd)
            {
                Assert.Contains(personResponse, personResponsesFromGet);
            }
        }

        #endregion
    }
}
