using Microsoft.OpenApi.Models;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Serilog;
using System.Reflection;
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

// Configuration Ocelot
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Configuration.AddJsonFile($"ocelot.{builder.Environment.EnvironmentName}.json", optional: true);

// Configuration SwaggerForOcelot
builder.Services.AddSwaggerForOcelot(builder.Configuration, opt => {
    opt.GenerateDocsForAggregates = false;
});


// L'API Gateway utilise le middleware JwtCookieMiddleware pour valider les JWT
// et injecter les headers utilisateur pour les microservices

// Configuration Swagger avec MMLib.SwaggerForOcelot
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Auth Service API",
        Version = "v1",
        Description = "Authentication Microservice"
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
              .AllowAnyHeader()
              .AllowCredentials()
              .SetIsOriginAllowed(origin => true); // Pour le développement
    });
});

// Configuration des services HTTP avec transmission des cookies
builder.Services.AddHttpClient("OcelotClient")
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        UseCookies = true,
        CookieContainer = new System.Net.CookieContainer()
    });

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

// CORS
app.UseCors("Policy");

// Middleware pour extraire le JWT du cookie et l'ajouter au header Authorization
// DOIT être AVANT UseRouting() et UseOcelot() pour s'exécuter sur toutes les requêtes
app.UseMiddleware<JwtCookieMiddleware>();

app.UseRouting();

// Routes API classiques (doivent être AVANT Ocelot pour ne pas être interceptées)
app.MapControllers();

// Ocelot + SwaggerForOcelot (terminal)
app.UseSwaggerForOcelotUI().UseOcelot().Wait();

app.Run();
