using HTB_Updates_Shared_Resources;
using HTB_Updates_Website.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((hostContext, services, configuration) =>
{
    configuration.ReadFrom.Configuration(hostContext.Configuration);
});

builder.Services.AddRazorPages();

var connectionString = builder.Configuration.GetValue<string>("ConnectionString");

builder.Services.AddDbContext<DatabaseContext>(options => 
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString),
        x => x.MigrationsHistoryTable("HTBUpdates_EFMigrationsHistory")
));

builder.Services.AddScoped<IAuthenticationManager, AuthenticationManager>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
