using System.Collections.Generic;

namespace SmartCareerPath.Application.Abstraction.DTOs.AI
{
    public class InterviewSummaryRequest
    {
        // List of questions asked in the interview
        public List<string> Questions { get; set; } = new();
        // Corresponding user answers (same order)
        public List<string> Answers { get; set; } = new();
        // Optional per-question feedback provided by AI or human
        public List<string> Feedbacks { get; set; } = new();
        // Optional interview meta (role, interviewer notes, etc.)
        public string? Role { get; set; }
        public string? InterviewType { get; set; }
    }

    public class InterviewSummaryResult
    {
        // Short summary suitable for display on results page
        public string Summary { get; set; } = string.Empty;
        // Overall score (0-10)
        public decimal OverallScore { get; set; }
        // Top strengths aggregated across answers
        public List<string> TopStrengths { get; set; } = new();
        // Main improvement areas aggregated across answers
        public List<string> MainImprovements { get; set; } = new();
        // Optional detailed per-question summaries
        public List<string> PerQuestionSummary { get; set; } = new();
    }
}
