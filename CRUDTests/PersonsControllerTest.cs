using CRUDExample.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CRUDTests
{
    public class PersonsControllerTest
    {
        private readonly IPersonsService _personsService;
        private readonly ICountriesService _countriesService;
        private readonly ILogger<PersonsController> _logger;

        private readonly Mock<IPersonsService> _personsServiceMock;
        private readonly Mock<ICountriesService> _countriesServiceMock;
        private readonly Mock<ILogger<PersonsController>> _loggerMock;

        private readonly IFixture _fixture;

        public PersonsControllerTest()
        {
            _fixture = new Fixture();

            _countriesServiceMock = new Mock<ICountriesService>();
            _personsServiceMock = new Mock<IPersonsService>();
            _loggerMock = new Mock<ILogger<PersonsController>>();

            _countriesService = _countriesServiceMock.Object;
            _personsService = _personsServiceMock.Object;
            _logger = _loggerMock.Object;
        }

        #region Index

        [Fact]
        public async Task Index_ReturnsIndexViewWithPersonsList()
        {
            //Arrange
            List<PersonResponse> personResponseList = _fixture.Create<List<PersonResponse>>();

            PersonsController personsController = new PersonsController(_personsService, _countriesService,_logger);

            _personsServiceMock.Setup(temp => temp.GetFilteredPersons(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(personResponseList);

            _personsServiceMock.Setup(temp => temp.GetSortedPersons(It.IsAny<List<PersonResponse>>(), It.IsAny<string>(), It.IsAny<SortOrderOptions>())).ReturnsAsync(personResponseList);

            //Act
            IActionResult result = await personsController.Index(_fixture.Create<string>(),
                                          _fixture.Create<string>(),
                                          _fixture.Create<string>(),
                                          _fixture.Create<SortOrderOptions>());

            //Assert
            ViewResult viewResult = Assert.IsType<ViewResult>(result);
            viewResult.ViewData.Model.Should().BeAssignableTo<IEnumerable<PersonResponse>>();
            viewResult.ViewData.Model.Should().Be(personResponseList);
        }

        #endregion

        #region Create
        [Fact]
        public async Task Create_IfModelErrors_ReturnsCreateView()
        {
            //Arrange
            PersonAddRequest personAddRequest = _fixture.Create<PersonAddRequest>();

            PersonResponse personResponse = _fixture.Create<PersonResponse>();

            List<CountryResponse> countries = _fixture.Create<List<CountryResponse>>();

            _countriesServiceMock.Setup(temp => temp.GetAllCountries()).ReturnsAsync(countries);

            _personsServiceMock.Setup(temp => temp.AddPerson(It.IsAny<PersonAddRequest>())).ReturnsAsync(personResponse);

            PersonsController personsController = new PersonsController(_personsService, _countriesService,null);

            //Act
            personsController.ModelState.AddModelError("Name", "Person name can't be blank"); //model error has been manually added

            IActionResult result = await personsController.Create(personAddRequest);

            //Assert
            ViewResult viewResult = Assert.IsType<ViewResult>(result);
            viewResult.ViewData.Model.Should().BeAssignableTo<PersonAddRequest>();
            viewResult.ViewData.Model.Should().Be(personAddRequest);
        }

        [Fact]
        public async Task Create_IfNoModelErrors_ReturnsRedirectToIndex()
        {
            //Arrange
            PersonAddRequest personAddRequest = _fixture.Create<PersonAddRequest>();

            PersonResponse personResponse = _fixture.Create<PersonResponse>();

            List<CountryResponse> countries = _fixture.Create<List<CountryResponse>>();

            _countriesServiceMock.Setup(temp => temp.GetAllCountries()).ReturnsAsync(countries);

            _personsServiceMock.Setup(temp => temp.AddPerson(It.IsAny<PersonAddRequest>())).ReturnsAsync(personResponse);

            PersonsController personsController = new PersonsController(_personsService, _countriesService,null);

            //Act
            IActionResult result = await personsController.Create(personAddRequest);

            //Assert
            RedirectToActionResult redirectResult = Assert.IsType<RedirectToActionResult>(result);

            redirectResult.ActionName.Should().Be("Index");
        }

        #endregion

    }
}
