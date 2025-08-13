using Microsoft.OpenApi.Models;
using Email.Infrastructure.Services;
using Email.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using System.Reflection;
using Email.Application.Handlers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Email Microservice API", 
        Version = "1.0.0",
        Description = "API pour l'envoi d'emails dans l'écosystème Pioloop",
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

// Add MediatR (register handlers from Application assembly)
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(SendEmailVerificationHandler).Assembly));

builder.Services.AddScoped<IEmailService, EmailService>();

            // Enable CORS for API Gateway only
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowApiGateway", policy =>
                {
                    policy.WithOrigins(
                             "https://api.pioloop.com",          // Production API Gateway
                             "http://localhost:5000"              // Local API Gateway (HTTP)
                           )
                          .WithMethods("GET", "POST", "PUT", "DELETE", "PATCH")
                          .AllowAnyHeader();
                });
            });

var app = builder.Build();

// Serve Swagger UI under /swagger consistently
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Email Microservice API v1.0.0");
    c.RoutePrefix = "swagger";
    c.DocumentTitle = "Email Microservice API Documentation";
    c.DefaultModelsExpandDepth(-1);
});

// Enable CORS
app.UseCors("AllowApiGateway");

app.UseHttpsRedirection();

app.MapControllers();


app.Run();


