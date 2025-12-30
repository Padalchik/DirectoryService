using DirectoryService.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging(builder.Configuration);
builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();

app.UseDatabaseMigration();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(c =>
        c.SwaggerEndpoint("/openapi/v1.json", "DirectoryService_v1"));
}

app.UseApplication();

app.MapControllers();
app.Run();

namespace DirectoryService.API
{
    public partial class Program;
}