using System;
using System.Collections.Generic;

namespace SmartCareerPath.Application.Abstraction.DTOs.AI
{
    /// <summary>
    /// Request for resume analysis
    /// </summary>
    public class ResumeAnalysisRequest
    {
        public string ResumeText { get; set; }
        public string TargetRole { get; set; }
    }

    /// <summary>
    /// Result of resume analysis
    /// </summary>
    public class ResumeAnalysisResult
    {
        public ResumeScores Scores { get; set; }
        public string Summary { get; set; }
        public List<ResumeSuggestion> Suggestions { get; set; }
        public List<string> ExtractedSkills { get; set; }
    }

    /// <summary>
    /// Resume scoring breakdown
    /// </summary>
    public class ResumeScores
    {
        public decimal ATSScore { get; set; }
        public decimal ReadabilityScore { get; set; }
        public decimal SkillsRelevanceScore { get; set; }
        public decimal OverallScore { get; set; }
    }

    /// <summary>
    /// Resume improvement suggestion
    /// </summary>
    public class ResumeSuggestion
    {
        public string Category { get; set; }
        public string Suggestion { get; set; }
        public string Priority { get; set; } // High, Medium, Low
    }
}
