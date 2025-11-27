using System;
using System.Collections.Generic;

namespace SmartCareerPath.Application.Abstraction.DTOs.AI
{
    /// <summary>
    /// Request for job matching
    /// </summary>
    public class JobMatchRequest
    {
        public string ResumeText { get; set; }
        public string JobDescription { get; set; }
        public List<string> RequiredSkills { get; set; }
    }

    /// <summary>
    /// Result of job matching
    /// </summary>
    public class JobMatchResult
    {
        public decimal MatchPercentage { get; set; }
        public List<string> MatchedSkills { get; set; }
        public List<string> MissingSkills { get; set; }
        public List<string> Recommendations { get; set; }
    }
}
