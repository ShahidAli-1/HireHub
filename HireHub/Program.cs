using Hangfire;
using HireHub.Data;
using HireHub.Helpers;
using HireHub.Jobs;
using HireHub.Services.Applications;
using HireHub.Services.ATS;
using HireHub.Services.Auth;
using HireHub.Services.Email;
using HireHub.Services.Export;
using HireHub.Services.Files;
using HireHub.Services.Jobs;
using HireHub.Services.Notifications;
using HireHub.Services.Subscriptions;
using HireHub.Services.Usage;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ===== MVC =====
builder.Services.AddControllersWithViews();

// ===== Database =====
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ===== Hangfire =====
builder.Services.AddHangfire(config =>
    config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
          .UseSimpleAssemblyNameTypeSerializer()
          .UseRecommendedSerializerSettings()
          .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"), new Hangfire.SqlServer.SqlServerStorageOptions
          {
              CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
              SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
              QueuePollInterval = TimeSpan.Zero,
              UseRecommendedIsolationLevel = true,
              DisableGlobalLocks = true
          }));

builder.Services.AddHangfireServer();

// ===== Session Configuration (Required for Guest ATS checks) =====
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ===== Application Services =====
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddScoped<IApplicationService, ApplicationService>();
builder.Services.AddScoped<IUsageLimitService, UsageLimitService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IExportService, ExportService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IAtsService, AtsService>();

// ===== Background Jobs Registration =====
builder.Services.AddScoped<DailyLimitResetJob>();
builder.Services.AddScoped<SubscriptionExpiryJob>();
builder.Services.AddScoped<JobExpiryJob>();

// ===== Cookie Authentication =====
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.SlidingExpiration = true;
    });

var app = builder.Build();

// ===== Seed Data =====
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await SeedData.InitializeAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// ===== HTTP Request Pipeline =====
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Enable Session (Must be before UseAuthentication)
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// ===== Hangfire Dashboard =====
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAdminAuthorization() }
});

// ===== Schedule Background Jobs =====
RecurringJob.AddOrUpdate<DailyLimitResetJob>(
    "daily-limit-reset",
    job => job.ExecuteAsync(),
    Cron.Daily);

RecurringJob.AddOrUpdate<SubscriptionExpiryJob>(
    "subscription-expiry",
    job => job.ExecuteAsync(),
    Cron.Daily(0, 30));

RecurringJob.AddOrUpdate<JobExpiryJob>(
    "job-expiry",
    job => job.ExecuteAsync(),
    Cron.Daily(1, 0));

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();