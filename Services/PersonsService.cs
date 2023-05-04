using System;
using Entities;
using ServiceContracts.DTO;
using ServiceContracts;
using System.ComponentModel.DataAnnotations;
using Services.Helpers;
using System.Runtime.CompilerServices;
using ServiceContracts.Enums;

namespace Services
{
    public class PersonsService : IPersonsService
    {
        private readonly List<Person> _persons;
        private readonly ICountriesService _countriesService;

        public PersonsService(bool initialize = true)
        {
            _persons = new List<Person>();
            _countriesService = new CountriesService();

            if(initialize)
            {
                _persons.AddRange(new List<Person>() { 
                    new Person() {PersonID = Guid.Parse("38BA5102-EB8C-4162-B564-F92316809E3D"),Name = "Serkan", Email = "amacgahey0@eepurl.com",DateOfBirth = DateTime.Parse("1996-4-29"), Gender ="Male" ,Address = "3175 Hollow Ridge Drive", ReceiveNewsLetters = false, CountryID = Guid.Parse("A5082F1A-8A91-480C-B709-E2824286F04F")},
                    new Person() {PersonID = Guid.Parse("B8CAB606-47C3-44E4-8472-F419E2E2B4A6"), Name = "Betül", Email = "abullock1@samsung.com",DateOfBirth =  DateTime.Parse("1997-9-19"), Gender ="Female" ,Address = "3175 Hollow Ridge Drive", ReceiveNewsLetters = true, CountryID = Guid.Parse("A5082F1A-8A91-480C-B709-E2824286F04F")},
                    new Person() {PersonID = Guid.Parse("DFD08699-88C0-4C16-8C18-20BDE0672B02"), Name = "Beau", Email = "bavard5@merriam-webster.com",DateOfBirth = DateTime.Parse("1990/5/17"), Gender ="Male" ,Address = "00668 Hagan Terrace", ReceiveNewsLetters = true, CountryID = Guid.Parse("B76053EB-EEC4-4258-8147-A3C514C03BC2")},
                    new Person() {PersonID = Guid.Parse("99294CDA-B9B0-45C6-931A-E88C5030931C"), Name = "Ayşe", Email = "yy@aa.com",DateOfBirth = DateTime.Parse("1997/9/19"), Gender ="Female" ,Address = "3135 Ridge Drive", ReceiveNewsLetters = false, CountryID = Guid.Parse("EF0BC4B1-EDF1-4598-8A5C-45EE61A74AF4")},
                    new Person() {PersonID = Guid.Parse("098EC21F-C786-4D8D-96CB-57369D257A2B"), Name = "Geno", Email = "gsympson8@booking.com",DateOfBirth = DateTime.Parse("1998/1/20"), Gender ="Male" ,Address = "5 Bluejay Way", ReceiveNewsLetters = true, CountryID = Guid.Parse("60A0F453-D0DE-44F8-BCDA-53BA837E0148")},
                    new Person() {PersonID = Guid.Parse("E6BBAD59-C227-45AF-AF9C-60D8AD345B2E"), Name = "Dorey", Email = "dlukes9@github.com",DateOfBirth = DateTime.Parse("1996/4/13"), Gender ="Male" ,Address = "59 Nancy Point", ReceiveNewsLetters = false, CountryID = Guid.Parse("3E3F9AC3-D1DE-4DF1-B1F0-F10A014AEC3D") }
                });
            }
        }

        private PersonResponse ConvertPersonToPersonResponse(Person person)
        {
            PersonResponse personResponse = person.ToPersonResponse();
            personResponse.Country = _countriesService.GetCountryById(person.CountryID)?.CountryName;

            return personResponse;
        }
        public PersonResponse AddPerson(PersonAddRequest? personAddRequest)
        {
            //Model Validations
            ValidationHelper.ModelValidation(personAddRequest);

            //convert personaddrequest to person type

            Person person = personAddRequest.ToPerson();

            //generate new PersonID

            person.PersonID = Guid.NewGuid();

            _persons.Add(person);

            return ConvertPersonToPersonResponse(person);

        }

        public List<PersonResponse> GetAllPersons()
        {
             return _persons.Select(temp => temp.ToPersonResponse()).ToList();
        }

        public PersonResponse? GetPersonByPersonID(Guid? personID)
        {
           if(personID == null)
            {
                return null;
            }

           Person? person = _persons.FirstOrDefault(temp=>temp.PersonID == personID);

            if(person == null)
            {
                return null;
            }

            return ConvertPersonToPersonResponse(person);
        }

