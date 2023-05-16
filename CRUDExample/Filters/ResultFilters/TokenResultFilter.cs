using Microsoft.AspNetCore.Mvc.Filters;

namespace CRUDExample.Filters.ResultFilters
{
    public class TokenResultFilter : IAsyncResultFilter
    {
        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {

            context.HttpContext.Response.Cookies.Append("Auth-Key","something");

            await next();
        }
    }
}
