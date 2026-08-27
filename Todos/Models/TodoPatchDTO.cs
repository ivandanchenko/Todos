namespace Todos.Models
{
    public record TodoPatchDto(string? Name, DateTime? DueDate, bool? IsCompleted);
}
