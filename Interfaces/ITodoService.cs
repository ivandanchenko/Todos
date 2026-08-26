using Todos.Models;

namespace Todos.Interfaces
{
    public interface ITodoService
    {
        Task <Todo> Create(Todo todo);
        Task <Todo?> ReadById(int id);
        Task<List<Todo>> ReadAll();
        Task <Todo?> Update(int id, Todo todo);
        Task Delete(int id);
    }

}
