﻿using System;
using Entities;
using ServiceContracts.DTO;
using ServiceContracts;
using System.ComponentModel.DataAnnotations;
using Services.Helpers;
using System.Runtime.CompilerServices;

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

            return person.ToPersonResponse();
        }

        public List<PersonResponse> GetFilteredPersons(string searchBy, string? searchString)
        {
            List<PersonResponse> allPersons = GetAllPersons(); //inside of the same class

            List<PersonResponse> matchingPersons = allPersons;

            if (string.IsNullOrEmpty(searchBy) && string.IsNullOrEmpty(searchString))
                return matchingPersons;

            switch(searchBy)
            {
                case nameof(Person.Name):
                    matchingPersons = allPersons.Where(temp => !string.IsNullOrEmpty(temp.Name) ? temp.Name.Contains(searchString,StringComparison.OrdinalIgnoreCase) : true).ToList();
                    break;
                case nameof(Person.Email):
                     matchingPersons = allPersons.Where(temp => !string.IsNullOrEmpty(temp.Email) ? temp.Email.Contains(searchString, StringComparison.OrdinalIgnoreCase) : true).ToList();
                    break;
                case nameof(Person.DateOfBirth):
                    matchingPersons = allPersons.Where(temp => (temp.DateOfBirth != null) ? temp.DateOfBirth.ToString().Contains(searchString, StringComparison.OrdinalIgnoreCase) : true).ToList();
                    break;
                case nameof(Person.Gender):
                    matchingPersons = allPersons.Where(temp => !string.IsNullOrEmpty(temp.Gender) ? temp.Gender.Contains(searchString, StringComparison.OrdinalIgnoreCase) : true).ToList();
                    break;
                case nameof(Person.CountryID):
                    matchingPersons = allPersons.Where(temp => !string.IsNullOrEmpty(temp.Country) ? temp.Country.Contains(searchString, StringComparison.OrdinalIgnoreCase) : true).ToList();
                    break;
                case nameof(Person.Address):
                    matchingPersons = allPersons.Where(temp => !string.IsNullOrEmpty(temp.Address) ? temp.Address.Contains(searchString, StringComparison.OrdinalIgnoreCase) : true).ToList();
                    break;
                default: matchingPersons = allPersons; break;
            }

            return matchingPersons;
        }
    }
}
