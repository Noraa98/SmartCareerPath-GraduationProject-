using MediatR;

namespace SmartCareerPath.Application.Abstraction.DTOs.User
{
    public class GetUserProfileQuery : IRequest<BaseResponse<UserProfileDto>>
    {
        public int UserId { get; set; }
    }
}
