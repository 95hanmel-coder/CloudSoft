using CloudSoft.Repositories;
using CloudSoft.Services;
using CloudSoft.Models;
using CloudSoft.Configurations;
using MongoDB.Driver;
using Azure.Identity;
using System.Security.Authentication;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseStaticWebAssets();

// --- AZURE KEY VAULT ---
bool useAzureKeyVault = builder.Configuration.GetValue<bool>("FeatureFlags:UseAzureKeyVault");

if (useAzureKeyVault)
{
    builder.Services.Configure<AzureKeyVaultOptions>(
        builder.Configuration.GetSection(AzureKeyVaultOptions.SectionName));

    var keyVaultOptions = builder.Configuration
        .GetSection(AzureKeyVaultOptions.SectionName)
        .Get<AzureKeyVaultOptions>();
    var keyVaultUri = keyVaultOptions?.KeyVaultUri;

    if (string.IsNullOrEmpty(keyVaultUri))
    {
        throw new InvalidOperationException("Key Vault URI is not configured.");
    }

    builder.Configuration.AddAzureKeyVault(
        new Uri(keyVaultUri),
        new DefaultAzureCredential());

    Console.WriteLine("Using Azure Key Vault for configuration");
}

builder.Services.AddControllersWithViews();

// --- MONGODB / COSMOS DB ---
bool useMongoDb = builder.Configuration.GetValue<bool>("FeatureFlags:UseMongoDb");

if (useMongoDb)
{
    builder.Services.Configure<MongoDbOptions>(
        builder.Configuration.GetSection(MongoDbOptions.SectionName));

    builder.Services.AddSingleton<IMongoClient>(serviceProvider => {
        var mongoDbOptions = builder.Configuration.GetSection(MongoDbOptions.SectionName).Get<MongoDbOptions>();
        
        var settings = MongoClientSettings.FromConnectionString(mongoDbOptions?.ConnectionString);
        settings.SslSettings = new SslSettings { EnabledSslProtocols = SslProtocols.Tls12 };
        
        // This fixes the "Retryable writes are not supported" error
        settings.RetryWrites = false;
        
        return new MongoClient(settings);
    });

    builder.Services.AddSingleton<IMongoCollection<Subscriber>>(serviceProvider => {
        var mongoDbOptions = builder.Configuration.GetSection(MongoDbOptions.SectionName).Get<MongoDbOptions>();
        var mongoClient = serviceProvider.GetRequiredService<IMongoClient>();
        var database = mongoClient.GetDatabase(mongoDbOptions?.DatabaseName);
        return database.GetCollection<Subscriber>(mongoDbOptions?.SubscribersCollectionName);
    });

    builder.Services.AddSingleton<ISubscriberRepository, MongoDbSubscriberRepository>();
    Console.WriteLine("Using MongoDB repository");
}
else
{
    builder.Services.AddSingleton<ISubscriberRepository, InMemorySubscriberRepository>();
    Console.WriteLine("Using in-memory repository");
}

builder.Services.AddScoped<INewsletterService, NewsletterService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();