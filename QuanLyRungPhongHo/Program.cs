using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using QuanLyRungPhongHo.Data;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using QuanLyRungPhongHo.Services; // Add this line

var builder = WebApplication.CreateBuilder(args);

// ===================== SERVICES =====================

// MVC + Localization
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

// DbContext
var connectionString = builder.Configuration.GetConnectionString("QLRungPhongHoConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string 'QLRungPhongHoConnection' not found in appsettings.json");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".QLRungPhongHo.Session";
});

// Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.Name = ".QLRungPhongHo.Auth";
    });

builder.Services.AddAuthorization();

// Add OTP and Email services
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IOtpService, OtpService>();

// ===================== BUILD APP =====================

var app = builder.Build();

// ===================== MIDDLEWARE =====================

// Localization (PHẢI sau Build, trước Routing)
var supportedCultures = new[]
{
    new CultureInfo("vi-VN"),
    new CultureInfo("en-US")
};



var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("vi-VN"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
};

// 🔥 QUAN TRỌNG NHẤT: đọc culture từ COOKIE
localizationOptions.RequestCultureProviders = new List<IRequestCultureProvider>
{
    new CookieRequestCultureProvider()
};

app.UseRequestLocalization(localizationOptions);


// Error handling
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Thứ tự quan trọng
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// Routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
