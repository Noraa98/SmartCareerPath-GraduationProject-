using System;
using System.Collections.Generic;

namespace SmartCareerPath.Application.Abstraction.DTOs.AI
{
    #region CV Generation

    /// <summary>
    /// Request for generating a new CV
    /// </summary>
    public class GenerateCVRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string CurrentRole { get; set; } = string.Empty;
        public int ExperienceYears { get; set; }
        public string Summary { get; set; } = string.Empty;
        public List<string> Skills { get; set; } = new List<string>();
        public string CVTemplate { get; set; } = "professional"; // "professional", "creative", "modern", "simple"
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Location { get; set; }
    }

    /// <summary>
    /// Result of CV generation
    /// </summary>
    public class GenerateCVResult
    {
        public string CVContent { get; set; }
        public string CVHtmlFormat { get; set; }
        public string TemplateName { get; set; }
        public string DownloadUrl { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    #endregion

    #region CV Improvement

    /// <summary>
    /// Request for improving an existing CV
    /// </summary>
    public class ImproveCVRequest
    {
        public string CurrentCVText { get; set; }
        public string? TargetRole { get; set; }
        public string ImprovementArea { get; set; } // "content", "formatting", "skills", "overall"
    }

    /// <summary>
    /// Result of CV improvement
    /// </summary>
    public class ImproveCVResult
    {
        public string ImprovedCVContent { get; set; }
        public List<ImprovementSuggestion> Suggestions { get; set; }
        public string Summary { get; set; }
        public decimal ImprovementScore { get; set; } // 0-100
        public List<string> ChangesApplied { get; set; }
    }

    /// <summary>
    /// Improvement suggestion for CV
    /// </summary>
    public class ImprovementSuggestion
    {
        public string Section { get; set; } // "header", "summary", "experience", "skills", "education"
        public string CurrentText { get; set; }
        public string SuggestedText { get; set; }
        public string Reason { get; set; }
        public int Priority { get; set; } // 1=High, 2=Medium, 3=Low
    }

    #endregion

    #region CV Parsing

    /// <summary>
    /// Request for parsing CV content
    /// </summary>
    public class ParseCVRequest
    {
        public string CVContent { get; set; }
    }

    /// <summary>
    /// Result of CV parsing
    /// </summary>
    public class ParseCVResult
    {
        public PersonalInfo PersonalInfo { get; set; }
        public ProfessionalSummary ProfessionalSummary { get; set; }
        public List<WorkExperience> WorkExperience { get; set; }
        public List<Education> Education { get; set; }
        public List<string> Skills { get; set; }
        public List<Certification> Certifications { get; set; }
        public List<string> Languages { get; set; }
        public string ParseQuality { get; set; } // "excellent", "good", "fair", "poor"
        public decimal ParseConfidenceScore { get; set; } // 0-100
    }

    /// <summary>
    /// Personal information extracted from CV
    /// </summary>
    public class PersonalInfo
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Location { get; set; }
        public string LinkedInUrl { get; set; }
        public string GithubUrl { get; set; }
        public string PortfolioUrl { get; set; }
    }

    /// <summary>
    /// Professional summary/profile from CV
    /// </summary>
    public class ProfessionalSummary
    {
        public string Summary { get; set; }
        public int YearsOfExperience { get; set; }
        public string CurrentRole { get; set; }
        public string CareerFocus { get; set; }
    }

    /// <summary>
    /// Work experience entry
    /// </summary>
    public class WorkExperience
    {
        public string CompanyName { get; set; }
        public string JobTitle { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsCurrentRole { get; set; }
        public int DurationMonths { get; set; }
        public string Description { get; set; }
        public List<string> KeyAchievements { get; set; }
        public List<string> SkillsUsed { get; set; }
    }

    /// <summary>
    /// Education entry
    /// </summary>
    public class Education
    {
        public string Institution { get; set; }
        public string Degree { get; set; }
        public string FieldOfStudy { get; set; }
        public int GraduationYear { get; set; }
        public string GPA { get; set; }
        public List<string> Coursework { get; set; }
    }

    /// <summary>
    /// Certification entry
    /// </summary>
    public class Certification
    {
        public string CertificationName { get; set; }
        public string IssuingOrganization { get; set; }
        public int IssuedYear { get; set; }
        public int? ExpirationYear { get; set; }
        public string CredentialUrl { get; set; }
    }

    #endregion
}
