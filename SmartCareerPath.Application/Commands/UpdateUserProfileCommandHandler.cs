using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartCareerPath.Application.Abstraction.DTOs.User;
using SmartCareerPath.Domain.Contracts;

namespace SmartCareerPath.Application.Commands
{
    public class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, BaseResponse<UserProfileDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UpdateUserProfileCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<BaseResponse<UserProfileDto>> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
        {
            var profile = await _unitOfWork.UserProfiles
                .FirstOrDefaultAsync(p => p.UserId == request.UserId);

            if (profile == null)
                return BaseResponse<UserProfileDto>.FailureResult("Profile not found");

            // Update profile
            profile.Bio = request.Bio;
            profile.CurrentRole = request.CurrentRole;
            profile.ExperienceYears = request.ExperienceYears;
            profile.City = request.City;
            profile.Country = request.Country;
            profile.LinkedInUrl = request.LinkedInUrl;
            profile.GithubUrl = request.GithubUrl;
            profile.PortfolioUrl = request.PortfolioUrl;
            profile.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.UserProfiles.UpdateAsync(profile);
            await _unitOfWork.SaveChangesAsync();

            var profileDto = _mapper.Map<UserProfileDto>(profile);
            return BaseResponse<UserProfileDto>.SuccessResult(profileDto, "Profile updated successfully");
        }
    }
}
