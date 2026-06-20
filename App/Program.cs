using BlazorApp1.Components;
using BlazorApp1.Services;
using CafeApp.Data;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();
builder.Services.AddHttpClient();
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient());

var connectionString = builder.Configuration.GetConnectionString("SqlConnectionString")
                       ?? builder.Configuration["SqlConnectionString"]
                       ?? throw new InvalidOperationException("Connection string 'SqlConnectionString' not found.");

builder.Services.AddDbContextFactory<AppDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddScoped<EmployeeService>();
builder.Services.AddScoped<ProductsService>();
builder.Services.AddScoped<OrdersService>();
builder.Services.AddScoped<LedgerService>();
builder.Services.AddScoped<ExpensesService>();
builder.Services.AddScoped<WorkSessionsService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await using var scope = app.Services.CreateAsyncScope();
    var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
    await using var db = await dbFactory.CreateDbContextAsync();
    await db.Database.EnsureCreatedAsync();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();


app.Run();