using System;
using Entities;
using ServiceContracts.DTO;
using ServiceContracts;

namespace Services
{
    public class PersonsService : IPersonsService
    {
        private readonly List<Person> _persons;
        private readonly ICountriesService _countriesService;

        public PersonsService()
        {
            _persons = new List<Person>();
            _countriesService = new CountriesService();
        }

        private PersonResponse ConvertPersonToPersonResponse(Person person)
        {
            PersonResponse personResponse = person.ToPersonResponse();
            personResponse.Country = _countriesService.GetCountryById(person.CountryID)?.CountryName;

            return personResponse;
        }
        public PersonResponse AddPerson(PersonAddRequest? personAddRequest)
        {      

           if(personAddRequest == null)
            {
                throw new ArgumentNullException(nameof(personAddRequest));
            }

           if(personAddRequest.Name == null) //string.IsNullOrEmpty
            {
                throw new ArgumentException("Person name can't be blank");
            }

            //convert personaddrequest to person type

            Person person = personAddRequest.ToPerson();

            //generate new PersonID

            person.PersonID = Guid.NewGuid();

            _persons.Add(person);

            return ConvertPersonToPersonResponse(person);

        }

        public List<PersonResponse> GetAllPersons()
        {
            throw new NotImplementedException();
        }
    }
}
