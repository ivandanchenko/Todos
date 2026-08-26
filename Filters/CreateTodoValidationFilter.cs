using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Todos.Interfaces;
using Todos.Models;

namespace Todos.Filters
{
    public class CreateTodoValidationFilter : ActionFilterAttribute
    {
        private readonly ITodoService _taskService;

        public CreateTodoValidationFilter(ITodoService taskService)
        {
            _taskService = taskService;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var task = context.ActionArguments.Values.OfType<Todo>().FirstOrDefault();
            if(task is not null)
            {
                var errors = new Dictionary<string, string[]>();
                if (task.DueDate < DateTime.UtcNow)
                {
                    errors.Add(nameof(Todo.DueDate), ["Cannot have due date in the past."]);
                }
                if (task.IsCompleted)
                {
                    errors.Add(nameof(Todo.IsCompleted), ["Cannot add completed todo."]);
                }

                if (errors.Count > 0)
                {
                    context.Result = new BadRequestObjectResult(new ValidationProblemDetails(errors));
                    return;
                }
            }
        }
    }
}