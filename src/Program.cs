using CloudSoft.Repositories;
using CloudSoft.Services;
using CloudSoft.Models;
using CloudSoft.Configurations;
using MongoDB.Driver;
using Azure.Identity;

var builder = WebApplication.CreateBuilder(args);

// Denna rad måste ligga EFTER builder har skapats, men FÖRE builder.Build()
builder.WebHost.UseStaticWebAssets();

// --- HÄR BÖRJAR DEN NYA KEY VAULT-KODEN ---
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
// --- HÄR SLUTAR DEN NYA KEY VAULT-KODEN ---

builder.Services.AddControllersWithViews();

bool useMongoDb = builder.Configuration.GetValue<bool>("FeatureFlags:UseMongoDb");

if (useMongoDb)
{
    builder.Services.Configure<MongoDbOptions>(
        builder.Configuration.GetSection(MongoDbOptions.SectionName));

    builder.Services.AddSingleton<IMongoClient>(serviceProvider => {
        var mongoDbOptions = builder.Configuration.GetSection(MongoDbOptions.SectionName).Get<MongoDbOptions>();
        return new MongoClient(mongoDbOptions?.ConnectionString);
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
app.UseStaticFiles(); // Viktigt för CSS
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();