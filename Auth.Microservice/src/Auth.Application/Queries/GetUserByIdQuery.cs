using MediatR;
using Auth.Application.DTOs.Response;

namespace Auth.Application.Queries;

public class GetUserByIdQuery : IRequest<ApiResponseDto<UserDto>>
{
    public Guid Id { get; set; }
}
