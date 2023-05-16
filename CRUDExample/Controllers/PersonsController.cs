using CRUDExample.Filters.ActionFilters;
using CRUDExample.Filters.AuthorizationFilters;
using CRUDExample.Filters.ResourceFilters;
using CRUDExample.Filters.ResultFilters;
using Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Rotativa.AspNetCore;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;

namespace CRUDExample.Controllers
{
    [Route("[controller]")]
    [TypeFilter(typeof(ResponseHeaderActionFilter), Arguments = new object[] { "my-key-from-controller", "my-value-from-controller",3 },Order = 3)]
    public class PersonsController : Controller
    {

        private readonly IPersonsService _personsService;
        private readonly ICountriesService _countriesService;
        private readonly ILogger<PersonsController> _logger;

        public PersonsController(IPersonsService personsService, ICountriesService countriesService, ILogger<PersonsController> logger)
        {
            _personsService = personsService;
            _countriesService = countriesService;
            _logger = logger;
        }

        [Route("[action]")]
        [Route("/")]
        [TypeFilter(typeof(PersonsListActionFilter),Order =4)]
        [TypeFilter(typeof(ResponseHeaderActionFilter), Arguments = new object[] {"My-Key-FromAction", "My-Value-FromAction", 1},Order = 1)]
        [TypeFilter(typeof(PersonsListResultFilter))]
        public async Task<IActionResult> Index(string searchBy, string? searchString = "", string sortBy = nameof(PersonResponse.Name), SortOrderOptions sortOrder = SortOrderOptions.ASC)
        {
            _logger.LogInformation("Index action method of PersonsController");

            _logger.LogDebug($"searchBy:{searchBy}, searchString:{searchString}, sortBy:{sortBy}, sortOrder:{sortOrder}");

            List<PersonResponse> persons = await _personsService.GetFilteredPersons(searchBy, searchString);

            //Sort
            List<PersonResponse> sortedPersons = await _personsService.GetSortedPersons(persons, sortBy, sortOrder);

            return View(sortedPersons); //Views/Shared/Index.cshtml
        }

        //Executes when the user clicks on "Create Person" hyperlink
        [Route("[action]")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            List<CountryResponse> countries = await _countriesService.GetAllCountries();
            ViewBag.Countries = countries.Select(temp =>
            new SelectListItem() { Text = temp.CountryName, Value = temp.CountryID.ToString() }
            );

            //new SelectListItem() { Text="Serkan", Value="1"}
            //<option value="1">Serkan</option>
            return View();
        }

        [Route("[action]")]
        [HttpPost]
        [TypeFilter(typeof(PersonCreateAndEditPostActionFilter))]
        [TypeFilter(typeof(FeatureDisabledResourceFilter), Arguments = new object[] {false})] //sonra false yapmak kolay olsun diye
        public async Task<IActionResult> Create(PersonAddRequest personRequest)
        {
            PersonResponse personResponse = await _personsService.AddPerson(personRequest);

            return RedirectToAction("Index", "Persons");
        }

        [HttpGet]
        [Route("[action]/{personID}")] // ~/persons/edit/1
        [TypeFilter(typeof(TokenResultFilter))]
        public async Task<IActionResult> Edit(Guid personID)
        {
            PersonResponse? personResponse = await _personsService.GetPersonByPersonID(personID);

            if (personResponse == null)
            {
                return RedirectToAction("Index");
            }

            PersonUpdateRequest personUpdateRequest = personResponse.ToPersonUpdateRequest();

            List<CountryResponse> countries = await _countriesService.GetAllCountries();
            ViewBag.Countries = countries.Select(temp =>
            new SelectListItem() { Text = temp.CountryName, Value = temp.CountryID.ToString() }
            );

            return View(personUpdateRequest);
        }

        [HttpPost]
        [Route("[action]/{personID}")] // ~/persons/edit/1
        [TypeFilter(typeof(PersonCreateAndEditPostActionFilter))]
        [TypeFilter(typeof(TokenAuthorizationFilter))]
        public async Task<IActionResult> Edit(PersonUpdateRequest personRequest)
        {
            PersonResponse? personResponse = await _personsService.GetPersonByPersonID(personRequest.PersonID);

            if (personResponse == null)
            {
                return RedirectToAction("Index");
            }

            PersonResponse updatedPerson = await _personsService.UpdatePerson(personRequest);

            return RedirectToAction("Index");
        }

        [HttpGet]
        [Route("[action]/{personID}")]
        public async Task<IActionResult> Delete(Guid personID)
        {
            PersonResponse? personResponse = await _personsService.GetPersonByPersonID(personID);

            if (personResponse == null)
            {
                return RedirectToAction("Index");
            }

            return View(personResponse);
        }

        [HttpPost]
        [Route("[action]/{personID}")] // ~/persons/delete/1
        public async Task<IActionResult> Delete(PersonUpdateRequest personUpdateResult)
        {
            PersonResponse? personResponse = await _personsService.GetPersonByPersonID(personUpdateResult.PersonID);

            if (personResponse == null)
            {
                return RedirectToAction("Index");
            }

            await _personsService.DeletePerson(personResponse.PersonID);

            return RedirectToAction("Index");

        }

        [Route("[action]")]
        public async Task<IActionResult> PersonsPDF()
        {
            List<PersonResponse> persons = await _personsService.GetAllPersons();

            return new ViewAsPdf("PersonsPDF", persons, ViewData)
            {
                PageMargins = new Rotativa.AspNetCore.Options.Margins() { Top = 20, Bottom = 20, Left = 20, Right = 20 },
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape
            };
        }

        [Route("[action]")]
        public async Task<IActionResult> PersonsCSV()
        {
           MemoryStream memoryStream = await _personsService.GetPersonsCSV();
            return File(memoryStream, "app/octet-stream", "persons.csv");
        }

        [Route("[action]")]
        public async Task<IActionResult> PersonsExcel()
        {
            MemoryStream memoryStream = await _personsService.GetPersonsExcel();
            return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "persons.xlsx"); //excel mime type
        }
    }
}
