using System.Runtime.InteropServices.JavaScript;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Locations;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Shared;
using DirectoryService.Infrastructure;
using DirectoryService.Infrastructure.Repositories;
using DirectoryService.Presentation.Middlewares;
using FluentValidation;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.Debug()
    .WriteTo.Seq(builder.Configuration.GetConnectionString("Seq") ?? throw new Exception("Not found connection string Seq"))
    .CreateLogger();

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddScoped<ApplicationDBContext>();
builder.Services.AddScoped<ICommandHandler<Location, CreateLocationCommand>, CreateLocationHandler>();
builder.Services.AddScoped<ILocationsRepository, LocationRepository>();
builder.Services.AddScoped<IValidator<CreateLocationCommand>, CreateLocationValidator>();
builder.Services.AddSerilog();

var app = builder.Build();

app.UseCustomExeptionMiddleware();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "DirectoryService_v1"));
}

app.UseSerilogRequestLogging();

app.MapControllers();
app.Run();