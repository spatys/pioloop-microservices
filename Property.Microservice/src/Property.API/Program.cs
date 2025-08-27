using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Property.Application.Handlers;
using Property.Domain.Interfaces;
using Property.Infrastructure.Data;
using Property.Infrastructure.Repositories;
using Property.Infrastructure.Extensions;
using System.Reflection;
using Microsoft.OpenApi.Models;
using System.Text;

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

// HttpContext Accessor pour récupérer l'utilisateur connecté
builder.Services.AddHttpContextAccessor();

// HttpClient pour appeler les autres microservices
builder.Services.AddHttpClient();

// Authentication JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured"))
            )
        };
    });

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
                  .AllowAnyHeader();
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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Initialize database with migrations
await app.InitializeDatabaseAsync();

app.Run();
