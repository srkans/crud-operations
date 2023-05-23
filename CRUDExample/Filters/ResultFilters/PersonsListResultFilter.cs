using Microsoft.AspNetCore.Mvc.Filters;

namespace CRUDExample.Filters.ResultFilters
{
    public class PersonsListResultFilter : IAsyncResultFilter
    {
        private readonly ILogger<PersonsListResultFilter> _logger;

        public PersonsListResultFilter(ILogger<PersonsListResultFilter> logger)
        {
            _logger = logger;
        }
        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            //Before Logic
            _logger.LogInformation("{FilterName}.{MethodName} before",nameof(PersonsListResultFilter),nameof(OnResultExecutionAsync));
            context.HttpContext.Response.Headers["Last-Modified"] = DateTime.Now.ToString("dd-MM-yyyy HH:mm");

            await next();//call the subsequent filter or IActionResult

             //After Logic
            _logger.LogInformation("{FilterName}.{MethodName} after", nameof(PersonsListResultFilter), nameof(OnResultExecutionAsync));
           // context.HttpContext.Response.Headers["Last-Modified"] = DateTime.Now.ToString("dd-MM-yyyy HH:mm"); 
           //response client tarafında oluşturulmaya başlandıysa header'ı değiştirme
        }
    }
}
