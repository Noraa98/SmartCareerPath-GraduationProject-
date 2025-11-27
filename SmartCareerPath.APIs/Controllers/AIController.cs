using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCareerPath.Application.Abstraction.DTOs.AI;
using SmartCareerPath.Application.Features.AI.Commands;
using SmartCareerPath.Application.Features.AI.Queries;
using SmartCareerPath.Application.Abstraction.ServicesContracts.AI;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SmartCareerPath.APIs.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class AIController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IAIService _aiService;

        public AIController(IMediator mediator, IAIService aiService)
        {
            _mediator = mediator;
            _aiService = aiService;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out int userId) ? userId : 0;
        }

        #region Resume AI Features

        /// <summary>
        /// Analyze resume with AI
        /// </summary>
        /// <param name="resumeId">The ID of the resume to analyze</param>
        /// <param name="request">Analysis request with target role</param>
        [HttpPost("analyze-resume/{resumeId}")]
        public async Task<IActionResult> AnalyzeResume(int resumeId, [FromBody] AnalyzeResumeRequest request)
        {
            var command = new AnalyzeResumeWithAICommand
            {
                UserId = GetCurrentUserId(),
                ResumeId = resumeId,
                TargetRole = request?.TargetRole
            };

            var result = await _mediator.Send(command);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Extract skills from resume text
        /// </summary>
        [HttpPost("extract-skills")]
        public async Task<IActionResult> ExtractSkills([FromBody] ExtractSkillsRequest request)
        {
            try
            {
                var skills = await _aiService.ExtractSkillsFromResumeAsync(request.ResumeText);

                return Ok(new
                {
                    success = true,
                    skills = skills,
                    message = "Skills extracted successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Get resume improvement suggestions
        /// </summary>
        [HttpPost("resume-suggestions")]
        public async Task<IActionResult> GetResumeSuggestions([FromBody] ResumeSuggestionsRequest request)
        {
            try
            {
                var suggestions = await _aiService.SuggestResumeImprovementsAsync(request.ResumeText);

                return Ok(new
                {
                    success = true,
                    suggestions = suggestions,
                    message = "Suggestions generated successfully"
                });
            }
            catch (Exception ex)
            { 
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        #endregion

        #region Job Matching Features

        /// <summary>
        /// Calculate job match score
        /// </summary>
        [HttpPost("job-match/{jobId}")]
        public async Task<IActionResult> CalculateJobMatch(int jobId, [FromBody] JobMatchCalculateRequest request)
        {
            var command = new CalculateJobMatchCommand
            {
                UserId = GetCurrentUserId(),
                JobPostingId = jobId,
                ResumeId = request?.ResumeId
            };

            var result = await _mediator.Send(command);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Generate cover letter for job
        /// </summary>
        [HttpPost("generate-cover-letter/{jobId}")]
        public async Task<IActionResult> GenerateCoverLetter(int jobId, [FromBody] GenerateCoverLetterRequest request)
        {
            var command = new GenerateCoverLetterCommand
            {
                UserId = GetCurrentUserId(),
                JobPostingId = jobId,
                ResumeId = request?.ResumeId
            };

            var result = await _mediator.Send(command);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        #endregion

        #region Interview Features

        /// <summary>
        /// Generate interview questions
        /// </summary>
        [HttpPost("generate-interview-questions")]
        public async Task<IActionResult> GenerateInterviewQuestions([FromBody] GenerateInterviewQuestionsCommand command)
        {
            var result = await _mediator.Send(command);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Summarize a completed interview: questions, answers and feedback
        /// </summary>
        [HttpPost("summarize-interview")]
        public async Task<IActionResult> SummarizeInterview([FromBody] GenerateInterviewSummaryCommand command)
        {
            var result = await _mediator.Send(command);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Analyze interview answer
        /// </summary>
        [HttpPost("analyze-interview-answer")]
        public async Task<IActionResult> AnalyzeInterviewAnswer([FromBody] AnalyzeInterviewAnswerCommand command)
        {
            var result = await _mediator.Send(command);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        #endregion

        #region Career Path Features

        /// <summary>
        /// Get AI career path recommendations
        /// </summary>
        [HttpGet("career-recommendations")]
        public async Task<IActionResult> GetCareerRecommendations([FromQuery] string desiredField = null)
        {
            var query = new GetCareerPathRecommendationsQuery
            {
                UserId = GetCurrentUserId(),
                DesiredField = desiredField
            };

            var result = await _mediator.Send(query);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Identify skill gaps for target role
        /// </summary>
        [HttpPost("identify-skill-gaps")]
        public async Task<IActionResult> IdentifySkillGaps([FromBody] SkillGapRequest request)
        {
            try
            {
                var skillGaps = await _aiService.IdentifySkillGapsAsync(
                    request.CurrentSkills,
                    request.TargetRole);

                return Ok(new
                {
                    success = true,
                    skillGaps = skillGaps,
                    message = "Skill gaps identified successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Recommend a career path based on quiz/interests/quiz answers
        /// </summary>
        [HttpPost("recommend-career-path-from-quiz")]
        public async Task<IActionResult> RecommendCareerPathFromQuiz([FromBody] Models.RecommendCareerPathFromQuizRequest request)
        {
            var command = new SmartCareerPath.Application.Commands.AI.RecommendCareerPathFromQuizCommand
            {
                Interests = request.Interests,
                Answers = request.Answers
            };
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        #endregion

        #region General AI Features

        /// <summary>
        /// Send custom prompt to AI (for testing - Admin only)
        /// </summary>
        [HttpPost("prompt")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SendPrompt([FromBody] CustomPromptRequest request)
        {
            try
            {
                var aiRequest = new AIPromptRequest
                {
                    Prompt = request.Prompt,
                    SystemPrompt = request.SystemPrompt,
                    MaxTokens = request.MaxTokens,
                    Temperature = request.Temperature
                };

                var response = await _aiService.SendPromptAsync(aiRequest);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        #endregion

        #region CV Builder Features

        /// <summary>
        /// Generate a new CV from user information
        /// </summary>
        [HttpPost("generate-cv")]
        public async Task<IActionResult> GenerateCV([FromBody] GenerateCVCommand command)
        {
            try
            {
                command.UserId = GetCurrentUserId();
                var result = await _mediator.Send(command);

                if (!result.Success)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Improve an existing CV with AI suggestions
        /// </summary>
        [HttpPost("improve-cv/{resumeId}")]
        public async Task<IActionResult> ImproveCV(int resumeId, [FromBody] ImproveCVCommand command)
        {
            try
            {
                command.UserId = GetCurrentUserId();
                command.ResumeId = resumeId;
                var result = await _mediator.Send(command);

                if (!result.Success)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Parse CV and extract structured information
        /// </summary>
        [HttpPost("parse-cv/{resumeId}")]
        public async Task<IActionResult> ParseCV(int resumeId)
        {
            try
            {
                var command = new ParseCVCommand
                {
                    UserId = GetCurrentUserId(),
                    ResumeId = resumeId
                };

                var result = await _mediator.Send(command);

                if (!result.Success)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        #endregion

        #region Job Description Parser Features

        /// <summary>
        /// Parse job description and extract structured information
        /// </summary>
        [HttpPost("parse-job-description")]
        public async Task<IActionResult> ParseJobDescription([FromBody] ParseJobDescriptionCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);

                if (!result.Success)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Extract required and preferred skills from job description
        /// </summary>
        [HttpPost("extract-job-skills")]
        public async Task<IActionResult> ExtractJobSkills([FromBody] ExtractJobSkillsCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);

                if (!result.Success)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Match candidate skills to job requirements
        /// </summary>
        [HttpPost("match-skills-to-job")]
        public async Task<IActionResult> MatchSkillsToJob([FromBody] MatchSkillsToJobCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);

                if (!result.Success)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        #endregion

        #region Quiz Features

        /// <summary>
        /// Get available interests for the quiz
        /// </summary>
        [HttpGet("interests")]
        public IActionResult GetAvailableInterests()
        {
            var interests = new List<string>
            {
                "Software Development",
                "Data Science",
                "Artificial Intelligence",
                "Cloud Computing",
                "Cybersecurity",
                "UI/UX Design",
                "Graphic Design",
                "Product Management",
                "Business Analysis",
                "Marketing",
                "Sales",
                "Finance",
                "Accounting",
                "Human Resources",
                "Project Management",
                "Networking",
                "DevOps",
                "Mobile Development",
                "Game Development",
                "Robotics",
                "Healthcare Technology",
                "Education Technology",
                "Environmental Science",
                "Mechanical Engineering",
                "Electrical Engineering",
                "Civil Engineering",
                "Content Writing",
                "Social Media",
                "Law/Legal Tech",
                "Entrepreneurship"
            };
            return Ok(interests);
        }

        /// <summary>
        /// Generate MCQ quiz questions and choices for selected interests
        /// </summary>
        [HttpPost("generate-quiz-questions")]
        public async Task<IActionResult> GenerateQuizQuestions([FromBody] SmartCareerPath.Application.Abstraction.DTOs.AI.GenerateQuizQuestionsRequest request)
        {
            var questions = await _aiService.GenerateQuizMCQQuestionsAsync(request.Interests, request.QuestionCount);
            return Ok(questions);
        }

        #endregion
    }
}
