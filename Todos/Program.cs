using Microsoft.AspNetCore.Rewrite;
using Todos.Interfaces;
using Todos.Services;
using Todos.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var connectionString = builder.Configuration.GetConnectionString("Todo");

builder.Services.AddDbContext<TodoContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<ITodoService, PostgresSqlTodoService>();


var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.UseRewriter(new RewriteOptions().AddRewrite(@"^tasks(?:/(.*))?$", "api/todos/$1", true));
app.Use(async (context, next) => {
    Console.WriteLine($"[{context.Request.Method} {context.Request.Path} {DateTime.UtcNow}] Started.");
    await next(context);
    Console.WriteLine($"[{context.Request.Method} {context.Request.Path} {DateTime.UtcNow}] Finished.");
});

app.Run();

