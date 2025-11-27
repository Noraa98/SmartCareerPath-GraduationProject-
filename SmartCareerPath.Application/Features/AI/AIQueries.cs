using MediatR;
using SmartCareerPath.Application.Abstraction.DTOs.AI;
using SmartCareerPath.Application.Abstraction.ServicesContracts.AI;
using SmartCareerPath.Application.PipelineBehaviors.BaseResponse;
using SmartCareerPath.Domain.Contracts;
using SmartCareerPath.Domain.Entities;
using SmartCareerPath.Domain.Entities.ProfileAndInterests;
using SmartCareerPath.Domain.Entities.SkillManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCareerPath.Application.Features.AI.Queries
{
    #region Career Path Recommendation

    /// <summary>
    /// Get AI career path recommendations
    /// </summary>
    public class GetCareerPathRecommendationsQuery : IRequest<BaseResponse<CareerPathResult>>
    {
        public int UserId { get; set; }
        public string? DesiredField { get; set; }
    }

    public class GetCareerPathRecommendationsQueryHandler : IRequestHandler<GetCareerPathRecommendationsQuery, BaseResponse<CareerPathResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAIService _aiService;

        public GetCareerPathRecommendationsQueryHandler(IUnitOfWork unitOfWork, IAIService aiService)
        {
            _unitOfWork = unitOfWork;
            _aiService = aiService;
        }

        public async Task<BaseResponse<CareerPathResult>> Handle(
            GetCareerPathRecommendationsQuery request,
            CancellationToken cancellationToken)
        {
            // Get user profile
            var profile = await _unitOfWork.Repository<UserProfile>()
                .FirstOrDefaultAsync(p => p.UserId == request.UserId);

            if (profile == null)
                return BaseResponse<CareerPathResult>.FailureResult("Profile not found");

            try
            {
                // TODO: Load user skills from database
                var skillsList = new List<string>();

                var recommendations = await _aiService.RecommendCareerPathAsync(new CareerPathRequest
                {
                    CurrentRole = profile.CurrentRole,
                    Skills = skillsList,
                    ExperienceYears = profile.ExperienceYears ?? 0,
                    DesiredField = request.DesiredField ?? ""
                });

                if (recommendations == null)
                    return BaseResponse<CareerPathResult>.FailureResult("Failed to get recommendations");

                return BaseResponse<CareerPathResult>.SuccessResult(
                    recommendations,
                    "Recommendations generated successfully");
            }
            catch (Exception ex)
            {
                return BaseResponse<CareerPathResult>.FailureResult($"Recommendation failed: {ex.Message}");
            }
        }
    }

    #endregion
}
