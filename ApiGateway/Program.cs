using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Serilog;
using System.Reflection;
using System.Text;
using ApiGateway.Middleware;
using ApiGateway.Models;
using MMLib.SwaggerForOcelot;

var builder = WebApplication.CreateBuilder(args);

// Configuration de Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Configuration des services
builder.Services.AddControllers();

// Configuration Ocelot
builder.Services.AddOcelot(builder.Configuration);

// **Ajout SwaggerForOcelot ici**
builder.Services.AddSwaggerForOcelot(builder.Configuration);

// Configuration de l'authentification JWT
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured"));

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
});

// Configuration de l'autorisation
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAuthenticatedUser", policy =>
        policy.RequireAuthenticatedUser());
});

// Configuration Swagger avec MMLib.SwaggerForOcelot
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Pioloop API Gateway",
        Version = "1.0.0",
        Description = "API Gateway unifié pour l'écosystème Pioloop",
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

    // Configuration JWT pour Swagger
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
            Array.Empty<string>()
        }
    });

    // Inclure la documentation XML
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Configuration CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("Policy", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "https://www.pioloop.com"
              )
              .WithMethods("GET", "POST", "PUT", "DELETE", "PATCH")
              .AllowAnyHeader();
    });
});

// Configuration des services HTTP
builder.Services.AddHttpClient();

// Configuration de la validation des modèles
builder.Services.Configure<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

var app = builder.Build();

// Configuration du pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Gateway API v1");
        c.RoutePrefix = "gateway-swagger"; // éviter le conflit avec l'UI agrégée
    });
}


// Middleware de logging des requêtes
app.UseMiddleware<RequestLoggingMiddleware>();

// Middleware de gestion d'erreurs global
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseRouting();
// CORS
app.UseCors("Policy");

// Authentification et autorisation
app.UseAuthentication();
app.UseAuthorization();

// Routes API classiques
app.MapControllers();

// Endpoint de santé
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Service = "API Gateway", Timestamp = DateTime.UtcNow }));

// Ocelot + SwaggerForOcelot (terminal middleware - should be last)
app.UseSwaggerForOcelotUI(opt =>
{
    // Default path is /swagger/docs per library; not overriding to avoid mismatch
}).UseOcelot().Wait();

app.Run();
