using Hangfire;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddHangfire(configuration =>
    {
        configuration.SetDataCompatibilityLevel(CompatibilityLevel.Version_180);
        configuration.UseSimpleAssemblyNameTypeSerializer();
        configuration.UseRecommendedSerializerSettings();
        configuration.UseSqlServerStorage("Server=localhost;Database=HangfireTest;User ID=sa;Password=Password12!;TrustServerCertificate=true;Enlist=false");
    });

builder.Services.AddHangfireServer();
builder.Services.AddLogging();

var app = builder.Build();
app.Run();