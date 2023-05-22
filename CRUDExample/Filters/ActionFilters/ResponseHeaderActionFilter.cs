using Microsoft.AspNetCore.Mvc.Filters;

namespace CRUDExample.Filters.ActionFilters
{
    public class ResponseHeaderFilterFactoryAttribute : Attribute, IFilterFactory
    {
        public bool IsReusable => false;
        private string? _key { get; set; }
        private string? _value { get; set; }
        private int _order { get; }

        public ResponseHeaderFilterFactoryAttribute(string key, string value, int order)
        {
            _key = key;
            _value = value;
            _order = order;
        }

        //controller - >filter factory -> filter
        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            //return filter object
            var filter = serviceProvider.GetRequiredService<ResponseHeaderActionFilter>();
            filter.Key = _key;
            filter.Value = _value;
            filter.Order = _order;
            return filter;
        }
    }
    public class ResponseHeaderActionFilter : IAsyncActionFilter, IOrderedFilter
    {
        private readonly ILogger<ResponseHeaderActionFilter> _logger;
        public  string Key { get; set; }
        public  string Value { get; set; }
        public int Order { get; set; }

        public ResponseHeaderActionFilter(ILogger<ResponseHeaderActionFilter> logger)
        {
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            //before logic
            _logger.LogInformation("{FilterName}.{MethodName} before-method", nameof(ResponseHeaderActionFilter), nameof(OnActionExecutionAsync));

            await next();//calls the subsequent filter or action

            //after logic
            _logger.LogInformation("{FilterName}.{MethodName} after-method", nameof(ResponseHeaderActionFilter), nameof(OnActionExecutionAsync));

            context.HttpContext.Response.Headers[Key] = Value;

        }
    }
}
