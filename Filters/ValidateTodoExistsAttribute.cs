using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Todos.Interfaces;

namespace Todos.Filters
{
    public class ValidateTodoExistsAttribute : ActionFilterAttribute
    {
        private readonly ITodoService _todoService;

        public ValidateTodoExistsAttribute(ITodoService todoService)
        {
            _todoService = todoService;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.ActionArguments.TryGetValue("id", out var rawId) && rawId is int id)
            {
                var todo = await _todoService.ReadById(id);

                if (todo is null)
                {
                    context.Result = new NotFoundResult();
                    return;
                }
                context.HttpContext.Items["Todo"] = todo;
            }
            await next();
        }
    }
}
