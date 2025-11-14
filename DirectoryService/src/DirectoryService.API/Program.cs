using DirectoryService.API.Middlewares;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Departments;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Locations.CreateLocation;
using DirectoryService.Application.Positions;
using DirectoryService.Infrastructure;
using DirectoryService.Infrastructure.Database;
using DirectoryService.Infrastructure.Repositories;
using FluentValidation;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.Debug()
    .WriteTo.Seq(builder.Configuration.GetConnectionString("Seq") ?? throw new Exception("Not found connection string Seq"))
    .CreateLogger();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddScoped<ApplicationDBContext>(_ =>
    new ApplicationDBContext(builder.Configuration.GetConnectionString("DataBase")!));

builder.Services.AddScoped<IReadDbConext>(_ =>
    new ApplicationDBContext(builder.Configuration.GetConnectionString("DataBase")!));

var applicationAssembly = typeof(CreateLocationHandler).Assembly;

builder.Services.Scan(scan => scan.FromAssemblies(applicationAssembly)
    .AddClasses(classes => classes.AssignableToAny(typeof(ICommandHandler<,>)))
    .AsSelfWithInterfaces()
    .WithScopedLifetime());

builder.Services.AddValidatorsFromAssembly(applicationAssembly);

builder.Services.AddScoped<ILocationsRepository, LocationRepository>();
builder.Services.AddScoped<IPositionsRepository, PositionRepository>();
builder.Services.AddScoped<IDepartmentsRepository, DepartmentRepository>();

builder.Services.AddScoped<ITransactionManager, TransactionManager>();

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

namespace DirectoryService.API
{
    public partial class Program;
}