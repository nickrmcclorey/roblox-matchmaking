using Microsoft.Extensions.Logging.AzureAppServices;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddAzureWebAppDiagnostics();

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddHostedService<Matchmaker>();
builder.Services.AddHostedService<OldResultsPatrol>();
builder.Services.AddHostedService<AccessCodeRequestor>();
builder.Services.AddSingleton<AccessCodeStore>();
builder.Services.AddSingleton<QueueStore>();
builder.Services.AddSingleton<UnfilledGamesStore>();

builder.Services.Configure<AzureFileLoggerOptions>(options => {
    options.FileName = "logs-";
    options.FileSizeLimit = 50 * 1024 * 1024; // 50 MB
    options.RetainedFileCountLimit = 5;
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();
app.UseMiddleware<ApiKeyMiddleware>();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();
