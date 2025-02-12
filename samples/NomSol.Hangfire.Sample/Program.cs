using Asp.Versioning;
using NomSol.Hangfire.Dev;
using NomSol.Hangfire.Dev.Extensions;
using NomSol.Hangfire.JobManager.Core;

List<CustomApiVersionModel> apiVersions =
[
    new CustomApiVersionModel()
    {
        Version = new ApiVersion(1),
        VersionPath = "v1"
    }
];

var builder = WebApplication.CreateBuilder(args);

// Access to configuration and environment
var configuration = builder.Configuration;
var environment = builder.Environment;

builder.Services.AddMinimalApiVersioning(environment, configuration);

string baseUrl = configuration["Auth0:BaseUrl"];

//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//}).AddJwtBearer(options =>
//{
//    options.Authority = baseUrl;
//    options.MetadataAddress = $"{baseUrl}/.well-known/openid-configuration";
//    options.Audience = configuration["Auth0:Audience"];
//    // options.RequireHttpsMetadata = false;
//});


//builder.Services.AddAuthorization();
builder.Services.AddApplicationServices(configuration);
builder.Services.AddBusinessServices();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
//app.UseAuthentication();
//app.UseAuthorization();

//////////////
///Add Versions
///
Asp.Versioning.Builder.ApiVersionSet versionSet = app.NewApiVersionSet().HasApiVersion(apiVersions[0].Version)
.Build();

app.AddCustomSwagger(configuration, apiVersions.Select(a => a.VersionPath).ToList());


//////////////////
///Configure Endpoints
///
app.EnableNomSolJobManagerEndpoints(RequireAuthorization: false);

// Map health check endpoint
app.MapHealthChecks("/health");

app.Run();
