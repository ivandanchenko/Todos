using Microsoft.AspNetCore.Rewrite;
//using Microsoft.OpenApi;
using Todos.Interfaces;
using Todos.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSingleton<ITodoService, InMemoryTodoService>();

//builder.Services.AddOpenApi(options => options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_0);

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.UseRewriter(new RewriteOptions().AddRedirect("tasks/(.*)", "api/todos/$1"));
app.Use(async (context, next) => {
    Console.WriteLine($"[{context.Request.Method} {context.Request.Path} {DateTime.UtcNow}] Started.");
    await next(context);
    Console.WriteLine($"[{context.Request.Method} {context.Request.Path} {DateTime.UtcNow}] Finished.");
});

app.Run();

