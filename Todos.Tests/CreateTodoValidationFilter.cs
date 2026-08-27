using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using FakeItEasy;
using Todos.Filters;
using Todos.Interfaces;
using Todos.Models;

namespace Todos.Tests;

public class CreateTodoValidationFilterTests
{
    private readonly ITodoService _fakeTodoService;
    private readonly CreateTodoValidationFilter _filter;

    public CreateTodoValidationFilterTests()
    {
        _fakeTodoService = A.Fake<ITodoService>();
        _filter = new CreateTodoValidationFilter(_fakeTodoService);
    }

    private ActionExecutingContext CreateFilterContext(Todo todo)
    {
        var actionContext = new ActionContext(
            new DefaultHttpContext(),
            new RouteData(),
            new ActionDescriptor()
        );

        var context = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?> { { "todo", todo } }!,
            new object()
        );

        return context;
    }
    private Task<ActionExecutedContext> NextContextDelegate()
    {
        var actionContext = new ActionContext(
            new DefaultHttpContext(),
            new RouteData(),
            new ActionDescriptor()
        );
        return Task.FromResult(new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), new object()));
    }

    [Fact]
    public async Task OnActionExecuting_WithValidTodo_DoesNotSetResult()
    {
        var validTodo = new Todo(1, "Valid", DateTime.UtcNow.AddDays(1), false);
        A.CallTo(() => _fakeTodoService.ReadById(validTodo.Id)).Returns(Task.FromResult<Todo?>(null));
        var context = CreateFilterContext(validTodo);

        await _filter.OnActionExecutionAsync(context, NextContextDelegate);

        Assert.Null(context.Result);
    }

    [Fact]
    public async Task OnActionExecuting_WithInvalidId_ReturnsBadRequestWithErrors()
    {
        var invalidTodo = new Todo(0, "Invalid ID", DateTime.UtcNow.AddDays(1), false);
        var context = CreateFilterContext(invalidTodo);

        await _filter.OnActionExecutionAsync(context, NextContextDelegate);

        var result = Assert.IsType<BadRequestObjectResult>(context.Result);
        var details = Assert.IsType<ValidationProblemDetails>(result.Value);
        Assert.True(details.Errors.ContainsKey(nameof(Todo.Id)));
    }

    [Fact]
    public async Task OnActionExecuting_WithDuplicateId_ReturnsBadRequestWithErrorsAsync()
    {
        var duplicateTodo = new Todo(5, "Duplicate ID", DateTime.UtcNow.AddDays(1), false);
        A.CallTo(() => _fakeTodoService.ReadById(5)).Returns(Task.FromResult<Todo?>(duplicateTodo));
        var context = CreateFilterContext(duplicateTodo);

        await _filter.OnActionExecutionAsync(context, NextContextDelegate);

        var result = Assert.IsType<BadRequestObjectResult>(context.Result);
        var details = Assert.IsType<ValidationProblemDetails>(result.Value);
        Assert.True(details.Errors.ContainsKey(nameof(Todo.Id)));
    }

    [Fact]
    public async Task OnActionExecuting_WithPastDueDate_ReturnsBadRequestWithErrorsAsync()
    {
        var invalidTodo = new Todo(1, "Past Date", DateTime.UtcNow.AddDays(-1), false);
        A.CallTo(() => _fakeTodoService.ReadById(1)).Returns(Task.FromResult<Todo?>(null));
        var context = CreateFilterContext(invalidTodo);

        await _filter.OnActionExecutionAsync(context, NextContextDelegate);

        var result = Assert.IsType<BadRequestObjectResult>(context.Result);
        var details = Assert.IsType<ValidationProblemDetails>(result.Value);
        Assert.True(details.Errors.ContainsKey(nameof(Todo.DueDate)));
    }

    [Fact]
    public async Task OnActionExecuting_AlreadyCompleted_ReturnsBadRequestWithErrorsAsync()
    {
        var invalidTodo = new Todo(1, "Completed", DateTime.UtcNow.AddDays(1), true);
        A.CallTo(() => _fakeTodoService.ReadById(1)).Returns(Task.FromResult<Todo?>(null));
        var context = CreateFilterContext(invalidTodo);

        await _filter.OnActionExecutionAsync(context, NextContextDelegate);
        
        var result = Assert.IsType<BadRequestObjectResult>(context.Result);
        var details = Assert.IsType<ValidationProblemDetails>(result.Value);
        Assert.True(details.Errors.ContainsKey(nameof(Todo.IsCompleted)));
    }
}
