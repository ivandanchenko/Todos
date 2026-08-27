using Todos.Controllers;
using Todos.Interfaces;
using FakeItEasy;
using Todos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace Todos.Tests;

public class TodosControllerTest
{
    private readonly ITodoService _fakeTodoService;
    private readonly TodosController _controller;

    public TodosControllerTest()
    {
        _fakeTodoService = A.Fake<ITodoService>();
        _controller = new TodosController(_fakeTodoService);
        
        // Инициализируем HttpContext, так как Get и Patch используют HttpContext.Items
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }
    
    [Fact]
    public async Task GetAll_ReturnsOkResult_WithListOfTodos()
    {
        var fakeTodos = new List<Todo>
        {
            new(1, "Task 1", DateTime.UtcNow.AddDays(1), false),
            new(2, "Task 2", DateTime.UtcNow.AddDays(2), false)
        };
        A.CallTo(() => _fakeTodoService.ReadAll()).Returns(Task.FromResult(fakeTodos));

        var actionResult = await _controller.GetAll();

        var result = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnTodos = Assert.IsType<List<Todo>>(result.Value);
        Assert.Equal(2, returnTodos.Count);
    }
    [Fact]
    public void Get_ReturnsOkResult_WithTodoFromHttpContext()
    {
        var expectedTodo = new Todo(1, "Context Todo", DateTime.UtcNow.AddDays(1), false);
        _controller.HttpContext.Items["Todo"] = expectedTodo;

        var actionResult = _controller.Get(1);

        var result = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnTodo = Assert.IsType<Todo>(result.Value);
        Assert.Equal(expectedTodo.Id, returnTodo.Id);
    }
    [Fact]
    public async Task Post_ReturnsCreatedAtActionResult_WithCreatedTodoAsync()
    {
        var todoToCreate = new Todo(1, "New Todo", DateTime.UtcNow.AddDays(1), false);
        A.CallTo(() => _fakeTodoService.Create(todoToCreate)).Returns(Task.FromResult(todoToCreate));

        var actionResult = await _controller.Post(todoToCreate);

        var result = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
        Assert.Equal(nameof(_controller.Get), result.ActionName);
        Assert.Equal(1, ((Todo)result.Value!).Id);
    }

    [Fact]
    public async Task Put_ReturnsNoContentResult()
    {
        var todoToUpdate = new Todo(1, "Updated Todo", DateTime.UtcNow.AddDays(1), false);
        A.CallTo(() => _fakeTodoService.Update(1, todoToUpdate)).Returns(Task.FromResult<Todo?>(todoToUpdate));

        var result = await _controller.Put(1, todoToUpdate);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Patch_UpdatesOnlyProvidedFields_AndReturnsNoContent()
    {
        var existingTodo = new Todo(1, "Old Name", DateTime.UtcNow.AddDays(1), false);
        _controller.HttpContext.Items["Todo"] = existingTodo;

        var patchDto = new TodoPatchDto("New Name", null, true);

        var result = await _controller.Patch(1, patchDto);

        Assert.IsType<NoContentResult>(result);
        A.CallTo(() => _fakeTodoService.Update(1, A<Todo>.That.Matches(t =>
            t.Name == "New Name" &&
            t.DueDate == existingTodo.DueDate &&
            t.IsCompleted == true
        ))).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Delete_ReturnsNoContentResult()
    {
        var result = _controller.Delete(1);

        Assert.IsType<NoContentResult>(result);
        A.CallTo(() => _fakeTodoService.Delete(1)).MustHaveHappenedOnceExactly();
    }
}
