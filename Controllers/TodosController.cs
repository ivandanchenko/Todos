using Microsoft.AspNetCore.Mvc;
using Todos.Filters;
using Todos.Interfaces;
using Todos.Models;

namespace Todos.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodosController :ControllerBase
    {
        private readonly ITodoService _todoService;

        public TodosController(ITodoService taskService)
        {
            _todoService=taskService;
        }
        [HttpGet]
        public async Task<ActionResult<List<Todo>>> GetAll()
        {
            var todos= await _todoService.ReadAll();
            return Ok(todos);
        }

        [HttpGet("{id:int:min(1)}")]
        [TypeFilter(typeof(ValidateTodoExistsAttribute))]
        public  ActionResult<Todo> Get(int id)
        {
            var todo = HttpContext.Items["Todo"] as Todo;
            return Ok(todo);
        }

        [HttpPost]
        [TypeFilter(typeof(CreateTodoValidationFilter))]
        public ActionResult<Todo> Post([FromBody] Todo todo)
        {
            var createdTodo = _todoService.Create(todo);
            return CreatedAtAction(nameof(Get), new { id = createdTodo.Id }, createdTodo);
        }

        [HttpPut("{id:int:min(1)}")]
        [TypeFilter(typeof(ValidateTodoExistsAttribute))]
        public async Task<IActionResult> Put(int id, [FromBody] Todo todo)
        {
            await _todoService.Update(id, todo);
            return NoContent();
        }

        [HttpPatch("{id:int:min(1)}")]
        [TypeFilter(typeof(ValidateTodoExistsAttribute))]
        public async Task<IActionResult> Patch(int id, [FromBody] TodoPatchDto patchDto)
        {
            var todo = HttpContext.Items["Todo"] as Todo;
            var updatedTodo = todo! with
            {
                Name = patchDto.Name ?? todo.Name,
                DueDate = patchDto.DueDate ?? todo.DueDate,
                IsCompleted = patchDto.IsCompleted ?? todo.IsCompleted
            };
            await _todoService.Update(id, updatedTodo);
            return NoContent();
        }

        [HttpDelete("{id:int:min(1)}")]
        [TypeFilter(typeof(ValidateTodoExistsAttribute))]
        public IActionResult Delete(int id)
        {
            _todoService.Delete(id);
            return NoContent();
        }
    }
}
