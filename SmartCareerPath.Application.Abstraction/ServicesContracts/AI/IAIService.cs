
using SmartCareerPath.Application.Abstraction.DTOs.AI;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace SmartCareerPath.Application.Abstraction.ServicesContracts.AI
{
    /// <summary>
    /// Service contract for AI operations
    /// </summary>
    public interface IAIService
    {
        /// <summary>
        /// Match candidate skills to job requirements
        /// </summary>
        Task<MatchSkillsToJobResult> MatchSkillsToJobAsync(MatchSkillsToJobRequest request);
        /// <summary>
        /// Analyze resume and return analysis results
        /// </summary>
        Task<ResumeAnalysisResult> AnalyzeResumeAsync(ResumeAnalysisRequest request);

        /// <summary>
        /// Extract skills from resume text
        /// </summary>
        Task<List<string>> ExtractSkillsFromResumeAsync(string resumeText);

        /// <summary>
        /// Get resume improvement suggestions
        /// </summary>
        Task<List<ResumeSuggestion>> SuggestResumeImprovementsAsync(string resumeText);

        /// <summary>
        /// Calculate job match score
        /// </summary>
        Task<JobMatchResult> MatchResumeToJobAsync(JobMatchRequest request);

        /// <summary>
        /// Generate cover letter
        /// </summary>
        Task<string> GenerateCoverLetterAsync(string jobDescription, string resumeText, string companyName);

        /// <summary>
        /// Analyze interview answer
        /// </summary>
        Task<InterviewAnalysisResult> AnalyzeInterviewAnswerAsync(InterviewAnalysisRequest request);

        /// <summary>
        /// Generate a consolidated interview summary given questions, answers and feedback
        /// </summary>
        Task<InterviewSummaryResult> GenerateInterviewSummaryAsync(InterviewSummaryRequest request);

        /// <summary>
        /// Generate interview questions
        /// </summary>
        Task<List<string>> GenerateInterviewQuestionsAsync(string role, string interviewType, int questionCount);

        /// <summary>
        /// Get career path recommendations
        /// </summary>
        Task<CareerPathResult> RecommendCareerPathAsync(CareerPathRequest request);

        /// <summary>
        /// Identify skill gaps
        /// </summary>
        Task<List<SkillGap>> IdentifySkillGapsAsync(List<string> currentSkills, string targetRole);

        /// <summary>
        /// Send custom prompt to AI
        /// </summary>
        Task<AIPromptResponse> SendPromptAsync(AIPromptRequest request);

        #region CV Builder Features

        /// <summary>
        /// Generate a new CV from user information
        /// </summary>
        Task<GenerateCVResult> GenerateCVAsync(GenerateCVRequest request);

        /// <summary>
        /// Improve an existing CV with AI suggestions
        /// </summary>
        Task<ImproveCVResult> ImproveCVAsync(ImproveCVRequest request);

        /// <summary>
        /// Parse CV content and extract structured information
        /// </summary>
        Task<ParseCVResult> ParseCVAsync(ParseCVRequest request);

        #endregion


        /// <summary>
        /// Recommend a career path based on quiz answers and interests
        /// </summary>
        Task<QuizCareerRecommendationResult> RecommendCareerPathFromQuizAsync(QuizCareerRecommendationRequest request);

        #region Job Description Parser Features

        /// <summary>
        /// Parse job description and extract structured information
        /// </summary>
        Task<ParseJobDescriptionResult> ParseJobDescriptionAsync(ParseJobDescriptionRequest request);

        /// <summary>
        /// Extract required and preferred skills from job description
        /// </summary>
        Task<ExtractJobSkillsResult> ExtractJobSkillsAsync(ExtractJobSkillsRequest request);

        /// <summary>
        /// Generate MCQ quiz questions based on interests
        /// </summary>
        Task<List<QuizMCQQuestionDto>> GenerateQuizMCQQuestionsAsync(List<string> interests, int questionCount);

        #endregion
    }
}
