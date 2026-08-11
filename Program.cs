using CAFRI.Application.Abstractions.Services;
using CAFRI.Domain.Users;
using CAFRI.Infrastructure.Authorization;
using CAFRI.Infrastructure.Content;
using CAFRI.Infrastructure.Identity;
using CAFRI.Infrastructure.Persistence;
using CAFRI.Infrastructure.Services.Access;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration
    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
    .AddUserSecrets<Program>(optional: true);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ProfessionalAccess", policy =>
        policy.RequireAuthenticatedUser()
            .AddRequirements(new ProfessionalAccessRequirement()));
});
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.Configure<AdminSeedOptions>(
    builder.Configuration.GetSection(AdminSeedOptions.SectionName));
builder.Services.Configure<PublicationImportApiOptions>(
    builder.Configuration.GetSection(PublicationImportApiOptions.SectionName));
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.User.RequireUniqueEmail = true;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.Lockout.MaxFailedAccessAttempts = 5;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});
builder.Services.AddScoped<IProfessionalAccessService, ProfessionalAccessService>();
builder.Services.AddScoped<IdentitySeedService>();
builder.Services.AddScoped<ContentSeedService>();
builder.Services.AddScoped<IAuthorizationHandler, ProfessionalAccessHandler>();
builder.Services.AddSingleton<JsonContentFileLoader>();
builder.Services.AddScoped<ContentMediaStorageService>();
builder.Services.AddScoped<IHomeContentService, DbHomeContentService>();
builder.Services.AddScoped<IIntelligenceContentService, DbIntelligenceContentService>();
builder.Services.AddScoped<IPublicationContentService, DbPublicationContentService>();
builder.Services.AddScoped<ICountryContentService, DbCountryContentService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();

    var seeder = scope.ServiceProvider.GetRequiredService<IdentitySeedService>();
    await seeder.SeedAsync();

    var contentSeeder = scope.ServiceProvider.GetRequiredService<ContentSeedService>();
    await contentSeeder.SeedAsync();
}

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
