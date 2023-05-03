﻿using Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;

namespace CRUDExample.Controllers
{
    [Route("[controller]")]
    public class PersonsController : Controller
    {

        private readonly IPersonsService _personsService;
        private readonly ICountriesService _countriesService;

        public PersonsController(IPersonsService personsService, ICountriesService countriesService)
        {
            _personsService = personsService;
            _countriesService = countriesService;

        }

        [Route("[action]")]
        [Route("/")]
        public IActionResult Index(string searchBy, string? searchString = "", string sortBy = nameof(PersonResponse.Name), SortOrderOptions sortOrder = SortOrderOptions.ASC)
        {
            //Search
            ViewBag.SearchField = new Dictionary<string, string>()
            {
                { nameof(PersonResponse.Name), "Person Name" },
                { nameof(PersonResponse.Email), "Email" },
                { nameof(PersonResponse.DateOfBirth), "Date of Birth" },
                { nameof(PersonResponse.Gender), "Gender" },
                { nameof(PersonResponse.Country), "Country" },
                { nameof(PersonResponse.Address), "Address" }
            };

            List<PersonResponse> persons = _personsService.GetFilteredPersons(searchBy,searchString);

            ViewBag.CurrentSearchBy = searchBy;
            ViewBag.CurrentSearchString = searchString;

            //Sort
            List<PersonResponse> sortedPersons = _personsService.GetSortedPersons(persons,sortBy,sortOrder);

            ViewBag.CurrentSortBy = sortBy;
            ViewBag.CurrentSortOrder = sortOrder.ToString();

            return View(sortedPersons); //Views/Shared/Index.cshtml
        }

        //Executes when the user clicks on "Create Person" hyperlink
        [Route("[action]")]
        [HttpGet]
        public IActionResult Create()
        {
            List<CountryResponse> countries = _countriesService.GetAllCountries();
            ViewBag.Countries = countries.Select(temp =>
            new SelectListItem() { Text=temp.CountryName, Value=temp.CountryID.ToString()}
            );

            //new SelectListItem() { Text="Serkan", Value="1"}
            //<option value="1">Serkan</option>
            return View();
        }

        [Route("[action]")]
        [HttpPost]
        public IActionResult Create(PersonAddRequest personAddRequest)
        {
            if (!ModelState.IsValid)
            {
                List<CountryResponse> countries = _countriesService.GetAllCountries();
                ViewBag.Countries = countries.Select(temp =>
                new SelectListItem() { Text = temp.CountryName, Value = temp.CountryID.ToString() }
                );
                ViewBag.Errors = ModelState.Values.SelectMany(v=>v.Errors).Select(e=>e.ErrorMessage).ToList();
                return View();
            }

            PersonResponse personResponse = _personsService.AddPerson(personAddRequest);
            
            return RedirectToAction("Index","Persons");
        }

        [HttpGet]
        [Route("[action]/{personID}")] // ~/persons/edit/1
        public IActionResult Edit(Guid personID)
        {
            PersonResponse? personResponse = _personsService.GetPersonByPersonID(personID);

            if(personResponse == null)
            {
                return RedirectToAction("Index");
            }

            PersonUpdateRequest personUpdateRequest = personResponse.ToPersonUpdateRequest();

            List<CountryResponse> countries = _countriesService.GetAllCountries();
            ViewBag.Countries = countries.Select(temp =>
            new SelectListItem() { Text = temp.CountryName, Value = temp.CountryID.ToString() }
            );

            return View(personUpdateRequest);
        }

        [HttpPost]
        [Route("[action]/{personID}")] // ~/persons/edit/1
        public IActionResult Edit(PersonUpdateRequest personUpdateRequest)
        {
            PersonResponse? personResponse = _personsService.GetPersonByPersonID(personUpdateRequest.PersonID);

            if(personResponse == null)
            {
                return RedirectToAction("Index");
            }

            if(ModelState.IsValid)
            {
                PersonResponse updatedPerson = _personsService.UpdatePerson(personUpdateRequest);
                return RedirectToAction("Index");
            }
            else
            {
                List<CountryResponse> countries = _countriesService.GetAllCountries();
                ViewBag.Countries = countries.Select(temp =>
                new SelectListItem() { Text = temp.CountryName, Value = temp.CountryID.ToString() }
                );

                ViewBag.Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();

                return View(personResponse.ToPersonUpdateRequest());
            }
        }

        [HttpGet]
        [Route("[action]/{personID}")]
        public IActionResult Delete(Guid personID)
        {
            PersonResponse? personResponse = _personsService.GetPersonByPersonID(personID);

            if(personResponse == null)
            {
                return RedirectToAction("Index");
            }

            return View(personResponse);
        }

        [HttpPost]
        [Route("[action]/{personID}")] // ~/persons/delete/1
        public IActionResult Delete(PersonUpdateRequest personUpdateResult)
        {
            PersonResponse? personResponse = _personsService.GetPersonByPersonID(personUpdateResult.PersonID);
            
            if (personResponse == null)
            {
                return RedirectToAction("Index");
            }

            _personsService.DeletePerson(personResponse.PersonID);

            return RedirectToAction("Index");
                  
        }
    }
}
