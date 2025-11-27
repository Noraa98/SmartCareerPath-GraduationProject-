using System;
using System.Collections.Generic;

namespace SmartCareerPath.Application.Abstraction.DTOs.AI
{
    /// <summary>
    /// Request for interview analysis
    /// </summary>
    public class InterviewAnalysisRequest
    {
        public string Question { get; set; }
        public string UserAnswer { get; set; }
        public string InterviewType { get; set; }
    }

    /// <summary>
    /// Result of interview analysis
    /// </summary>
    public class InterviewAnalysisResult
    {
        public decimal OverallScore { get; set; }
        public string Feedback { get; set; }
        public List<string> Strengths { get; set; }
        public List<string> Improvements { get; set; }
    }
}
