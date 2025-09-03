using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ApiGateway.Middleware;

public class JwtCookieMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<JwtCookieMiddleware> _logger;
    private readonly IConfiguration _configuration;

    public JwtCookieMiddleware(RequestDelegate next, ILogger<JwtCookieMiddleware> logger, IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            _logger.LogInformation("JwtCookieMiddleware: Début du traitement pour {Path}", context.Request.Path);
            
            // Extraire le JWT du cookie auth_token (priorité aux cookies de la requête)
            string? jwtToken = null;
            
            // 1. Essayer d'abord les cookies de la requête (méthode standard)
            if (context.Request.Cookies.TryGetValue("auth_token", out var cookieToken) && !string.IsNullOrEmpty(cookieToken))
            {
                jwtToken = cookieToken;
                _logger.LogInformation("JwtCookieMiddleware: Cookie auth_token trouvé dans Request.Cookies, longueur: {Length}", jwtToken.Length);
            }
            // 2. Essayer le header Cookie (pour credentials: 'include')
            else if (context.Request.Headers.TryGetValue("Cookie", out var cookieHeader) && !string.IsNullOrEmpty(cookieHeader))
            {
                var cookieValue = cookieHeader.ToString();
                var authTokenMatch = System.Text.RegularExpressions.Regex.Match(cookieValue, @"auth_token=([^;]+)");
                if (authTokenMatch.Success)
                {
                    jwtToken = authTokenMatch.Groups[1].Value;
                    _logger.LogInformation("JwtCookieMiddleware: Cookie auth_token extrait du header Cookie, longueur: {Length}", jwtToken.Length);
                }
            }
            
            if (!string.IsNullOrEmpty(jwtToken))
            {
                // Valider le JWT et extraire les claims
                var userClaims = ValidateJwtAndExtractClaims(jwtToken);
                
                if (userClaims != null)
                {
                    // Injecter les headers utilisateur pour les microservices
                    context.Request.Headers["X-User-Id"] = userClaims.UserId;
                    context.Request.Headers["X-User-Email"] = userClaims.Email;
                    context.Request.Headers["X-User-Roles"] = string.Join(",", userClaims.Roles);
                    
                    _logger.LogInformation("JwtCookieMiddleware: Headers utilisateur injectés: UserId={UserId}, Email={Email}, Roles={Roles}", 
                        userClaims.UserId, userClaims.Email, string.Join(",", userClaims.Roles));
                }
                else
                {
                    _logger.LogWarning("JwtCookieMiddleware: JWT invalide dans le cookie auth_token");
                }
            }
            else
            {
                _logger.LogInformation("JwtCookieMiddleware: Aucun cookie auth_token trouvé dans la requête (ni dans Request.Cookies ni dans header Cookie)");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "JwtCookieMiddleware: Erreur lors de la validation du JWT du cookie");
        }

        await _next(context);
    }

    private UserClaims? ValidateJwtAndExtractClaims(string token)
    {
        try
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured"));
            
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
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

            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            
            if (validatedToken is JwtSecurityToken jwtToken)
            {
                var userId = principal.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
                var email = principal.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value;
                var roles = principal.FindAll("http://schemas.microsoft.com/ws/2008/06/identity/claims/role").Select(c => c.Value).ToList();

                if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(email))
                {
                    return new UserClaims
                    {
                        UserId = userId,
                        Email = email,
                        Roles = roles
                    };
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la validation du JWT");
        }

        return null;
    }

    private class UserClaims
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
    }
}
