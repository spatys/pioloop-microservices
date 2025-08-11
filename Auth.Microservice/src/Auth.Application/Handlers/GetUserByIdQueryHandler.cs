using MediatR;
using Auth.Application.Queries;
using Auth.Application.DTOs.Response;
using Auth.Domain.Interfaces;

namespace Auth.Application.Handlers;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, ApiResponseDto<UserDto>>
{
    private readonly IUserRepository _userRepository;

    public GetUserByIdQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<ApiResponseDto<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(request.Id);
            if (user == null)
            {
                return ApiResponseDto<UserDto>.Error("Utilisateur non trouvé");
            }

            var userDto = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.GetFullName(),
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                EmailConfirmed = user.EmailConfirmed,
                ConsentAccepted = user.ConsentAccepted,
                ConsentAcceptedAt = user.ConsentAcceptedAt,
                Roles = new List<string>() // TODO: Ajouter les rôles
            };

            return ApiResponseDto<UserDto>.FromSuccess(userDto, "Utilisateur trouvé");
        }
        catch (Exception ex)
        {
            return ApiResponseDto<UserDto>.Error("Erreur interne du serveur");
        }
    }
}


