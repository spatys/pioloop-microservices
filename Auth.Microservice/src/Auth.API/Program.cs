using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using MediatR;
using Auth.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Auth.Domain.Identity;
using Auth.Domain.Services;
using Auth.Infrastructure.Services;
using Auth.Infrastructure.Extensions;
using Auth.Application.Handlers;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Auth Microservice API", 
        Version = "1.0.0",
        Description = "API pour l'authentification et la gestion des utilisateurs dans l'écosystème Pioloop",
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
    
    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });

    // Inclure la documentation XML si elle existe
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // Ajouter des exemples de réponses (optionnel)
    // c.ExampleFilters();
});

// Database
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
})
    .AddEntityFrameworkStores<AuthDbContext>()
    .AddDefaultTokenProviders();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"] ?? "default-secret-key-32-chars-long");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };

    // Read JWT from HttpOnly cookie 'auth_token'
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (context.Request.Cookies.TryGetValue("auth_token", out var token))
            {
                context.Token = token;
            }
            return Task.CompletedTask;
        }
    };
});

// MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(LoginCommandHandler).Assembly));

// Repositories - supprimés (on passera par Identity)

            // Enable CORS for API Gateway only
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowApiGateway", policy =>
                {
                    policy.WithOrigins(
                            "https://api.pioloop.com",  // Production API Gateway
                            "http://localhost:5000" // Local API Gateway (HTTP)
                          )
                          .WithMethods("GET", "POST", "PUT", "DELETE", "PATCH")
                          .AllowAnyHeader()
                          .AllowCredentials();
                });
            });

// Services
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IDatabaseSeedService, DatabaseSeedService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
// Serve Swagger UI under /swagger consistently
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Auth Microservice API v1.0.0");
    c.RoutePrefix = "swagger";
    c.DocumentTitle = "Auth Microservice API Documentation";
    c.DefaultModelsExpandDepth(-1);
});

// Enable CORS
app.UseCors("AllowApiGateway");

app.UseHttpsRedirection();

// Remove the old AllowAll CORS policy
// app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Initialize database (migrations + seed)
await app.InitializeDatabaseAsync();

app.Run();
