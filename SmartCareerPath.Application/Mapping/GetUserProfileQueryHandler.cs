using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartCareerPath.Application.Abstraction.DTOs.User;
using SmartCareerPath.Domain.Contracts;

namespace SmartCareerPath.Application.Mapping
{
    public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, BaseResponse<UserProfileDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetUserProfileQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<BaseResponse<UserProfileDto>> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
        {
            var profile = await _unitOfWork.UserProfiles
                .Include(p => p.UserInterests)
                    .ThenInclude(ui => ui.Interest)
                .FirstOrDefaultAsync(p => p.UserId == request.UserId);

            if (profile == null)
                return BaseResponse<UserProfileDto>.FailureResult("Profile not found");

            // Get user skills
            var skills = await _unitOfWork.UserSkills
                .Include(s => s.Skill)
                .Where(s => s.UserId == request.UserId)
                .ToListAsync();

            var profileDto = _mapper.Map<UserProfileDto>(profile);
            profileDto.Skills = _mapper.Map<List<UserSkillDto>>(skills);

            return BaseResponse<UserProfileDto>.SuccessResult(profileDto);
        }
    }
}
