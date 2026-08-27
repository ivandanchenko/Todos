using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Todos.Interfaces;
using Todos.Models;

namespace Todos.Filters
{
    public class CreateTodoValidationFilter : IAsyncActionFilter
    {
        private readonly ITodoService _todoService;

        public CreateTodoValidationFilter(ITodoService todoService)
        {
            _todoService = todoService;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var todo = context.ActionArguments.Values.OfType<Todo>().FirstOrDefault();
            if(todo is not null)
            {
                var errors = new Dictionary<string, string[]>();

                if (todo.Id <= 0)
                    errors.Add(nameof(Todo.Id), ["ID must be a positive integer greater than zero."]);
                else
                {
                    var IdExists= await _todoService.ReadById(todo.Id);
                    if(IdExists is not null)
                        errors.Add(nameof(Todo.Id), [$"Todo with {todo.Id} already exists."]);
                }

                if (todo.DueDate < DateTime.UtcNow)
                    errors.Add(nameof(Todo.DueDate), ["Cannot have due date in the past."]);

                if (todo.IsCompleted)
                    errors.Add(nameof(Todo.IsCompleted), ["Cannot add completed todo."]);


                if (errors.Count > 0)
                {
                    context.Result = new BadRequestObjectResult(new ValidationProblemDetails(errors));
                    return;
                }
            }
            await next();
        }
    }
}