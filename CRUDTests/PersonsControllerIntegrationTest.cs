using Fizzler;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;

namespace CRUDTests
{
    public class PersonsControllerIntegrationTest :IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public PersonsControllerIntegrationTest(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        #region Index
        [Fact]
        public async Task Index_ReturnsView()
        {
            //Arrange

            //Act
            HttpResponseMessage response = await _client.GetAsync("/Persons/Index");

            //Assert
            response.Should().BeSuccessful();

            string responseBody = await response.Content.ReadAsStringAsync();

            HtmlDocument html = new HtmlDocument(); //empty html document

            html.LoadHtml(responseBody); //html dom 

            var document = html.DocumentNode;

            document.QuerySelectorAll("table.persons").Should().NotBeNull();
        }
        #endregion
    }
}
