using MediatR;
using SmartCareerPath.Application.Abstraction.DTOs.AI;
using SmartCareerPath.Application.Abstraction.ServicesContracts.AI;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCareerPath.Application.Commands.AI
{
    public class RecommendCareerPathFromQuizCommandHandler : IRequestHandler<RecommendCareerPathFromQuizCommand, QuizCareerRecommendationResult>
    {
        private readonly IAIService _aiService;
        public RecommendCareerPathFromQuizCommandHandler(IAIService aiService)
        {
            _aiService = aiService;
        }

        public async Task<QuizCareerRecommendationResult> Handle(RecommendCareerPathFromQuizCommand request, CancellationToken cancellationToken)
        {
            var dto = new QuizCareerRecommendationRequest
            {
                Interests = request.Interests,
                Answers = request.Answers
            };
            return await _aiService.RecommendCareerPathFromQuizAsync(dto);
        }
    }
}
