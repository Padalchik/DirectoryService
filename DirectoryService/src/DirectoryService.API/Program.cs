using DirectoryService.API.Extensions;
using DirectoryService.Application;
using DirectoryService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .ManageApplicationDbContext(builder.Configuration)
    .AddSerilogLogging(builder.Configuration)
    .AddApi()
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

var app = builder.Build();
app.MigrateDatabase();
app.UseApi();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "DirectoryService_v1"));
}

app.MapControllers();
app.Run();

namespace DirectoryService.API
{
    public partial class Program;
}