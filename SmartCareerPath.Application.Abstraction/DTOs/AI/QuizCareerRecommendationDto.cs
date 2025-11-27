using System.Collections.Generic;

namespace SmartCareerPath.Application.Abstraction.DTOs.AI
{
    public class QuizCareerRecommendationRequest
    {
        public List<string> Interests { get; set; } = new();
        public List<QuizAnswerForCareerDto> Answers { get; set; } = new();
    }

    public class QuizAnswerForCareerDto
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public string UserAnswer { get; set; }
    }

    public class QuizCareerRecommendationResult
    {
        public string CareerPath { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Reasoning { get; set; } = string.Empty;
    }
}
