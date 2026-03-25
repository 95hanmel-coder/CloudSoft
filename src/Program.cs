using CloudSoft.Repositories;
using CloudSoft.Services;
using CloudSoft.Models;
using CloudSoft.Configurations;
using MongoDB.Driver;
using Azure.Identity;

var builder = WebApplication.CreateBuilder(args);

// --- HÄR BÖRJAR DEN NYA KEY VAULT-KODEN ---
// Check if Azure Key Vault should be used
bool useAzureKeyVault = builder.Configuration.GetValue<bool>("FeatureFlags:UseAzureKeyVault");

if (useAzureKeyVault)
{
    // Configure Azure Key Vault options
    builder.Services.Configure<AzureKeyVaultOptions>(
        builder.Configuration.GetSection(AzureKeyVaultOptions.SectionName));

    // Get Key Vault URI from configuration
    var keyVaultOptions = builder.Configuration
        .GetSection(AzureKeyVaultOptions.SectionName)
        .Get<AzureKeyVaultOptions>();
    var keyVaultUri = keyVaultOptions?.KeyVaultUri;

    // Register Azure Key Vault as configuration provider
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

// Add services to the container.
builder.Services.AddControllersWithViews();

// Check if MongoDB should be used (default to false if not specified)
bool useMongoDb = builder.Configuration.GetValue<bool>("FeatureFlags:UseMongoDb");

if (useMongoDb)
{
    // Configure MongoDB options
    builder.Services.Configure<MongoDbOptions>(
        builder.Configuration.GetSection(MongoDbOptions.SectionName));

    // Configure MongoDB client
    builder.Services.AddSingleton<IMongoClient>(serviceProvider => {
        var mongoDbOptions = builder.Configuration.GetSection(MongoDbOptions.SectionName).Get<MongoDbOptions>();
        return new MongoClient(mongoDbOptions?.ConnectionString);
    });

    // Configure MongoDB collection
    builder.Services.AddSingleton<IMongoCollection<Subscriber>>(serviceProvider => {
        var mongoDbOptions = builder.Configuration.GetSection(MongoDbOptions.SectionName).Get<MongoDbOptions>();
        var mongoClient = serviceProvider.GetRequiredService<IMongoClient>();
        var database = mongoClient.GetDatabase(mongoDbOptions?.DatabaseName);
        return database.GetCollection<Subscriber>(mongoDbOptions?.SubscribersCollectionName);
    });

    // Register MongoDB repository
    builder.Services.AddSingleton<ISubscriberRepository, MongoDbSubscriberRepository>();

    Console.WriteLine("Using MongoDB repository");
}
else
{
    // Register in-memory repository as fallback
    builder.Services.AddSingleton<ISubscriberRepository, InMemorySubscriberRepository>();

    Console.WriteLine("Using in-memory repository");
}

// Register service (depends on repository)
builder.Services.AddScoped<INewsletterService, NewsletterService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();