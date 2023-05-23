using Entities;
using Microsoft.EntityFrameworkCore;
using ServiceContracts;
using Services;
using RepositoryContracts;
using Repositories;
using Serilog;
using CRUDExample.Filters.ActionFilters;
using CRUDExample;

var builder = WebApplication.CreateBuilder(args);

//Logging Serilog
builder.Host.UseSerilog((HostBuilderContext context, IServiceProvider services, LoggerConfiguration loggerConfiguration) =>
{
    loggerConfiguration.ReadFrom.Configuration(context.Configuration)
                       .ReadFrom.Services(services);  
}); //serilog'un konfigurasyon ayarlarini ve servisleri okumayabilmesi icin

builder.Services.ConfigureServices(builder.Configuration); //my extension

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
