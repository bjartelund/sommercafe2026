using Azure.Monitor.OpenTelemetry.Exporter;
using CafeApp.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;
using OpenTelemetry;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

var sqlConnectionString = builder.Configuration["ConnectionStrings:SqlConnectionString"]
    ?? builder.Configuration["SqlConnectionString"];

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(sqlConnectionString));

builder.Services.AddMicrosoftIdentityWebApiAuthentication(builder.Configuration);
builder.Services.AddAuthorization();

builder.Services.AddOpenTelemetry()
    .UseFunctionsWorkerDefaults();
    //.UseAzureMonitorExporter();

builder.Build().Run();