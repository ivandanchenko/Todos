using Microsoft.AspNetCore.Http.HttpResults;
using Todos.Interfaces;
using Todos.Models;
namespace Todos.Services
{
    public class InMemoryTodoService : ITodoService
    {
        private readonly List<Todo> _todos = [];
       
        public Task<Todo?> ReadById(int id)
        {
            return Task.FromResult(_todos.SingleOrDefault(t => id == t.Id));
        }

        public Task<List<Todo>> ReadAll()
        {
            return Task.FromResult(_todos);
        } 
        public Task<Todo> Create(Todo task)
        {
            _todos.Add(task);
            return Task.FromResult(task);
        }
        public async Task<Todo?> Update(int id, Todo task)
        {
            var existingTodo = await ReadById(id);

            if (existingTodo is null)
                return null;
            var todoToUpdate = task with { Id = id };

            _todos.Remove(existingTodo);
            _todos.Add(todoToUpdate);

            return todoToUpdate;
        }
        public Task Delete(int id)
        {
            _todos.RemoveAll(task => id == task.Id);
            return Task.CompletedTask;
        }
    }}
