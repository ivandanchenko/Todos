using System;
using Microsoft.EntityFrameworkCore;
using Todos.Data;
using Todos.Interfaces;
using Todos.Models;

namespace Todos.Services
{
    public class PostgresSqlTodoService : ITodoService
    {
        TodoContext _context;

        public PostgresSqlTodoService(TodoContext context)
        {
            _context=context;
        }
        public async Task<Todo> Create(Todo todo)
        {
            var utcTodo = todo with { DueDate = DateTime.SpecifyKind(todo.DueDate, DateTimeKind.Utc) };
            _context.Todo.Add(utcTodo);
            await _context.SaveChangesAsync();
            return todo;
        }

        public async Task Delete(int id)
        {
            var todo = await _context.Todo.FindAsync(id);
            if (todo != null)
            {
                _context.Todo.Remove(todo);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Todo>> ReadAll()
        {
            return await _context.Todo.ToListAsync<Todo>();
        }

        public async Task<Todo?> ReadById(int id)
        {
            return await _context.Todo.SingleOrDefaultAsync(t => t.Id==id);
        }

        public async Task<Todo?> Update(int id, Todo todo)
        {
            var existingTodo = await ReadById(id);

            if (existingTodo is null)
                return null;
            var todoToUpdate = todo with { Id = id };

            _context.Todo.Update(todoToUpdate);
            await _context.SaveChangesAsync();

            return todoToUpdate;
        }
    }
}
