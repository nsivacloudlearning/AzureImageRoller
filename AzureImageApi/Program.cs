using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using AzureImageApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

// Load appsettings.json first
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

var keyVaultUrl = builder.Configuration["KeyVaultUrl"];

// Connect to Key Vault
if (keyVaultUrl != null)
{

    var keyVaultUri = new Uri(keyVaultUrl);

    var secretClient = new SecretClient(keyVaultUri, new DefaultAzureCredential());
    builder.Configuration.AddAzureKeyVault(secretClient, new KeyVaultSecretManager());
}


// Now you can safely read values: if they exist in Key Vault, they override appsettings.json
var jwtIssuer = builder.Configuration["JwtIssuer"];
var jwtAudience = builder.Configuration["JwtAudience"];
var jwtKey = builder.Configuration["JwtKey"];
var azureStorageConnection = builder.Configuration["AzureStorageConnectionString"];
var appInsightsConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
{
    if (string.IsNullOrEmpty(jwtKey))
    {
        throw new InvalidOperationException("JWT key is not configured. Please set 'JwtKey' in Key Vault or appsettings.json.");
    }

    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(jwtKey))
    };
});



builder.Services.AddAuthorization();

// Register BlobService
builder.Services.AddSingleton<BlobService>();

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Logging and monitoring with Application Insights
builder.Services.AddApplicationInsightsTelemetry();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


///keyvaulturl in appsettings if local testing else azure appsettings.
// If running locally (Development), take Key Vault URL from appsettings.json
//if (builder.Environment.IsDevelopment())
//{
//    keyVaultUrl = builder.Configuration["KeyVault:Url"];
//}
//else
//{
//    // In Production/Staging, pull Key Vault URL from Azure App Configuration
//    builder.Configuration.AddAzureAppConfiguration(options =>
//    {
//        options.Connect(new DefaultAzureCredential())
//               .ConfigureKeyVault(kv =>
//               {
//                   kv.SetCredential(new DefaultAzureCredential());
//               });
//    });

//    keyVaultUrl = builder.Configuration["KeyVault:Url"];
//}