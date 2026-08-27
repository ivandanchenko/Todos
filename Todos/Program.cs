using Microsoft.AspNetCore.Rewrite;
using Todos.Interfaces;
using Todos.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSingleton<ITodoService, InMemoryTodoService>();

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

