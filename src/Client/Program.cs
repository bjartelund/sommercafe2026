using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Client;
using Client.Services;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Register MudBlazor services
builder.Services.AddMudServices();

// Register application services
builder.Services.AddScoped<EmployeeService>();
builder.Services.AddScoped<ProductsService>();
builder.Services.AddScoped<OrdersService>();
builder.Services.AddScoped<LedgerService>();
builder.Services.AddScoped<ExpensesService>();
builder.Services.AddScoped<WorkSessionsService>();

await builder.Build().RunAsync();