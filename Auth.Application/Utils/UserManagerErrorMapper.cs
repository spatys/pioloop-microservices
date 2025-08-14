using Microsoft.AspNetCore.Identity;

namespace Auth.Application.Utils;

public static class UserManagerErrorMapper
{
    public static Dictionary<string, string> MapPasswordErrors(IEnumerable<IdentityError> errors)
    {
        var mappedErrors = new Dictionary<string, string>();

        foreach (var error in errors)
        {
            switch (error.Code)
            {
                case "PasswordTooShort":
                    mappedErrors["newPassword"] = "Le mot de passe doit contenir au moins 6 caractères";
                    break;
                case "PasswordRequiresNonAlphanumeric":
                    mappedErrors["newPassword"] = "Le mot de passe doit contenir au moins un caractère spécial";
                    break;
                case "PasswordRequiresDigit":
                    mappedErrors["newPassword"] = "Le mot de passe doit contenir au moins un chiffre";
                    break;
                case "PasswordRequiresLower":
                    mappedErrors["newPassword"] = "Le mot de passe doit contenir au moins une lettre minuscule";
                    break;
                case "PasswordRequiresUpper":
                    mappedErrors["newPassword"] = "Le mot de passe doit contenir au moins une lettre majuscule";
                    break;
                case "PasswordMismatch":
                    mappedErrors["currentPassword"] = "Le mot de passe actuel est incorrect";
                    break;
                case "InvalidToken":
                    mappedErrors["token"] = "Token de réinitialisation invalide ou expiré";
                    break;
                case "UserNotFound":
                    mappedErrors["email"] = "Utilisateur non trouvé";
                    break;
                case "DuplicateEmail":
                    mappedErrors["email"] = "Cette adresse email est déjà utilisée";
                    break;
                case "InvalidEmail":
                    mappedErrors["email"] = "Format d'email invalide";
                    break;
                case "DuplicateUserName":
                    mappedErrors["email"] = "Cette adresse email est déjà utilisée";
                    break;
                default:
                    // Pour les erreurs non mappées, on les met sur le champ principal
                    if (!mappedErrors.ContainsKey("newPassword"))
                    {
                        mappedErrors["newPassword"] = error.Description;
                    }
                    else
                    {
                        // Si on a déjà une erreur sur newPassword, on l'ajoute
                        mappedErrors["newPassword"] += $"; {error.Description}";
                    }
                    break;
            }
        }

        return mappedErrors;
    }

    public static Dictionary<string, string> MapUserErrors(IEnumerable<IdentityError> errors)
    {
        var mappedErrors = new Dictionary<string, string>();

        foreach (var error in errors)
        {
            switch (error.Code)
            {
                case "DuplicateEmail":
                    mappedErrors["email"] = "Cette adresse email est déjà utilisée";
                    break;
                case "InvalidEmail":
                    mappedErrors["email"] = "Format d'email invalide";
                    break;
                case "DuplicateUserName":
                    mappedErrors["email"] = "Cette adresse email est déjà utilisée";
                    break;
                case "InvalidUserName":
                    mappedErrors["email"] = "Format d'email invalide";
                    break;
                case "UserAlreadyInRole":
                    mappedErrors["role"] = "L'utilisateur a déjà ce rôle";
                    break;
                case "UserNotInRole":
                    mappedErrors["role"] = "L'utilisateur n'a pas ce rôle";
                    break;
                default:
                    // Pour les erreurs non mappées, on les met sur le champ principal
                    if (!mappedErrors.ContainsKey("general"))
                    {
                        mappedErrors["general"] = error.Description;
                    }
                    else
                    {
                        mappedErrors["general"] += $"; {error.Description}";
                    }
                    break;
            }
        }

        return mappedErrors;
    }
}
