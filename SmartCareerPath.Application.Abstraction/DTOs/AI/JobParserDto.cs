using System;
using System.Collections.Generic;

namespace SmartCareerPath.Application.Abstraction.DTOs.AI
{
    #region Job Parsing

    /// <summary>
    /// Request for parsing job description
    /// </summary>
    public class ParseJobDescriptionRequest
    {
        public string JobDescription { get; set; } = string.Empty;
        public string? JobTitle { get; set; }
        public string? CompanyName { get; set; }
    }

    /// <summary>
    /// Result of job description parsing
    /// </summary>
    public class ParseJobDescriptionResult
    {
        public JobBasicInfo BasicInfo { get; set; }
        public List<string> RequiredSkills { get; set; }
        public List<string> PreferredSkills { get; set; }
        public List<string> Responsibilities { get; set; }
        public List<string> Requirements { get; set; }
        public JobBenefits Benefits { get; set; }
        public List<string> KeyTechnologies { get; set; }
        public string SeniorityLevel { get; set; } // "entry-level", "mid-level", "senior", "lead", "c-level"
        public string EmploymentType { get; set; } // "full-time", "part-time", "contract", "freelance"
        public string ParseQuality { get; set; } // "excellent", "good", "fair", "poor"
        public decimal ParseConfidenceScore { get; set; } // 0-100
    }

    /// <summary>
    /// Basic information about the job
    /// </summary>
    public class JobBasicInfo
    {
        public string JobTitle { get; set; }
        public string CompanyName { get; set; }
        public string Location { get; set; }
        public bool IsRemote { get; set; }
        public string SalaryRangeMin { get; set; }
        public string SalaryRangeMax { get; set; }
        public string SalaryCurrency { get; set; }
        public string SalaryPeriod { get; set; } // "annual", "monthly", "hourly"
        public int ExperienceYearsRequired { get; set; }
        public string Department { get; set; }
        public List<string> ReportingLine { get; set; }
    }

    /// <summary>
    /// Benefits information
    /// </summary>
    public class JobBenefits
    {
        public List<string> Benefits { get; set; }
        public string HealthInsurance { get; set; }
        public string RetirementPlan { get; set; }
        public int PaidTimeOff { get; set; } // in days
        public string ProfessionalDevelopment { get; set; }
        public bool RemoteWorkOptions { get; set; }
        public bool FlexibleSchedule { get; set; }
    }

    /// <summary>
    /// Request for extracting skills from job description
    /// </summary>
    public class ExtractJobSkillsRequest
    {
        public string JobDescription { get; set; }
    }

    /// <summary>
    /// Extracted skills from job description
    /// </summary>
    public class ExtractJobSkillsResult
    {
        public List<SkillDetail> RequiredSkills { get; set; }
        public List<SkillDetail> PreferredSkills { get; set; }
        public List<string> KeyTechnologies { get; set; }
        public int TotalSkillsIdentified { get; set; }
    }

    /// <summary>
    /// Detailed skill information
    /// </summary>
    public class SkillDetail
    {
        public string SkillName { get; set; }
        public string Category { get; set; } // "technical", "soft", "domain", "tool"
        public string ProficiencyLevel { get; set; } // "beginner", "intermediate", "advanced", "expert"
        public string ExperienceLevel { get; set; } // "fresh", "1-3 years", "3-5 years", "5+ years"
        public double RelevanceScore { get; set; } // 0-100, how critical for the role
        public string Importance { get; set; } // "critical", "important", "nice-to-have"
    }

    /// <summary>
    /// Request for matching candidate skills to job requirements
    /// </summary>
    public class MatchSkillsToJobRequest
    {
        public List<string> CandidateSkills { get; set; }
        public string JobDescription { get; set; }
    }

    /// <summary>
    /// Result of matching skills to job
    /// </summary>
    public class MatchSkillsToJobResult
    {
        public List<string> MatchedSkills { get; set; }
        public List<string> MissingRequiredSkills { get; set; }
        public List<string> MissingPreferredSkills { get; set; }
        public decimal MatchPercentageRequired { get; set; }
        public decimal MatchPercentagePreferred { get; set; }
        public decimal OverallMatchPercentage { get; set; }
        public List<SkillGapWithPriority> SkillGaps { get; set; }
        public List<string> RecommendedLearningPath { get; set; }
    }

    /// <summary>
    /// Skill gap with priority information
    /// </summary>
    public class SkillGapWithPriority
    {
        public string SkillName { get; set; }
        public string Importance { get; set; } // "critical", "important", "nice-to-have"
        public int EstimatedLearningTimeWeeks { get; set; }
        public List<string> LearningResources { get; set; }
        public string CertificationPath { get; set; }
    }

    #endregion
}
