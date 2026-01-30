using Microsoft.EntityFrameworkCore;
using TrustIdentity.Licensing;
using TrustIdentity.Storage.EntityFramework;
using TrustIdentity.Storage.Stores;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddDbContext<LicensingDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("LicensingDb") ?? "Data Source=licensing.db"));

builder.Services.AddScoped<ILicenseStore, LicenseStore>();
builder.Services.AddSingleton<LicenseService>();
builder.Services.AddSingleton<ILicenseGenerator>(sp => sp.GetRequiredService<LicenseService>());
builder.Services.AddSingleton<ILicenseValidator>(sp => sp.GetRequiredService<LicenseService>());

var app = builder.Build();

// Ensure DB is created
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LicensingDbContext>();
    db.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
