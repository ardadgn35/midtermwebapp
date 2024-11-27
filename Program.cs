using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Azure Key Vault URL'si (Azure Key Vault isminiz "MyKeyVaultArda" olduğunu varsayıyorum)
string keyVaultUrl = "https://MyKeyVaultArda.vault.azure.net/";

// Key Vault'dan bağlantı bilgisini almak için SecretClient kullanıyoruz
var client = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
KeyVaultSecret dbHostSecret = await client.GetSecretAsync("DbHost");
KeyVaultSecret dbUserSecret = await client.GetSecretAsync("DbUser");
KeyVaultSecret dbPasswordSecret = await client.GetSecretAsync("DbPassword");
KeyVaultSecret dbNameSecret = await client.GetSecretAsync("DbName");

// Key Vault'tan alınan değerleri kullanarak bağlantı dizesini oluşturuyoruz
string dbHost = dbHostSecret.Value;
string dbUser = dbUserSecret.Value;
string dbPassword = dbPasswordSecret.Value;
string dbName = dbNameSecret.Value;

// Bağlantı dizesini oluştur
var connectionString = $"Host={dbHost};Database={dbName};Username={dbUser};Password={dbPassword}";

// Uygulamanın oluşturulması
var app = builder.Build();

app.MapGet("/hello", async () =>
{
    try
    {
        // PostgreSQL'e bağlan
        using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        // Örnek bir sorgu çalıştır
        using var command = new NpgsqlCommand("SELECT NOW()", connection);
        var result = await command.ExecuteScalarAsync();

        return $"Connected to PostgreSQL! Current Time: {result}";
    }
    catch (Exception ex)
    {
        return $"An error occurred: {ex.Message}";
    }
});

// Uygulamayı çalıştır
app.Run();
