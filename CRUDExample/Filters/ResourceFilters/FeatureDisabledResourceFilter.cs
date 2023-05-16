using CRUDExample.Filters.ResultFilters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CRUDExample.Filters.ResourceFilters
{
    public class FeatureDisabledResourceFilter : IAsyncResourceFilter
    {
        private readonly ILogger<FeatureDisabledResourceFilter> _logger;
        private readonly bool _isDisabled;

        public FeatureDisabledResourceFilter(ILogger<FeatureDisabledResourceFilter> logger, bool isDisabled = true)
        {
            _isDisabled = isDisabled;
            _logger = logger;
        }
        public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
        {
            //Before logic
            _logger.LogInformation("{FilterName}.{MethodName} before", nameof(FeatureDisabledResourceFilter), nameof(OnResourceExecutionAsync));

            if(_isDisabled)
            {
               //context.Result = new NotFoundResult(); //404 not found -- kalıcı olarak kaldırıldı

               context.Result = new StatusCodeResult(501); //not implemented -- gelecekte çalışacak henüz bitmedi
            }
            else
            {
               await next();
            }

            //After logic
            _logger.LogInformation("{FilterName}.{MethodName} after", nameof(FeatureDisabledResourceFilter), nameof(OnResourceExecutionAsync));
        }
    }
}
