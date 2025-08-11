using Microsoft.OpenApi.Models;
using Email.Infrastructure.Services;
using Email.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Email Microservice API", Version = "v1" });
});
builder.Services.AddScoped<IEmailService, EmailService>();

// Enable CORS for API Gateway only
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowApiGateway", policy =>
    {
        policy.WithOrigins("http://localhost:5002")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Enable CORS
app.UseCors("AllowApiGateway");

app.UseHttpsRedirection();

app.MapControllers();

// Configure the application to listen on the correct port
app.Urls.Clear();
app.Urls.Add("http://0.0.0.0:80");

app.Run();


