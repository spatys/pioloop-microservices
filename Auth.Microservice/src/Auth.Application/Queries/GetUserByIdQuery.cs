using MediatR;
using Auth.Application.DTOs.Response;

namespace Auth.Application.Queries;

public class GetUserByIdQuery : IRequest<ApiResponseDto<ApplicationUserDto>>
{
    public Guid Id { get; set; }
}
