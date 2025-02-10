using System.Transactions;
using apiservice;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using shared;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<BloggingContext>(contextOptions =>
{
    contextOptions.UseSqlServer("Server=localhost;Database=HangfireTest;User ID=sa;Password=Password12!;TrustServerCertificate=true");
});

builder.Services
    .AddHangfire(configuration =>
    { 
        configuration.SetDataCompatibilityLevel(CompatibilityLevel.Version_180);
        configuration.UseSimpleAssemblyNameTypeSerializer();
        configuration.UseRecommendedSerializerSettings();
        configuration.UseSqlServerStorage("Server=localhost;Database=HangfireTest;User ID=sa;Password=Password12!;TrustServerCertificate=true;Enlist=false");
    });

/*
 * Register BackgroundJobClient manually, to pass the existing DbConnection.
 * This makes sure transaction is not upgraded to a distributed transaction.
 * When DbContext and Hangfire enlist in the transaction scope.
 */

builder.Services.RemoveAll<IBackgroundJobClient>();
builder.Services
    .AddScoped<IBackgroundJobClient>(serviceProvider =>
    {
        var dbContext = serviceProvider.GetRequiredService<BloggingContext>();
        var sqlServerStorageOptions = new SqlServerStorage(dbContext.Database.GetDbConnection());
        var backgroundJobClient = new BackgroundJobClient(sqlServerStorageOptions);
        return backgroundJobClient;
    });

var app = builder.Build();

app
    .MapGet("/enqueue-fail", async (BloggingContext bloggingContext, IBackgroundJobClient backgroundJobClient) =>
    {
        using var transaction = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled);

        var blog = new Blog();
        bloggingContext.Blogs.Add(blog);

        // Is not persisted to database, due to participation in TransactionScope.
        await bloggingContext.SaveChangesAsync();

        // Is not persisted to database, due to participation in TransactionScope.
        var jobId = backgroundJobClient.Enqueue(() => new DummyJob().Execute());
        Console.WriteLine($"Persisted job with id {jobId}");

        // Fail transaction, which causes rollback when disposing TransactionScope.
        throw new InvalidOperationException();
    })
    .WithName("EnqueueThrow");

app
    .MapGet("/enqueue-success", async (BloggingContext bloggingContext, IBackgroundJobClient backgroundJobClient) =>
    {
        using var transaction = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled);

        var blog = new Blog();
        bloggingContext.Blogs.Add(blog);

        // Is not persisted to database, due to participation in TransactionScope.
        await bloggingContext.SaveChangesAsync();

        // Is not persisted to database, due to participation in TransactionScope
        var jobId = backgroundJobClient.Enqueue(() => new DummyJob().Execute());
        Console.WriteLine($"Persisted job with id {jobId}");

        // Blog and Job are persisted, as transaction is committed.
        transaction.Complete();
    })
    .WithName("");

app.Run();