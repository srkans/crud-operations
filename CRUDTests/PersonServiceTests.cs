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

        public PersonServiceTests()
        {
            _personsService = new PersonsService();
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
    }
}
