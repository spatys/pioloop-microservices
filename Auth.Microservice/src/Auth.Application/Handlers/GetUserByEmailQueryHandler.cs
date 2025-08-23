using MediatR;
using Auth.Application.Queries;
using Auth.Application.DTOs.Response;
using Microsoft.AspNetCore.Identity;
using Auth.Domain.Identity;

namespace Auth.Application.Handlers;

public class GetUserByEmailQueryHandler : IRequestHandler<GetUserByEmailQuery, ApiResponseDto<ApplicationUserDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public GetUserByEmailQueryHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<ApiResponseDto<ApplicationUserDto>> Handle(GetUserByEmailQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return ApiResponseDto<ApplicationUserDto>.Error("Utilisateur non trouvé");
            }

            var userDto = new ApplicationUserDto
            {
                Id = user.Id,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.GetFullName(),
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                EmailConfirmed = user.EmailConfirmed,
                Roles = new List<string>()
            };

            return ApiResponseDto<ApplicationUserDto>.FromSuccess(userDto, "Utilisateur trouvé");
        }
        catch (Exception ex)
        {
            return ApiResponseDto<ApplicationUserDto>.Error("Erreur interne du serveur");
        }
    }
}


