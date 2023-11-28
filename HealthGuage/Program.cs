using HealthGuage.Filters;
using HealthGuage.HelpingClasses;
using HealthGuage.Models;
using HealthGuage.Repositories;
using LoginFinal.Filters;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Template.Models;
using Template.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
builder.Services.Configure<ProjectVariables>(builder.Configuration.GetSection("ProjectVariables"));

builder.Services.AddScoped<IUserRepo, UserRepo>();
builder.Services.AddScoped<IContentFileRepo, ContentFileRepo>();
builder.Services.AddScoped<IIngredientRepo, IngredientRepo>();
builder.Services.AddScoped<IProductRepo, ProductRepo>();
builder.Services.AddScoped<IPreperationRepo, PreperationRepo>();
builder.Services.AddScoped<IMenuRepo, MenuRepo>();

builder.Services.AddScoped<ExceptionFilter>();
builder.Services.AddScoped<ValidationFilter>();

builder.Services.AddScoped<GeneralPurpose>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(x =>
    {
        x.LoginPath = "/Auth/Login";
        x.ExpireTimeSpan = TimeSpan.FromHours(12);  // Set the expiry time
    });

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

app.UseStaticFiles();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
