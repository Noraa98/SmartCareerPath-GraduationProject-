using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SmartCareerPath.Application.Abstraction.DTOs.AI
{
   public class QuizMCQQuestionDto
{
    [JsonPropertyName("QuestionText")]
    public string QuestionText { get; set; }
    [JsonPropertyName("Choices")]
    public List<string> Choices { get; set; }
}

    public class GenerateQuizQuestionsRequest
    {
        public List<string> Interests { get; set; }
        public int QuestionCount { get; set; } = 5;
    }
}
