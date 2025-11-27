using System;
using System.Collections.Generic;

namespace SmartCareerPath.Application.Abstraction.DTOs.AI
{
    #region Resume Analysis Request DTOs

    /// <summary>
    /// Request to analyze resume
    /// </summary>
    public class AnalyzeResumeRequest
    {
        public string? TargetRole { get; set; }
    }

    /// <summary>
    /// Request to extract skills from resume
    /// </summary>
    public class ExtractSkillsRequest
    {
        public string ResumeText { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request for resume improvement suggestions
    /// </summary>
    public class ResumeSuggestionsRequest
    {
        public string ResumeText { get; set; } = string.Empty;
    }

    #endregion

    #region Job Matching Request DTOs

    /// <summary>
    /// Request for job match calculation
    /// </summary>
    public class JobMatchCalculateRequest
    {
        public int? ResumeId { get; set; }
    }

    /// <summary>
    /// Request to generate cover letter for a job
    /// </summary>
    public class GenerateCoverLetterRequest
    {
        public int? ResumeId { get; set; }
    }

    #endregion

    #region Career Path Request DTOs

    /// <summary>
    /// Request for skill gap analysis
    /// </summary>
    public class SkillGapRequest
    {
        public List<string> CurrentSkills { get; set; } = new List<string>();
        public string TargetRole { get; set; } = string.Empty;
    }

    #endregion

    #region General AI Request DTOs

    /// <summary>
    /// Request for custom AI prompt (Admin only)
    /// </summary>
    public class CustomPromptRequest
    {
        public string Prompt { get; set; } = string.Empty;
        public string? SystemPrompt { get; set; }
        public int MaxTokens { get; set; } = 1000;
        public double Temperature { get; set; } = 0.7;
    }

    #endregion
}
