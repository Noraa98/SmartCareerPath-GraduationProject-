using MediatR;
using SmartCareerPath.Application.Abstraction.DTOs.AI;
using System.Collections.Generic;

namespace SmartCareerPath.Application.Commands.AI
{
    public class RecommendCareerPathFromQuizCommand : IRequest<QuizCareerRecommendationResult>
    {
        public List<string> Interests { get; set; }
        public List<QuizAnswerForCareerDto> Answers { get; set; }
    }
}
