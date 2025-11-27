using System.Collections.Generic;
using SmartCareerPath.Application.Abstraction.DTOs.AI;

namespace SmartCareerPath.APIs.Models
{
    public class RecommendCareerPathFromQuizRequest
    {
        public List<string> Interests { get; set; }
        public List<QuizAnswerForCareerDto> Answers { get; set; }
    }
}
