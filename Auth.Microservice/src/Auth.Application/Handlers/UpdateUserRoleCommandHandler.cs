using MediatR;
using Microsoft.AspNetCore.Identity;
using Auth.Application.Commands;
using Auth.Application.DTOs.Response;
using Auth.Domain.Identity;

namespace Auth.Application.Handlers;

public class UpdateUserRoleCommandHandler : IRequestHandler<UpdateUserRoleCommand, ApiResponseDto<object>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;

    public UpdateUserRoleCommandHandler(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<ApiResponseDto<object>> Handle(UpdateUserRoleCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Vérifier que l'utilisateur existe
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
            {
                return ApiResponseDto<object>.Error("Utilisateur non trouvé");
            }

            // Vérifier que le rôle existe
            var roleExists = await _roleManager.RoleExistsAsync(request.NewRole);
            if (!roleExists)
            {
                return ApiResponseDto<object>.Error($"Le rôle '{request.NewRole}' n'existe pas");
            }

            // Récupérer les rôles actuels de l'utilisateur
            var currentRoles = await _userManager.GetRolesAsync(user);

            // Supprimer tous les rôles actuels
            if (currentRoles.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeResult.Succeeded)
                {
                    return ApiResponseDto<object>.Error("Erreur lors de la suppression des rôles actuels");
                }
            }

            // Ajouter le nouveau rôle
            var addResult = await _userManager.AddToRoleAsync(user, request.NewRole);
            if (!addResult.Succeeded)
            {
                return ApiResponseDto<object>.Error("Erreur lors de l'ajout du nouveau rôle");
            }

            return ApiResponseDto<object>.FromSuccess(null);
        }
        catch (Exception ex)
        {
            return ApiResponseDto<object>.Error($"Erreur lors de la mise à jour du rôle: {ex.Message}");
        }
    }
}