        public List<PersonResponse> GetFilteredPersons(string searchBy, string? searchString)
        {
            List<PersonResponse> allPersons = GetAllPersons(); //inside of the same class

            List<PersonResponse> matchingPersons = allPersons;

            if (string.IsNullOrEmpty(searchString))
                return matchingPersons;

            switch(searchBy)
            {
                case nameof(PersonResponse.Name):
                    matchingPersons = allPersons.Where(temp => !string.IsNullOrEmpty(temp.Name) ? temp.Name.Contains(searchString,StringComparison.OrdinalIgnoreCase) : true).ToList();
                    break;
                case nameof(PersonResponse.Email):
                     matchingPersons = allPersons.Where(temp => !string.IsNullOrEmpty(temp.Email) ? temp.Email.Contains(searchString, StringComparison.OrdinalIgnoreCase) : true).ToList();
                    break;
                case nameof(PersonResponse.DateOfBirth):
                    matchingPersons = allPersons.Where(temp => (temp.DateOfBirth != null) ? temp.DateOfBirth.ToString().Contains(searchString, StringComparison.OrdinalIgnoreCase) : true).ToList();
                    break;
                case nameof(PersonResponse.Gender):
                    matchingPersons = allPersons.Where(temp => !string.IsNullOrEmpty(temp.Gender) ? temp.Gender.Contains(searchString, StringComparison.OrdinalIgnoreCase) : true).ToList();
                    break;
                case nameof(PersonResponse.CountryID):
                    matchingPersons = allPersons.Where(temp => !string.IsNullOrEmpty(temp.Country) ? temp.Country.Contains(searchString, StringComparison.OrdinalIgnoreCase) : true).ToList();
                    break;
                case nameof(PersonResponse.Address):
                    matchingPersons = allPersons.Where(temp => !string.IsNullOrEmpty(temp.Address) ? temp.Address.Contains(searchString, StringComparison.OrdinalIgnoreCase) : true).ToList();
                    break;
                default: matchingPersons = allPersons; break;
            }

            return matchingPersons;
        }

        public List<PersonResponse> GetSortedPersons(List<PersonResponse> allPersons, string sortBy, SortOrderOptions sortOrder)
        {
            if (string.IsNullOrEmpty(sortBy))
                return allPersons;

            List<PersonResponse> sortedPersons = (sortBy, sortOrder) switch
            {
                (nameof(PersonResponse.Name), SortOrderOptions.ASC) => allPersons.OrderBy(temp => temp.Name, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.Name), SortOrderOptions.DESC) => allPersons.OrderByDescending(temp => temp.Name, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.Email), SortOrderOptions.ASC) => allPersons.OrderBy(temp => temp.Email, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.Email), SortOrderOptions.DESC) => allPersons.OrderByDescending(temp => temp.Email, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.DateOfBirth), SortOrderOptions.ASC) => allPersons.OrderBy(temp => temp.DateOfBirth).ToList(),

                (nameof(PersonResponse.DateOfBirth), SortOrderOptions.DESC) => allPersons.OrderByDescending(temp => temp.DateOfBirth).ToList(),

                (nameof(PersonResponse.Age), SortOrderOptions.ASC) => allPersons.OrderBy(temp => temp.Age).ToList(),

                (nameof(PersonResponse.Age), SortOrderOptions.DESC) => allPersons.OrderByDescending(temp => temp.Age).ToList(),

                (nameof(PersonResponse.Gender), SortOrderOptions.ASC) => allPersons.OrderBy(temp => temp.Gender, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.Gender), SortOrderOptions.DESC) => allPersons.OrderByDescending(temp => temp.Gender, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.Country), SortOrderOptions.ASC) => allPersons.OrderBy(temp => temp.Country, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.Country), SortOrderOptions.DESC) => allPersons.OrderByDescending(temp => temp.Country, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.Address), SortOrderOptions.ASC) => allPersons.OrderBy(temp => temp.Address, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.Address), SortOrderOptions.DESC) => allPersons.OrderByDescending(temp => temp.Address, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.ReceiveNewsLetters), SortOrderOptions.ASC) => allPersons.OrderBy(temp => temp.ReceiveNewsLetters).ToList(),

                (nameof(PersonResponse.ReceiveNewsLetters), SortOrderOptions.DESC) => allPersons.OrderByDescending(temp => temp.ReceiveNewsLetters).ToList(),

                _ => allPersons
            };

            return sortedPersons;
        }

        public PersonResponse UpdatePerson(PersonUpdateRequest? personUpdateRequest)
        {
            if (personUpdateRequest == null)
                throw new ArgumentNullException(nameof(Person));

            //Validation
            ValidationHelper.ModelValidation(personUpdateRequest);

            //get matching person object to update
            Person? matchingPerson = _persons.FirstOrDefault(temp=>temp.PersonID == personUpdateRequest.PersonID);

            if(matchingPerson == null)
            {
                throw new ArgumentException("Given person id doesn't exist");
            }

            //update all details
            matchingPerson.Name = personUpdateRequest.Name;
            matchingPerson.Email = personUpdateRequest.Email;
            matchingPerson.DateOfBirth = personUpdateRequest.DateOfBirth;
            matchingPerson.Gender = personUpdateRequest.Gender.ToString();
            matchingPerson.CountryID = personUpdateRequest.CountryID;
            matchingPerson.Address = personUpdateRequest.Address;
            matchingPerson.ReceiveNewsLetters = personUpdateRequest.ReceiveNewsLetters;

            return ConvertPersonToPersonResponse(matchingPerson);
        }

        public bool DeletePerson(Guid personID)
        {
            if (personID == null)
                throw new ArgumentNullException(nameof(personID));

           Person? person = _persons.FirstOrDefault(temp => temp.PersonID == personID);

            if (person == null)
                return false;

            _persons.RemoveAll(temp => temp.PersonID == personID);
            return true;
        }
    }
}
