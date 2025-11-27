using System;
using System.Collections.Generic;

namespace SmartCareerPath.Application.Abstraction.DTOs.AI
{
    /// <summary>
    /// Request for career path recommendations
    /// </summary>
    public class CareerPathRequest
    {
        public string CurrentRole { get; set; }
        public List<string> Skills { get; set; }
        public int ExperienceYears { get; set; }
        public string DesiredField { get; set; }
    }

    /// <summary>
    /// Career path recommendation result
    /// </summary>
    public class CareerPathResult
    {
        public List<string> RecommendedRoles { get; set; }
        public List<string> SkillsToLearn { get; set; }
        public List<string> Certifications { get; set; }
        public List<string> ActionPlan { get; set; }
    }

    /// <summary>
    /// Skill gap information
    /// </summary>
    public class SkillGap
    {
        public string Skill { get; set; }
        public string Proficiency { get; set; }
        public string ResourceUrl { get; set; }
        public int EstimatedMonths { get; set; }
    }
}
