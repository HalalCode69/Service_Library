using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Service_Library.Entities;
using Service_Library.Services;
using PayPalCheckoutSdk.Core;
using PayPalCheckoutSdk.Orders;
using Microsoft.Extensions.Options;
using Service_Library.Entities;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
// Add Hangfire services
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"), new SqlServerStorageOptions
    {
        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        QueuePollInterval = TimeSpan.Zero,
        UseRecommendedIsolationLevel = true,
        DisableGlobalLocks = true
    })
);

builder.Services.AddHangfireServer();
// Add email service
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddTransient<EmailService>();
builder.Services.AddTransient<ReminderService>();

// Add PayPal settings
builder.Services.Configure<PayPalSettings>(builder.Configuration.GetSection("PayPalSettings"));
builder.Services.AddTransient<PayPalService>();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();

var app = builder.Build();

// Configure Hangfire Dashboard (optional for monitoring)
app.UseHangfireDashboard();
app.MapHangfireDashboard();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles(); // Ensures static files like CSS are served
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();
RecurringJob.AddOrUpdate<AutoReturnService>(
    service => service.AutoReturnOverdueBooks(),
    Cron.Minutely // Check every minute
);
RecurringJob.AddOrUpdate<ReminderService>(
    reminderService => reminderService.SendReminderEmails(),
    Cron.Minutely); // Runs the job every minute

app.Run();
