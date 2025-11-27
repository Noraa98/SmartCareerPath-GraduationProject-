using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartCareerPath.Application.Abstraction.DTOs.AI;
using SmartCareerPath.Application.Abstraction.ServicesContracts.AI;
using SmartCareerPath.Application.PipelineBehaviors.BaseResponse;
using SmartCareerPath.Domain.Contracts;
using SmartCareerPath.Domain.Entities;
using SmartCareerPath.Domain.Entities.ResumeAndParsing;
using SmartCareerPath.Domain.Entities.JobPostingAndMatching;
using SmartCareerPath.Domain.Entities.InterviewSystem;
using SmartCareerPath.Domain.Entities.AIEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCareerPath.Application.Features.AI.Commands
{
    #region Resume AI Analysis

    /// <summary>
    /// Analyze resume with AI and save results
    /// </summary>
    public class AnalyzeResumeWithAICommand : IRequest<BaseResponse<ResumeAnalysisResult>>
    {
        public int UserId { get; set; }
        public int ResumeId { get; set; }
        public string? TargetRole { get; set; }
    }

    public class AnalyzeResumeWithAICommandHandler : IRequestHandler<AnalyzeResumeWithAICommand, BaseResponse<ResumeAnalysisResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAIService _aiService;

        public AnalyzeResumeWithAICommandHandler(
            IUnitOfWork unitOfWork,
            IAIService aiService)
        {
            _unitOfWork = unitOfWork;
            _aiService = aiService;
        }

        public async Task<BaseResponse<ResumeAnalysisResult>> Handle(
            AnalyzeResumeWithAICommand request,
            CancellationToken cancellationToken)
        {
            // Get resume
            var resume = await _unitOfWork.Repository<Resume>()
                .FirstOrDefaultAsync(r => r.Id == request.ResumeId && r.UserId == request.UserId);

            if (resume == null)
                return BaseResponse<ResumeAnalysisResult>.FailureResult("Resume not found");

            try
            {
                // Analyze with AI
                var analysisResult = await _aiService.AnalyzeResumeAsync(new ResumeAnalysisRequest
                {
                    ResumeText = resume.Content ?? "Resume content not available",
                    TargetRole = request.TargetRole
                });

                if (analysisResult == null)
                    return BaseResponse<ResumeAnalysisResult>.FailureResult("AI analysis failed");

                // Save scores
                var score = new ResumeScore
                {
                    ResumeId = resume.Id,
                    ATSScore = (int)analysisResult.Scores.ATSScore,
                    ReadabilityScore = (int)analysisResult.Scores.ReadabilityScore,
                    SkillsRelevanceScore = (int)analysisResult.Scores.SkillsRelevanceScore,
                    OverallScore = (int)analysisResult.Scores.OverallScore,
                    Notes = analysisResult.Summary,
                    ScoredAt = DateTime.UtcNow
                };

                await _unitOfWork.Repository<ResumeScore>().AddAsync(score);

                // Save suggestions
                if (analysisResult.Suggestions != null)
                {
                    foreach (var suggestion in analysisResult.Suggestions)
                    {
                        var priority = suggestion.Priority?.ToLower() == "high" ? 1 : 
                                       suggestion.Priority?.ToLower() == "medium" ? 2 : 3;
                        
                        var resumeSuggestion = new SmartCareerPath.Domain.Entities.ResumeAndParsing.ResumeSuggestion
                        {
                            ResumeId = resume.Id,
                            Category = suggestion.Category,
                            Suggestion = suggestion.Suggestion,
                            Priority = priority
                        };

                        await _unitOfWork.Repository<SmartCareerPath.Domain.Entities.ResumeAndParsing.ResumeSuggestion>().AddAsync(resumeSuggestion);
                    }
                }

                // Save extracted skills as keywords
                if (analysisResult.ExtractedSkills != null)
                {
                    foreach (var skill in analysisResult.ExtractedSkills)
                    {
                        var keyword = new ResumeKeyword
                        {
                            ResumeId = resume.Id,
                            Keyword = skill,
                            Category = "Skill",
                            Frequency = 1,
                            Importance = 1
                        };

                        await _unitOfWork.Repository<ResumeKeyword>().AddAsync(keyword);
                    }
                }

                // Log AI request
                var aiRequest = new AIRequest
                {
                    UserId = request.UserId,
                    Type = "CV_Improve",
                    InputText = $"Resume Analysis for Resume ID: {resume.Id}",
                    OutputText = analysisResult.Summary,
                    ModelName = "mistralai/mistral-7b-instruct",
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Repository<AIRequest>().AddAsync(aiRequest);

                await _unitOfWork.SaveChangesAsync();

                return BaseResponse<ResumeAnalysisResult>.SuccessResult(
                    analysisResult,
                    "Resume analyzed successfully");
            }
            catch (Exception ex)
            {
                return BaseResponse<ResumeAnalysisResult>.FailureResult($"Analysis failed: {ex.Message}");
            }
        }
    }

    #endregion

    #region Job Matching with AI

    /// <summary>
    /// Calculate job match score using AI
    /// </summary>
    public class CalculateJobMatchCommand : IRequest<BaseResponse<JobMatchResult>>
    {
        public int UserId { get; set; }
        public int JobPostingId { get; set; }
        public int? ResumeId { get; set; }
    }

    public class CalculateJobMatchCommandHandler : IRequestHandler<CalculateJobMatchCommand, BaseResponse<JobMatchResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAIService _aiService;

        public CalculateJobMatchCommandHandler(IUnitOfWork unitOfWork, IAIService aiService)
        {
            _unitOfWork = unitOfWork;
            _aiService = aiService;
        }

        public async Task<BaseResponse<JobMatchResult>> Handle(
            CalculateJobMatchCommand request,
            CancellationToken cancellationToken)
        {
            // Get job posting
            var job = await _unitOfWork.Repository<JobPosting>()
                .FirstOrDefaultAsync(j => j.Id == request.JobPostingId);

            if (job == null)
                return BaseResponse<JobMatchResult>.FailureResult("Job not found");

            // Get resume
            Resume? resume = null;
            if (request.ResumeId.HasValue)
            {
                resume = await _unitOfWork.Repository<Resume>()
                    .FirstOrDefaultAsync(r => r.Id == request.ResumeId.Value && r.UserId == request.UserId);
            }
            else
            {
                // Get primary resume
                resume = await _unitOfWork.Repository<Resume>()
                    .FirstOrDefaultAsync(r => r.UserId == request.UserId && r.IsPrimary);
            }

            if (resume == null)
                return BaseResponse<JobMatchResult>.FailureResult("Resume not found");

            try
            {
                // Get required skills
                var requiredSkills = new List<string>();
                // TODO: Implement skill loading based on your domain structure

                // Prepare job match request
                var matchRequest = new JobMatchRequest
                {
                    ResumeText = resume.Content ?? "Resume content",
                    JobDescription = job.Description,
                    RequiredSkills = requiredSkills
                };

                // Get AI analysis
                var matchResult = await _aiService.MatchResumeToJobAsync(matchRequest);

                if (matchResult == null)
                    return BaseResponse<JobMatchResult>.FailureResult("AI matching failed");

                // Save job analysis
                var analysis = new JobAnalysis
                {
                    UserId = request.UserId,
                    JobPostingId = job.Id,
                    ResumeId = resume.Id,
                    MatchPercentage = (int)matchResult.MatchPercentage,
                    SkillGapsJson = System.Text.Json.JsonSerializer.Serialize(matchResult.MissingSkills),
                    RecommendedActionsJson = System.Text.Json.JsonSerializer.Serialize(matchResult.Recommendations),
                    StrengthsJson = System.Text.Json.JsonSerializer.Serialize(matchResult.MatchedSkills),
                    AnalyzedAt = DateTime.UtcNow
                };

                await _unitOfWork.Repository<JobAnalysis>().AddAsync(analysis);
                await _unitOfWork.SaveChangesAsync();

                return BaseResponse<JobMatchResult>.SuccessResult(matchResult, "Job match calculated successfully");
            }
            catch (Exception ex)
            {
                return BaseResponse<JobMatchResult>.FailureResult($"Matching failed: {ex.Message}");
            }
        }
    }

    #endregion

    #region Interview AI Analysis

    /// <summary>
    /// Analyze interview answer with AI
    /// </summary>
    public class AnalyzeInterviewAnswerCommand : IRequest<BaseResponse<InterviewAnalysisResult>>
    {
        public int SessionId { get; set; }
        public int QuestionId { get; set; }
        public string? UserAnswer { get; set; }
        // Optional: provide the question text directly to perform ad-hoc analysis
        // without requiring a persisted InterviewSession / InterviewQuestion.
        public string? Question { get; set; }
        public string? InterviewType { get; set; }
    }

    public class AnalyzeInterviewAnswerCommandHandler : IRequestHandler<AnalyzeInterviewAnswerCommand, BaseResponse<InterviewAnalysisResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAIService _aiService;

        public AnalyzeInterviewAnswerCommandHandler(IUnitOfWork unitOfWork, IAIService aiService)
        {
            _unitOfWork = unitOfWork;
            _aiService = aiService;
        }

        public async Task<BaseResponse<InterviewAnalysisResult>> Handle(
            AnalyzeInterviewAnswerCommand request,
            CancellationToken cancellationToken)
        {
            // Try to load session/question when IDs are provided.
            InterviewSession? session = null;
            InterviewQuestion? question = null;

            if (request.SessionId > 0)
            {
                session = await _unitOfWork.Repository<InterviewSession>()
                    .FirstOrDefaultAsync(s => s.Id == request.SessionId);
            }

            if (request.QuestionId > 0)
            {
                question = await _unitOfWork.Repository<InterviewQuestion>()
                    .FirstOrDefaultAsync(q => q.Id == request.QuestionId);
            }

            // If we have both persisted session and question, proceed with DB-updating flow.
            if (session != null && question != null)
            {
                try
                {
                    var analysisResult = await _aiService.AnalyzeInterviewAnswerAsync(new InterviewAnalysisRequest
                    {
                        Question = question.QuestionText,
                        UserAnswer = request.UserAnswer,
                        InterviewType = session.InterviewType
                    });

                    if (analysisResult == null)
                        return BaseResponse<InterviewAnalysisResult>.FailureResult("AI analysis failed");

                    // Update persisted question with feedback
                    question.UserAnswer = request.UserAnswer;
                    question.Score = (int?)analysisResult.OverallScore;
                    question.FeedbackJson = System.Text.Json.JsonSerializer.Serialize(analysisResult);
                    question.AnsweredAt = DateTime.UtcNow;

                    await _unitOfWork.Repository<InterviewQuestion>().UpdateAsync(question);
                    await _unitOfWork.SaveChangesAsync();

                    return BaseResponse<InterviewAnalysisResult>.SuccessResult(
                        analysisResult,
                        "Answer analyzed successfully");
                }
                catch (Exception ex)
                {
                    return BaseResponse<InterviewAnalysisResult>.FailureResult($"Analysis failed: {ex.Message}");
                }
            }

            // If no persisted session/question, allow ad-hoc analysis when the caller provides the question text.
            if (!string.IsNullOrWhiteSpace(request.Question))
            {
                try
                {
                    var analysisResult = await _aiService.AnalyzeInterviewAnswerAsync(new InterviewAnalysisRequest
                    {
                        Question = request.Question,
                        UserAnswer = request.UserAnswer,
                        InterviewType = request.InterviewType ?? "General"
                    });

                    if (analysisResult == null)
                        return BaseResponse<InterviewAnalysisResult>.FailureResult("AI analysis failed");

                    // Do NOT persist anything for ad-hoc analysis; return the analysis directly.
                    return BaseResponse<InterviewAnalysisResult>.SuccessResult(
                        analysisResult,
                        "Ad-hoc answer analyzed successfully");
                }
                catch (Exception ex)
                {
                    return BaseResponse<InterviewAnalysisResult>.FailureResult($"Ad-hoc analysis failed: {ex.Message}");
                }
            }

            // If we reached here, we couldn't find the session/question and no ad-hoc question was provided.
            return BaseResponse<InterviewAnalysisResult>.FailureResult("Interview session or question not found. To perform ad-hoc analysis, include the 'Question' and optionally 'InterviewType' in the request.");
        }
    }

    #endregion

    #region Generate Interview Questions

    /// <summary>
    /// Generate interview questions with AI
    /// </summary>
    public class GenerateInterviewQuestionsCommand : IRequest<BaseResponse<List<string>>>
    {
        public string? Role { get; set; }
        public string? InterviewType { get; set; }
        public int QuestionCount { get; set; } = 5;
    }

    public class GenerateInterviewQuestionsCommandHandler : IRequestHandler<GenerateInterviewQuestionsCommand, BaseResponse<List<string>>>
    {
        private readonly IAIService _aiService;

        public GenerateInterviewQuestionsCommandHandler(IAIService aiService)
        {
            _aiService = aiService;
        }

        public async Task<BaseResponse<List<string>>> Handle(
            GenerateInterviewQuestionsCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                var questions = await _aiService.GenerateInterviewQuestionsAsync(
                    request.Role ?? "Software Engineer",
                    request.InterviewType ?? "Technical",
                    request.QuestionCount);

                if (questions == null || !questions.Any())
                    return BaseResponse<List<string>>.FailureResult("Failed to generate questions");

                return BaseResponse<List<string>>.SuccessResult(questions, "Questions generated successfully");
            }
            catch (Exception ex)
            {
                return BaseResponse<List<string>>.FailureResult($"Generation failed: {ex.Message}");
            }
        }
    }

    #endregion

    #region Interview Summary

    public class GenerateInterviewSummaryCommand : IRequest<BaseResponse<InterviewSummaryResult>>
    {
        public List<string> Questions { get; set; } = new();
        public List<string> Answers { get; set; } = new();
        public List<string> Feedbacks { get; set; } = new();
        public string? Role { get; set; }
        public string? InterviewType { get; set; }
    }

    public class GenerateInterviewSummaryCommandHandler : IRequestHandler<GenerateInterviewSummaryCommand, BaseResponse<InterviewSummaryResult>>
    {
        private readonly IAIService _aiService;

        public GenerateInterviewSummaryCommandHandler(IAIService aiService)
        {
            _aiService = aiService;
        }

        public async Task<BaseResponse<InterviewSummaryResult>> Handle(GenerateInterviewSummaryCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var summaryRequest = new InterviewSummaryRequest
                {
                    Questions = request.Questions,
                    Answers = request.Answers,
                    Feedbacks = request.Feedbacks,
                    Role = request.Role,
                    InterviewType = request.InterviewType
                };

                var result = await _aiService.GenerateInterviewSummaryAsync(summaryRequest);

                if (result == null)
                    return BaseResponse<InterviewSummaryResult>.FailureResult("Failed to generate interview summary");

                return BaseResponse<InterviewSummaryResult>.SuccessResult(result, "Interview summary generated successfully");
            }
            catch (Exception ex)
            {
                return BaseResponse<InterviewSummaryResult>.FailureResult($"Summary generation failed: {ex.Message}");
            }
        }
    }

    #endregion

    #region Generate Cover Letter

    /// <summary>z
    /// Generate cover letter with AI
    /// </summary>
    public class GenerateCoverLetterCommand : IRequest<BaseResponse<string>>
    {
        public int UserId { get; set; }
        public int JobPostingId { get; set; }
        public int? ResumeId { get; set; }
    }

    public class GenerateCoverLetterCommandHandler : IRequestHandler<GenerateCoverLetterCommand, BaseResponse<string>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAIService _aiService;

        public GenerateCoverLetterCommandHandler(IUnitOfWork unitOfWork, IAIService aiService)
        {
            _unitOfWork = unitOfWork;
            _aiService = aiService;
        }

        public async Task<BaseResponse<string>> Handle(
            GenerateCoverLetterCommand request,
            CancellationToken cancellationToken)
        {
            // Get job
            var job = await _unitOfWork.Repository<JobPosting>().GetByIdAsync(request.JobPostingId);
            if (job == null)
                return BaseResponse<string>.FailureResult("Job not found");

            // Get resume
            Resume? resume;
            if (request.ResumeId.HasValue)
            {
                resume = await _unitOfWork.Repository<Resume>()
                    .FirstOrDefaultAsync(r => r.Id == request.ResumeId.Value && r.UserId == request.UserId);
            }
            else
            {
                resume = await _unitOfWork.Repository<Resume>()
                    .FirstOrDefaultAsync(r => r.UserId == request.UserId && r.IsPrimary);
            }

            if (resume == null)
                return BaseResponse<string>.FailureResult("Resume not found");

            try
            {
                var coverLetter = await _aiService.GenerateCoverLetterAsync(
                    job.Description ?? "No description",
                    resume.Content ?? "Resume content",
                    job.Company ?? "Company");

                if (string.IsNullOrEmpty(coverLetter))
                    return BaseResponse<string>.FailureResult("Failed to generate cover letter");

                return BaseResponse<string>.SuccessResult(coverLetter, "Cover letter generated successfully");
            }
            catch (Exception ex)
            {
                return BaseResponse<string>.FailureResult($"Generation failed: {ex.Message}");
            }
        }
    }

    #endregion

    #region CV Builder Features

    /// <summary>
    /// Generate a new CV from user information
    /// </summary>
    public class GenerateCVCommand : IRequest<BaseResponse<GenerateCVResult>>
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string CurrentRole { get; set; } = string.Empty;
        public int ExperienceYears { get; set; }
        public string Summary { get; set; } = string.Empty;
        public List<string> Skills { get; set; } = new();
        public string CVTemplate { get; set; } = "professional";
    }

    public class GenerateCVCommandHandler : IRequestHandler<GenerateCVCommand, BaseResponse<GenerateCVResult>>
    {
        private readonly IAIService _aiService;

        public GenerateCVCommandHandler(IAIService aiService)
        {
            _aiService = aiService;
        }

        public async Task<BaseResponse<GenerateCVResult>> Handle(
            GenerateCVCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                var cvRequest = new GenerateCVRequest
                {
                    FullName = request.FullName,
                    CurrentRole = request.CurrentRole,
                    ExperienceYears = request.ExperienceYears,
                    Summary = request.Summary,
                    Skills = request.Skills,
                    CVTemplate = request.CVTemplate
                };

                var result = await _aiService.GenerateCVAsync(cvRequest);

                if (!result.Success)
                    return BaseResponse<GenerateCVResult>.FailureResult("CV generation failed");

                return BaseResponse<GenerateCVResult>.SuccessResult(result, result.Message);
            }
            catch (Exception ex)
            {
                return BaseResponse<GenerateCVResult>.FailureResult($"CV generation error: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Improve an existing CV
    /// </summary>
    public class ImproveCVCommand : IRequest<BaseResponse<ImproveCVResult>>
    {
        public int UserId { get; set; }
        public int ResumeId { get; set; }
        public string? TargetRole { get; set; }
        public string ImprovementArea { get; set; } = "overall";
    }

    public class ImproveCVCommandHandler : IRequestHandler<ImproveCVCommand, BaseResponse<ImproveCVResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAIService _aiService;

        public ImproveCVCommandHandler(IUnitOfWork unitOfWork, IAIService aiService)
        {
            _unitOfWork = unitOfWork;
            _aiService = aiService;
        }

        public async Task<BaseResponse<ImproveCVResult>> Handle(
            ImproveCVCommand request,
            CancellationToken cancellationToken)
        {
            var resume = await _unitOfWork.Repository<Resume>()
                .FirstOrDefaultAsync(r => r.Id == request.ResumeId && r.UserId == request.UserId);

            if (resume == null)
                return BaseResponse<ImproveCVResult>.FailureResult("Resume not found");

            try
            {
                var improveRequest = new ImproveCVRequest
                {
                    CurrentCVText = resume.Content ?? "Resume content not available",
                    TargetRole = request.TargetRole,
                    ImprovementArea = request.ImprovementArea
                };

                var result = await _aiService.ImproveCVAsync(improveRequest);
                return BaseResponse<ImproveCVResult>.SuccessResult(result, "CV improvement suggestions generated successfully");
            }
            catch (Exception ex)
            {
                return BaseResponse<ImproveCVResult>.FailureResult($"CV improvement error: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Parse CV content and extract information
    /// </summary>
    public class ParseCVCommand : IRequest<BaseResponse<ParseCVResult>>
    {
        public int UserId { get; set; }
        public int ResumeId { get; set; }
    }

    public class ParseCVCommandHandler : IRequestHandler<ParseCVCommand, BaseResponse<ParseCVResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAIService _aiService;

        public ParseCVCommandHandler(IUnitOfWork unitOfWork, IAIService aiService)
        {
            _unitOfWork = unitOfWork;
            _aiService = aiService;
        }

        public async Task<BaseResponse<ParseCVResult>> Handle(
            ParseCVCommand request,
            CancellationToken cancellationToken)
        {
            var resume = await _unitOfWork.Repository<Resume>()
                .FirstOrDefaultAsync(r => r.Id == request.ResumeId && r.UserId == request.UserId);

            if (resume == null)
                return BaseResponse<ParseCVResult>.FailureResult("Resume not found");

            try
            {
                var parseRequest = new ParseCVRequest
                {
                    CVContent = resume.Content ?? "Resume content not available"
                };

                var result = await _aiService.ParseCVAsync(parseRequest);
                return BaseResponse<ParseCVResult>.SuccessResult(result, "CV parsed successfully");
            }
            catch (Exception ex)
            {
                return BaseResponse<ParseCVResult>.FailureResult($"CV parsing error: {ex.Message}");
            }
        }
    }

    #endregion

    #region Job Description Parser Features

    /// <summary>
    /// Parse job description and extract information
    /// </summary>
    public class ParseJobDescriptionCommand : IRequest<BaseResponse<ParseJobDescriptionResult>>
    {
        public string JobDescription { get; set; } = string.Empty;
        public string? JobTitle { get; set; }
        public string? CompanyName { get; set; }
    }

    public class ParseJobDescriptionCommandHandler : IRequestHandler<ParseJobDescriptionCommand, BaseResponse<ParseJobDescriptionResult>>
    {
        private readonly IAIService _aiService;

        public ParseJobDescriptionCommandHandler(IAIService aiService)
        {
            _aiService = aiService;
        }

        public async Task<BaseResponse<ParseJobDescriptionResult>> Handle(
            ParseJobDescriptionCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                var parseRequest = new ParseJobDescriptionRequest
                {
                    JobDescription = request.JobDescription,
                    JobTitle = request.JobTitle,
                    CompanyName = request.CompanyName
                };

                var result = await _aiService.ParseJobDescriptionAsync(parseRequest);
                return BaseResponse<ParseJobDescriptionResult>.SuccessResult(result, "Job description parsed successfully");
            }
            catch (Exception ex)
            {
                return BaseResponse<ParseJobDescriptionResult>.FailureResult($"Job parsing error: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Extract skills from job description
    /// </summary>
    public class ExtractJobSkillsCommand : IRequest<BaseResponse<ExtractJobSkillsResult>>
    {
        public string JobDescription { get; set; } = string.Empty;
    }

    public class ExtractJobSkillsCommandHandler : IRequestHandler<ExtractJobSkillsCommand, BaseResponse<ExtractJobSkillsResult>>
    {
        private readonly IAIService _aiService;

        public ExtractJobSkillsCommandHandler(IAIService aiService)
        {
            _aiService = aiService;
        }

        public async Task<BaseResponse<ExtractJobSkillsResult>> Handle(
            ExtractJobSkillsCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                var extractRequest = new ExtractJobSkillsRequest
                {
                    JobDescription = request.JobDescription
                };

                var result = await _aiService.ExtractJobSkillsAsync(extractRequest);
                return BaseResponse<ExtractJobSkillsResult>.SuccessResult(result, "Skills extracted successfully");
            }
            catch (Exception ex)
            {
                return BaseResponse<ExtractJobSkillsResult>.FailureResult($"Skill extraction error: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Match candidate skills to job requirements
    /// </summary>
    public class MatchSkillsToJobCommand : IRequest<BaseResponse<MatchSkillsToJobResult>>
    {
        public List<string> CandidateSkills { get; set; } = new();
        public string JobDescription { get; set; } = string.Empty;
    }

    public class MatchSkillsToJobCommandHandler : IRequestHandler<MatchSkillsToJobCommand, BaseResponse<MatchSkillsToJobResult>>
    {
        private readonly IAIService _aiService;

        public MatchSkillsToJobCommandHandler(IAIService aiService)
        {
            _aiService = aiService;
        }

        public async Task<BaseResponse<MatchSkillsToJobResult>> Handle(
            MatchSkillsToJobCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                var matchRequest = new MatchSkillsToJobRequest
                {
                    CandidateSkills = request.CandidateSkills,
                    JobDescription = request.JobDescription
                };

                var result = await _aiService.MatchSkillsToJobAsync(matchRequest);
                return BaseResponse<MatchSkillsToJobResult>.SuccessResult(result, "Skills matched successfully");
            }
            catch (Exception ex)
            {
                return BaseResponse<MatchSkillsToJobResult>.FailureResult($"Skill matching error: {ex.Message}");
            }
        }
    }

    #endregion
}
