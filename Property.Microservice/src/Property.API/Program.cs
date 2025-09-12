using Microsoft.EntityFrameworkCore;
using Property.Application.Handlers;
using Property.Application.Mapping;
using Property.Application.Services;
using Property.Domain.Interfaces;
using Property.Infrastructure.Data;
using Property.Infrastructure.Repositories;
using Property.Infrastructure.Extensions;
using Property.Infrastructure.Services;
using System.Reflection;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Property Microservice API", 
        Version = "1.0.0",
        Description = "API pour la gestion des propriétés dans l'écosystème Pioloop",
        Contact = new OpenApiContact
        {
            Name = "Pioloop Team",
            Email = "support@pioloop.com",
            Url = new Uri("https://www.pioloop.com")
        },
        License = new OpenApiLicense
        {
            Name = "MIT",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });
    
    // Inclure la documentation XML si elle existe
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Database
builder.Services.AddDbContext<PropertyDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories
builder.Services.AddScoped<IPropertyRepository, PropertyRepository>();

// Services
builder.Services.AddScoped<IPopularityService, PopularityService>();
builder.Services.AddScoped<IAmenityService, AmenityService>();
builder.Services.AddScoped<IDatabaseSeedService, DatabaseSeedService>();

// Images are now stored as base64 in database - no external storage needed

// HttpContext Accessor pour récupérer l'utilisateur connecté
builder.Services.AddHttpContextAccessor();

// HttpClient pour appeler les autres microservices
builder.Services.AddHttpClient();

// Le Property Service fait confiance à l'API Gateway pour l'authentification
// Il utilise les headers X-User-Id, X-User-Email, X-User-Roles injectés par l'API Gateway

// AutoMapper
builder.Services.AddAutoMapper(typeof(PropertyMappingProfile));

// MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(SearchPropertiesQueryHandler).Assembly));

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowApiGateway", policy =>
    {
        policy.WithOrigins(
                     "https://www.api.pioloop.com",      // Production API Gateway
                     "http://localhost:5000"              // Local API Gateway (HTTP)
                   )
                  .WithMethods("GET", "POST", "PUT", "DELETE", "PATCH")
                  .AllowAnyHeader()
                  .AllowCredentials(); // Important pour les cookies
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Property API V1");
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowApiGateway");

app.UseRouting();

// Le Property Service fait confiance à l'API Gateway pour l'authentification

// Initialize database with migrations
await app.InitializeDatabaseAsync();

app.MapControllers();

app.Run();
