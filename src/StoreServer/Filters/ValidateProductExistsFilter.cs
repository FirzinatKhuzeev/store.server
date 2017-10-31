namespace StoreServer
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;

    public class ValidateProductExistsFilter : TypeFilterAttribute
    {
        public ValidateProductExistsFilter() : base(typeof(ValidateProductExistsFilterImpl))
        {
        }

        public class ValidateProductExistsFilterImpl : IAsyncActionFilter
        {
            private readonly IDocumentDbRepository productRepository;
            public ValidateProductExistsFilterImpl(IDocumentDbRepository repository)
            {
                productRepository = repository;
            }

            public async Task OnActionExecutionAsync(ActionExecutingContext context,ActionExecutionDelegate next)
            {
                if (context.ActionArguments.ContainsKey("id"))
                {
                    var id = context.ActionArguments["id"] as string;
                    if (!string.IsNullOrWhiteSpace(id))
                    {
                        var product = productRepository.GetAsync<Product>(id).Result;
                        if (product == null)
                        {
                            context.Result = new NotFoundObjectResult(id);
                        }
                    }
                }

                await next();
            }
        }
    }
}
