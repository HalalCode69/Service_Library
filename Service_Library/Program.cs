using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Service_Library.Entities;
using Service_Library.Services;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();
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
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddTransient<EmailService>();
builder.Services.AddTransient<ReminderService>();
builder.Services.Configure<PayPalSettings>(builder.Configuration.GetSection("PayPalSettings"));
builder.Services.AddTransient<PayPalService>();
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Services.AddScoped<AutoReturnService>();
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddControllersWithViews();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
var app = builder.Build();
app.UseHangfireDashboard();
app.MapHangfireDashboard();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

RecurringJob.AddOrUpdate<AutoReturnService>(
    service => service.CleanUpExpiredReservations(),
    Cron.Minutely);

RecurringJob.AddOrUpdate<AutoReturnService>(
    service => service.AutoReturnOverdueBooks(),
    Cron.Minutely);

RecurringJob.AddOrUpdate<ReminderService>(
    reminderService => reminderService.SendReminderEmails(),
    Cron.Minutely);

app.Run();