using Entities;
using Microsoft.EntityFrameworkCore;
using ServiceContracts;
using Services;
using RepositoryContracts;
using Repositories;
using Serilog;
using CRUDExample.Filters.ActionFilters;

var builder = WebApplication.CreateBuilder(args);

//Logging Serilog
builder.Host.UseSerilog((HostBuilderContext context, IServiceProvider services, LoggerConfiguration loggerConfiguration) =>
{
    loggerConfiguration.ReadFrom.Configuration(context.Configuration)
                       .ReadFrom.Services(services);  
}); //serilog'un konfigurasyon ayarlarini ve servisleri okumayabilmesi icin

builder.Services.AddControllersWithViews(options =>
{
    ILogger<ResponseHeaderActionFilter> logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<ResponseHeaderActionFilter>>(); //GetService->bulamazsa null atýyor.
    //options.Filters.Add<ResponseHeaderActionFilter>();
    options.Filters.Add(new ResponseHeaderActionFilter(logger,"My-Key-From-Global","My-Value-From-Global"));
});

//IoC services into IoC container
builder.Services.AddScoped<ICountriesRepository, CountriesRepository>();
builder.Services.AddScoped<IPersonsRepository, PersonsRepository>();
builder.Services.AddScoped<ICountriesService, CountriesService>();
builder.Services.AddScoped<IPersonsService, PersonsService>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestProperties | Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.ResponsePropertiesAndHeaders;
});

var app = builder.Build();

app.UseSerilogRequestLogging();

if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}


if (builder.Environment.IsEnvironment("Test") != true)
{ 
Rotativa.AspNetCore.RotativaConfiguration.Setup("wwwroot", wkhtmltopdfRelativePath: "Rotativa");
}

app.UseStaticFiles();
app.UseRouting();
app.MapControllers();

app.UseHttpLogging();

app.Run();

public partial class Program { }
