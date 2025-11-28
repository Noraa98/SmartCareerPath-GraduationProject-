

using SmartCareerPath.Application.Abstraction.DTOs.AI;
using SmartCareerPath.Application.Abstraction.ServicesContracts.AI;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;



namespace SmartCareerPath.Infrastructure.Persistence.Services.AI
{
    public class AIService : IAIService
    {
        private readonly IConfiguration? _config;

        public AIService(IConfiguration? config = null)
        {
            _config = config;
        }

        private const string OpenRouterUrl = "https://openrouter.ai/api/v1/chat/completions";

        public async Task<QuizCareerRecommendationResult> RecommendCareerPathFromQuizAsync(QuizCareerRecommendationRequest request)
        {
            // Build system prompt for OpenRouter
            var system = "You are a career advisor AI. Given a user's interests and their quiz answers, recommend a single best-fit career path. Respond ONLY with: { CareerPath, Description, Reasoning }. No extra text.";

            // Build user prompt
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("User Interests: " + string.Join(", ", request.Interests ?? new List<string>()));
            sb.AppendLine("Quiz Answers:");
            foreach (var ans in request.Answers ?? new List<QuizAnswerForCareerDto>())
            {
                sb.AppendLine($"Q: {ans.QuestionText}");
                sb.AppendLine($"A: {ans.UserAnswer}");
            }

            var reply = await CallOpenRouterAsync(system, sb.ToString(), 0.3, 400);
            if (!string.IsNullOrWhiteSpace(reply))
            {
                var parsed = TryDeserializeObject<QuizCareerRecommendationResult>(reply);
                if (parsed != null && !string.IsNullOrWhiteSpace(parsed.CareerPath))
                    return parsed;
            }

            // Fallback: simple heuristic based on interests/answers
            var allText = string.Join(" ", (request.Interests ?? new List<string>()).Concat((request.Answers ?? new List<QuizAnswerForCareerDto>()).Select(a => a.UserAnswer)));
            string path = "Software Engineer", desc = "Develops software solutions.", reason = "Interest in technology and problem solving.";
            if (allText.ToLower().Contains("design")) { path = "UI/UX Designer"; desc = "Designs user interfaces and experiences."; reason = "Strong interest in design and creativity."; }
            else if (allText.ToLower().Contains("data")) { path = "Data Scientist"; desc = "Analyzes and interprets complex data."; reason = "Interest in data analysis and statistics."; }
            else if (allText.ToLower().Contains("cloud")) { path = "Cloud Engineer"; desc = "Builds and manages cloud infrastructure."; reason = "Interest in cloud technologies."; }
            else if (allText.ToLower().Contains("business")) { path = "Business Analyst"; desc = "Bridges business needs and technology."; reason = "Interest in business processes and analysis."; }

            return new QuizCareerRecommendationResult
            {
                CareerPath = path,
                Description = desc,
                Reasoning = reason
            };
        }

        // Tolerant mapping from raw JSON string to ParseJobDescriptionResult
        private static ParseJobDescriptionResult? TryMapJsonToParseJobDescriptionResult(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;

            using var doc = System.Text.Json.JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (root.ValueKind != System.Text.Json.JsonValueKind.Object) return null;

            var result = new SmartCareerPath.Application.Abstraction.DTOs.AI.ParseJobDescriptionResult();

            // Helper local funcs
            static string? GetString(System.Text.Json.JsonElement el, string name)
            {
                if (el.ValueKind != System.Text.Json.JsonValueKind.Object) return null;
                foreach (var prop in el.EnumerateObject())
                {
                    if (string.Equals(prop.Name, name, StringComparison.OrdinalIgnoreCase))
                        return prop.Value.ValueKind == System.Text.Json.JsonValueKind.String ? prop.Value.GetString() : prop.Value.ToString();
                }
                return null;
            }

            static List<string> GetStringList(System.Text.Json.JsonElement el, string name)
            {
                var list = new List<string>();
                if (el.ValueKind != System.Text.Json.JsonValueKind.Object) return list;
                foreach (var prop in el.EnumerateObject())
                {
                    if (string.Equals(prop.Name, name, StringComparison.OrdinalIgnoreCase))
                    {
                        var v = prop.Value;
                        if (v.ValueKind == System.Text.Json.JsonValueKind.Array)
                        {
                            foreach (var item in v.EnumerateArray())
                            {
                                if (item.ValueKind == System.Text.Json.JsonValueKind.String)
                                    list.Add(item.GetString()!);
                                else
                                    list.Add(item.ToString());
                            }
                        }
                        else if (v.ValueKind == System.Text.Json.JsonValueKind.String)
                        {
                            list.Add(v.GetString()!);
                        }
                        break;
                    }
                }
                return list;
            }

            // Map BasicInfo
            var basic = new SmartCareerPath.Application.Abstraction.DTOs.AI.JobBasicInfo
            {
                JobTitle = GetString(root, "BasicInfo") is null ? GetString(root, "JobTitle") ?? string.Empty : string.Empty,
                CompanyName = string.Empty,
                Location = string.Empty,
                IsRemote = false,
                SalaryRangeMin = string.Empty,
                SalaryRangeMax = string.Empty,
                SalaryCurrency = string.Empty,
                SalaryPeriod = string.Empty,
                ExperienceYearsRequired = 0,
                Department = string.Empty,
                ReportingLine = new List<string>()
            };

            // If there's a BasicInfo object, read from it
            if (root.TryGetProperty("BasicInfo", out var basicEl) && basicEl.ValueKind == System.Text.Json.JsonValueKind.Object)
            {
                basic.JobTitle = GetString(basicEl, "JobTitle") ?? GetString(root, "JobTitle") ?? basic.JobTitle;
                basic.CompanyName = GetString(basicEl, "CompanyName") ?? GetString(root, "CompanyName") ?? string.Empty;
                basic.Location = GetString(basicEl, "Location") ?? string.Empty;
                var isRemoteStr = GetString(basicEl, "IsRemote");
                if (!string.IsNullOrWhiteSpace(isRemoteStr) && bool.TryParse(isRemoteStr, out var b)) basic.IsRemote = b;
                basic.SalaryRangeMin = GetString(basicEl, "SalaryRangeMin") ?? GetString(basicEl, "SalaryMin") ?? string.Empty;
                basic.SalaryRangeMax = GetString(basicEl, "SalaryRangeMax") ?? GetString(basicEl, "SalaryMax") ?? string.Empty;
                basic.SalaryCurrency = GetString(basicEl, "SalaryCurrency") ?? string.Empty;
                var expYears = GetString(basicEl, "ExperienceYearsRequired");
                if (!string.IsNullOrWhiteSpace(expYears) && int.TryParse(expYears, out var ey)) basic.ExperienceYearsRequired = ey;
                basic.Department = GetString(basicEl, "Department") ?? string.Empty;
                // ReportingLine array
                if (basicEl.TryGetProperty("ReportingLine", out var rl) && rl.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    foreach (var item in rl.EnumerateArray()) if (item.ValueKind == System.Text.Json.JsonValueKind.String) basic.ReportingLine.Add(item.GetString()!);
                }
            }
            else
            {
                // Top-level fields fallback
                basic.JobTitle = GetString(root, "JobTitle") ?? basic.JobTitle;
                basic.CompanyName = GetString(root, "CompanyName") ?? string.Empty;
            }

            result.BasicInfo = basic;

            result.RequiredSkills = GetStringList(root, "RequiredSkills");
            result.PreferredSkills = GetStringList(root, "PreferredSkills");
            result.Responsibilities = GetStringList(root, "Responsibilities");
            result.Requirements = GetStringList(root, "Requirements");

            // Benefits may be an object or array
            var benefits = new SmartCareerPath.Application.Abstraction.DTOs.AI.JobBenefits
            {
                Benefits = GetStringList(root, "Benefits"),
                HealthInsurance = GetString(root, "HealthInsurance") ?? string.Empty,
                RetirementPlan = GetString(root, "RetirementPlan") ?? string.Empty,
                PaidTimeOff = 0,
                ProfessionalDevelopment = GetString(root, "ProfessionalDevelopment") ?? string.Empty,
                RemoteWorkOptions = false,
                FlexibleSchedule = false
            };
            // If Benefits is an object with nested fields
            if (root.TryGetProperty("Benefits", out var benefitsEl))
            {
                if (benefitsEl.ValueKind == System.Text.Json.JsonValueKind.Object)
                {
                    benefits.Benefits = GetStringList(root, "Benefits") ;
                    benefits.HealthInsurance = GetString(benefitsEl, "HealthInsurance") ?? benefits.HealthInsurance;
                    benefits.RetirementPlan = GetString(benefitsEl, "RetirementPlan") ?? benefits.RetirementPlan;
                    var pto = GetString(benefitsEl, "PaidTimeOff");
                    if (!string.IsNullOrWhiteSpace(pto) && int.TryParse(pto, out var ptoi)) benefits.PaidTimeOff = ptoi;
                    benefits.ProfessionalDevelopment = GetString(benefitsEl, "ProfessionalDevelopment") ?? benefits.ProfessionalDevelopment;
                    var remote = GetString(benefitsEl, "RemoteWorkOptions"); if (!string.IsNullOrWhiteSpace(remote) && bool.TryParse(remote, out var r)) benefits.RemoteWorkOptions = r;
                    var flex = GetString(benefitsEl, "FlexibleSchedule"); if (!string.IsNullOrWhiteSpace(flex) && bool.TryParse(flex, out var f)) benefits.FlexibleSchedule = f;
                }
                else if (benefitsEl.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    var arr = new List<string>();
                    foreach (var it in benefitsEl.EnumerateArray()) arr.Add(it.ToString());
                    benefits.Benefits = arr;
                }
            }

            result.Benefits = benefits;

            result.KeyTechnologies = GetStringList(root, "KeyTechnologies");
            result.SeniorityLevel = GetString(root, "SeniorityLevel") ?? string.Empty;
            result.EmploymentType = GetString(root, "EmploymentType") ?? string.Empty;
            result.ParseQuality = GetString(root, "ParseQuality") ?? string.Empty;

            // Parse confidence: accept 0-1 or 0-100
            var confStr = GetString(root, "ParseConfidenceScore");
            if (decimal.TryParse(confStr, out var conf))
            {
                if (conf <= 1) conf *= 100;
                result.ParseConfidenceScore = conf;
            }
            else
            {
                // try numeric property
                if (root.TryGetProperty("ParseConfidenceScore", out var confEl))
                {
                    if (confEl.ValueKind == System.Text.Json.JsonValueKind.Number && confEl.TryGetDecimal(out var v))
                    {
                        if (v <= 1) v *= 100;
                        result.ParseConfidenceScore = v;
                    }
                }
            }

            // Ensure non-null lists
            result.RequiredSkills ??= new List<string>();
            result.PreferredSkills ??= new List<string>();
            result.Responsibilities ??= new List<string>();
            result.Requirements ??= new List<string>();
            result.KeyTechnologies ??= new List<string>();

            return result;
        }

        // Generic helper to call OpenRouter and return the model text (or null on failure)
        private async Task<string?> CallOpenRouterAsync(string systemPrompt, string userPrompt, double temperature = 0.7, int maxTokens = 512)
        {
            var apiKey = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY");
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                Console.WriteLine("[AI DEBUG] OpenRouter API key loaded from environment (OPENROUTER_API_KEY) for GenerateInterviewQuestions.");
            }

            if (string.IsNullOrWhiteSpace(apiKey) && _config != null)
            {
                apiKey = _config["OpenRouter:ApiKey"];
                if (!string.IsNullOrWhiteSpace(apiKey))
                {
                    Console.WriteLine("[AI DEBUG] OpenRouter API key loaded from configuration (OpenRouter:ApiKey).");
                }
            }

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                System.Diagnostics.Debug.WriteLine("OpenRouter API key not found in environment (OPENROUTER_API_KEY) or configuration (OpenRouter:ApiKey)");
                Console.WriteLine("[AI DEBUG] OpenRouter API key not found in environment (OPENROUTER_API_KEY) or configuration (OpenRouter:ApiKey). The model will not be called.");
                return null;
            }

            try
            {
                using var http = new System.Net.Http.HttpClient();
                http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

                var payload = new
                {
                    model = "mistralai/mistral-7b-instruct",
                    messages = new[] {
                        new { role = "system", content = systemPrompt },
                        new { role = "user", content = userPrompt }
                    },
                    temperature = temperature,
                    max_tokens = maxTokens
                };

                var json = System.Text.Json.JsonSerializer.Serialize(payload);

                int maxAttempts = 3;
                for (int attempt = 1; attempt <= maxAttempts; attempt++)
                {
                    using var content = new System.Net.Http.StringContent(json, System.Text.Encoding.UTF8, "application/json");

                    var resp = await http.PostAsync(OpenRouterUrl, content);
                    var respBody = await resp.Content.ReadAsStringAsync();
                    if (!resp.IsSuccessStatusCode)
                    {
                        System.Diagnostics.Debug.WriteLine($"OpenRouter request failed: {resp.StatusCode} {respBody}");
                        Console.WriteLine($"[AI DEBUG] OpenRouter request failed: {resp.StatusCode} {respBody}");
                        if ((int)resp.StatusCode >= 400 && (int)resp.StatusCode < 500) return null;
                        if (attempt < maxAttempts) await Task.Delay(300 * attempt);
                        continue;
                    }

                    try
                    {
                        using var doc = System.Text.Json.JsonDocument.Parse(respBody);
                        if (doc.RootElement.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
                        {
                            var first = choices[0];
                            string? s = null;
                            if (first.TryGetProperty("message", out var message) && message.TryGetProperty("content", out var contentElem))
                                s = contentElem.GetString();
                            else if (first.TryGetProperty("content", out var contentAlt))
                                s = contentAlt.GetString();

                            if (!string.IsNullOrWhiteSpace(s))
                            {
                                Console.WriteLine($"[AI DEBUG] OpenRouter returned message.content (preview): {s?.Substring(0, Math.Min(800, s.Length))}");
                                return s;
                            }
                            else
                            {
                                Console.WriteLine($"[AI DEBUG] OpenRouter returned empty content on attempt {attempt}.");
                                if (attempt < maxAttempts) await Task.Delay(300 * attempt);
                                continue;
                            }
                        }

                        Console.WriteLine($"[AI DEBUG] OpenRouter returned raw body (preview): {respBody?.Substring(0, Math.Min(800, respBody.Length))}");
                        if (!string.IsNullOrWhiteSpace(respBody)) return respBody;
                        if (attempt < maxAttempts) await Task.Delay(300 * attempt);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"OpenRouter parse failed: {ex.Message}");
                        Console.WriteLine($"[AI DEBUG] OpenRouter parse failed: {ex.Message}");
                        if (attempt < maxAttempts) await Task.Delay(300 * attempt);
                    }
                }

                Console.WriteLine("[AI DEBUG] OpenRouter: all retry attempts returned empty responses.");
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"OpenRouter call error: {ex.Message}");
                Console.WriteLine($"[AI DEBUG] OpenRouter call error: {ex.Message}");
                return null;
            }
        }

        // Try to deserialize JSON object embedded in text to a specific type
        private static T? TryDeserializeObject<T>(string text)
        {
            text = text ?? string.Empty;
            try
            {
                text = text.Trim();

                // If response includes triple-backtick fencing (``` or ```json), extract the inner block
                if (text.Contains("```"))
                {
                    var firstFence = text.IndexOf("```", StringComparison.Ordinal);
                    var lastFence = text.LastIndexOf("```", StringComparison.Ordinal);
                    if (lastFence > firstFence)
                    {
                        var inner = text.Substring(firstFence + 3, lastFence - firstFence - 3).Trim();
                        if (inner.StartsWith("json", StringComparison.OrdinalIgnoreCase))
                            inner = inner.Substring(4).Trim();
                        text = inner;
                    }
                }

                // If it starts with { or [, try direct deserialize
                if (text.StartsWith("{") || text.StartsWith("["))
                {
                    var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    return System.Text.Json.JsonSerializer.Deserialize<T>(text, options);
                }

                // Try to locate the first JSON object or array and extract a balanced substring
                int startObj = text.IndexOf('{');
                int startArr = text.IndexOf('[');
                int start = -1;
                char openChar = '\0';
                char closeChar = '\0';

                if (startObj >= 0 && (startObj < startArr || startArr == -1))
                {
                    start = startObj; openChar = '{'; closeChar = '}';
                }
                else if (startArr >= 0)
                {
                    start = startArr; openChar = '['; closeChar = ']';
                }

                if (start >= 0)
                {
                    int depth = 0;
                    for (int i = start; i < text.Length; i++)
                    {
                        if (text[i] == openChar) depth++;
                        else if (text[i] == closeChar)
                        {
                            depth--;
                            if (depth == 0)
                            {
                                var sub = text.Substring(start, i - start + 1);
                                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                                return System.Text.Json.JsonSerializer.Deserialize<T>(sub, options);
                            }
                        }
                    }
                }
            }
            catch { }

            return default;
        }
        /// <summary>
        /// Analyze resume with AI
        /// </summary>
        public async Task<ResumeAnalysisResult> AnalyzeResumeAsync(ResumeAnalysisRequest request)
        {
            // Ensure request and its properties are not null
            var resumeText = request?.ResumeText ?? string.Empty;
            var targetRole = request?.TargetRole ?? string.Empty;

            var system = "You are a resume analysis assistant. Given resume text and optional target role, return a strict JSON object matching the ResumeAnalysisResult schema: { Scores: { ATSScore, ReadabilityScore, SkillsRelevanceScore, OverallScore }, Summary, Suggestions: [ { Category, Suggestion, Priority } ], ExtractedSkills: [string] }. Return ONLY the JSON object.";
            var user = $"ResumeText:\n{resumeText}\n\nTargetRole: {targetRole}";

            var reply = await CallOpenRouterAsync(system, user, 0.7, 800);
            if (!string.IsNullOrWhiteSpace(reply))
            {
                var parsed = TryDeserializeObject<ResumeAnalysisResult>(reply);
                if (parsed != null)
                    return parsed;
            }

            // Fallback to mock
            await Task.Delay(500);
            var skills = ExtractSkillsFromText(resumeText);

            return new ResumeAnalysisResult
            {
                Scores = new ResumeScores
                {
                    ATSScore = 78.5m,
                    ReadabilityScore = 85.0m,
                    SkillsRelevanceScore = 82.0m,
                    OverallScore = 81.8m
                },
                Summary = "Strong resume with good technical experience and clear communication.",
                Suggestions = new List<ResumeSuggestion>
                {
                    new ResumeSuggestion { Category = "Format", Suggestion = "Add more metrics to achievements", Priority = "High" },
                    new ResumeSuggestion { Category = "Content", Suggestion = "Include certifications section", Priority = "Medium" },
                    new ResumeSuggestion { Category = "Keywords", Suggestion = "Add more industry keywords", Priority = "Medium" }
                },
                ExtractedSkills = skills
            };
        }

        /// <summary>
        /// Extract skills from resume text
        /// </summary>
        public async Task<List<string>> ExtractSkillsFromResumeAsync(string resumeText)
        {
            // Try OpenRouter to extract skills as JSON array
            // same, use .liquid files
            var system = "You are an assistant that extracts a list of skill keywords from resume text. Return a JSON array of skill strings only.";
            var user = resumeText ?? string.Empty;
            var reply = await CallOpenRouterAsync(system, user, 0.0, 400);
            if (!string.IsNullOrWhiteSpace(reply))
            {
                var arr = TryExtractJsonArray(reply);
                if (arr != null && arr.Any())
                    return arr;
            }

            await Task.Delay(300);
            return ExtractSkillsFromText(resumeText ?? string.Empty);
        }

        /// <summary>
        /// Suggest resume improvements
        /// </summary>
        public async Task<List<ResumeSuggestion>> SuggestResumeImprovementsAsync(string resumeText)
        {
            var system = "You are a resume improvement assistant. Given a resume text, return a JSON array of ResumeSuggestion objects: { Category, Suggestion, Priority }. Return ONLY the JSON array.";
            var user = resumeText ?? string.Empty;
            var reply = await CallOpenRouterAsync(system, user, 0.7, 600);
            if (!string.IsNullOrWhiteSpace(reply))
            {
                var parsed = TryDeserializeObject<List<ResumeSuggestion>>(reply);
                if (parsed != null)
                    return parsed;
            }

            await Task.Delay(500);

            return new List<ResumeSuggestion>
            {
                new ResumeSuggestion
                {
                    Category = "Structure",
                    Suggestion = "Add a professional summary section at the top",
                    Priority = "High"
                },
                new ResumeSuggestion
                {
                    Category = "Content",
                    Suggestion = "Quantify your achievements with metrics (e.g., 'Improved performance by 40%')",
                    Priority = "High"
                },
                new ResumeSuggestion
                {
                    Category = "Format",
                    Suggestion = "Use consistent date formatting",
                    Priority = "Medium"
                },
                new ResumeSuggestion
                {
                    Category = "Keywords",
                    Suggestion = "Add more industry-specific keywords for ATS optimization",
                    Priority = "Medium"
                },
                new ResumeSuggestion
                {
                    Category = "Skills",
                    Suggestion = "Create a dedicated skills section",
                    Priority = "Low"
                }
            };
        }

        /// <summary>
        /// Match resume to job posting
        /// </summary>
        public async Task<JobMatchResult> MatchResumeToJobAsync(JobMatchRequest request)
        {
            var system = "You are a job matching assistant. Given a resume text and job description, return a JSON object matching JobMatchResult: { MatchPercentage, MatchedSkills, MissingSkills, Recommendations }. Return ONLY the JSON object.";
            var user = $"ResumeText:\n{request?.ResumeText}\n\nJobDescription:\n{request?.JobDescription}";
            var reply = await CallOpenRouterAsync(system, user, 0.7, 800);
            if (!string.IsNullOrWhiteSpace(reply))
            {
                var parsed = TryDeserializeObject<JobMatchResult>(reply);
                if (parsed != null)
                    return parsed;
            }

            await Task.Delay(600);
            var matchedSkills = new List<string> { "C#", "ASP.NET Core", "SQL Server", "Git" };
            var missingSkills = new List<string> { "Kubernetes", "Docker", "Microservices Architecture" };

            return new JobMatchResult
            {
                MatchPercentage = 75.5m,
                MatchedSkills = matchedSkills,
                MissingSkills = missingSkills,
                Recommendations = new List<string>
                {
                    "Your experience aligns well with the core requirements",
                    "Consider gaining experience with containerization technologies",
                    "Your architecture knowledge is strong - great foundation"
                }
            };
        }

        /// <summary>
        /// Generate cover letter
        /// </summary>
        public async Task<string> GenerateCoverLetterAsync(string jobDescription, string resumeText, string companyName)
        {
            var system = "You are a cover letter generator. Given the job description, candidate resume text and company name, produce a professional cover letter tailored to the job. Return ONLY the cover letter text (no JSON).";
            var user = $"Company: {companyName}\n\nJob Description:\n{jobDescription}\n\nResume:\n{resumeText}";
            var reply = await CallOpenRouterAsync(system, user, 0.7, 800);
            if (!string.IsNullOrWhiteSpace(reply))
            {
                return reply.Trim();
            }

            await Task.Delay(800);

            return $@"Dear Hiring Manager at {companyName},

I am writing to express my strong interest in the position described in your job posting. 
With my extensive background in software development and proven track record of building scalable applications, 
I am confident that I can make significant contributions to your team.

Throughout my career, I have successfully delivered high-impact projects using cutting-edge technologies. 
My expertise in cloud architecture and DevOps practices has enabled me to design and implement robust, scalable solutions 
that meet and exceed business requirements.

I am particularly drawn to this opportunity because of your company's commitment to innovation and excellence. 
Your recent projects align perfectly with my professional interests and skills.

I would welcome the opportunity to discuss how my experience and skills can contribute to your team's success. 
Thank you for considering my application.

Sincerely,
[Candidate Name]";
        }

       
        public async Task<InterviewAnalysisResult> AnalyzeInterviewAnswerAsync(InterviewAnalysisRequest request)
        {
            var system = "You are an interview evaluator. Given a question, the user's answer, and the interview type, return a JSON object matching InterviewAnalysisResult: { OverallScore: number (0-10), Feedback: string, Strengths: [string], Improvements: [string] }. Return ONLY the JSON object.";
            var user = $"Question: {request?.Question}\n\nInterviewType: {request?.InterviewType}\n\nUserAnswer: {request?.UserAnswer}";
            var reply = await CallOpenRouterAsync(system, user, 0.3, 400);
            if (!string.IsNullOrWhiteSpace(reply))
            {
                var parsed = TryDeserializeObject<InterviewAnalysisResult>(reply);
                if (parsed != null)
                    return parsed;
            }

            await Task.Delay(200);
            
            var question = request?.Question ?? "";
            var answer = request?.UserAnswer ?? "";
            var answerLength = answer.Length;
            var questionLength = question.Length;
            var interviewType = request?.InterviewType?.ToLower() ?? "";
            
            var isTechnical = interviewType.Contains("technical");
            var isBehavioral = interviewType.Contains("behavioral") || interviewType.Contains("behavior");
            var isSystemDesign = question.ToLower().Contains("design") || question.ToLower().Contains("architecture");
            var isChallengeQuestion = question.ToLower().Contains("challenge") || question.ToLower().Contains("difficult") || question.ToLower().Contains("conflict");
            var isExperienceQuestion = question.ToLower().Contains("describe") || question.ToLower().Contains("tell") || question.ToLower().Contains("example");
            
            // Determine answer quality indicators
            var hasMetrics = answer.Contains("%") || answer.Contains("metrics") || answer.Contains("improved");
            var hasCode = answer.Contains("code") || answer.Contains("implemented") || answer.Contains("developed");
            var hasTeamwork = answer.Contains("team") || answer.Contains("collaborated") || answer.Contains("worked");
            var hasLessons = answer.Contains("learned") || answer.Contains("lesson") || answer.Contains("improve");
            
            // Hash-based variation for question-specific feedback
            var questionHash = Math.Abs(question.GetHashCode() % 5);
            var answerHash = Math.Abs(answer.GetHashCode() % 3);
            
            // Vary score based on answer length, metrics, and question type
            var baseScore = 5.5m + (Math.Min(answerLength / 60, 25) * 0.12m);
            if (hasMetrics) baseScore += 1.2m;
            if (hasCode && isTechnical) baseScore += 0.8m;
            if (hasTeamwork && isBehavioral) baseScore += 0.7m;
            if (hasLessons) baseScore += 0.6m;
            
            // Slightly adjust based on question/answer hash for variety
            baseScore += (questionHash * 0.05m) + (answerHash * 0.03m);
            var score = Math.Min(Math.Round(baseScore, 1), 10.0m);
            
            // Generate question-specific feedback
            var feedback = "";
            if (isSystemDesign)
            {
                feedback = answerLength < 150 
                    ? "System design answer is too brief. Include components, trade-offs, and scalability considerations."
                    : hasCode && hasMetrics
                    ? "Good design discussion with technical depth and measurable outcomes."
                    : "Solid design explanation; consider adding implementation details or performance metrics.";
            }
            else if (isChallengeQuestion)
            {
                feedback = hasLessons && hasTeamwork
                    ? "Excellent handling of challenge with clear lessons learned and collaboration demonstrated."
                    : answerLength < 100
                    ? "Challenge answer needs more detail. Explain the situation, your actions, and the outcome."
                    : "Good problem-solving approach; highlight what you learned from this experience.";
            }
            else if (isExperienceQuestion)
            {
                feedback = answerLength < 100 
                    ? "Experience description is brief. Provide context, your specific role, and results."
                    : hasMetrics && hasCode
                    ? "Strong example with quantifiable impact and technical involvement."
                    : "Good experience narrative; add specific metrics or technical achievements.";
            }
            else
            {
                feedback = answerLength < 100 
                    ? "Answer is too brief. Expand with more details and specific examples."
                    : "Good answer; consider adding more depth or examples.";
            }

            var strengths = new List<string>();
            if (answerLength > 150) strengths.Add("Detailed explanation");
            if (hasMetrics) strengths.Add("Quantifiable impact");
            if (hasCode && isTechnical) strengths.Add("Technical depth");
            if (hasTeamwork) strengths.Add("Collaboration skills");
            if (hasLessons) strengths.Add("Self-reflection and learning");
            if (isSystemDesign && answer.ToLower().Contains("trade")) strengths.Add("Systems thinking");
            if (isExperienceQuestion && answer.ToLower().Contains("leadership")) strengths.Add("Leadership demonstrated");

            // Build context-aware improvements list
            var improvements = new List<string>();
            if (!hasMetrics) improvements.Add("Include specific metrics or measurable results");
            if (isSystemDesign && !answer.ToLower().Contains("scalab"))
                improvements.Add("Discuss scalability and performance considerations");
            if (isChallengeQuestion && !hasLessons)
                improvements.Add("Clearly articulate lessons learned from this challenge");
            if (!hasTeamwork && isBehavioral)
                improvements.Add("Emphasize teamwork and collaboration aspects");
            if (answerLength < 150)
                improvements.Add("Provide more specific details and examples");
            if (improvements.Count == 0)
                improvements.Add("Overall strong response; consider adding more technical depth");


            return new InterviewAnalysisResult
            {
                OverallScore = score,
                Feedback = feedback,
                Strengths = strengths,
                Improvements = improvements
            };
        }

        /// <summary>
        /// Generate a consolidated interview summary given questions, answers and feedback
        /// </summary>
        public async Task<InterviewSummaryResult> GenerateInterviewSummaryAsync(InterviewSummaryRequest request)
        {
            // Build prompt for OpenRouter
            var system = "You are an interview summarizer. Provide a concise 1-paragraph summary (3-4 sentences) of the candidate's interview performance. Include overall score (0-10). Return JSON: { \"Summary\": \"...\", \"OverallScore\": 8.5 }";

            // Compose user content with items enumerated
            var sb = new System.Text.StringBuilder();
            if (!string.IsNullOrWhiteSpace(request.Role)) sb.AppendLine($"Role: {request.Role}");
            if (!string.IsNullOrWhiteSpace(request.InterviewType)) sb.AppendLine($"InterviewType: {request.InterviewType}");
            sb.AppendLine("\nInterview Items:");
            for (int i = 0; i < Math.Max(request.Questions?.Count ?? 0, request.Answers?.Count ?? 0); i++)
            {
                var q = (request.Questions != null && i < request.Questions.Count) ? request.Questions[i] : "";
                var a = (request.Answers != null && i < request.Answers.Count) ? request.Answers[i] : "";
                var f = (request.Feedbacks != null && i < request.Feedbacks.Count) ? request.Feedbacks[i] : "";
                sb.AppendLine($"Q{i+1}: {q}");
                sb.AppendLine($"A{i+1}: {a}");
                if (!string.IsNullOrWhiteSpace(f)) sb.AppendLine($"F{i+1}: {f}");
                sb.AppendLine();
            }

            var reply = await CallOpenRouterAsync(system, sb.ToString(), 0.2, 400);
            if (!string.IsNullOrWhiteSpace(reply))
            {
                var parsed = TryDeserializeObject<InterviewSummaryResult>(reply);
                if (parsed != null)
                    return parsed;
            }

            // Fallback summarization: single paragraph summary with score
            decimal totalScore = 0;
            int scored = 0;
            int detailedAnswers = 0;
            int measurableResults = 0;

            for (int i = 0; i < (request.Answers?.Count ?? 0); i++)
            {
                var ans = (request.Answers != null && i < request.Answers.Count) ? request.Answers[i] : "";
                var fb = (request.Feedbacks != null && i < request.Feedbacks.Count) ? request.Feedbacks[i] : "";

                // Score heuristic: answer length, measurable results, feedback quality
                var score = 5.0m;
                if (ans.Length > 120) { score += 1.5m; detailedAnswers++; }
                if (ans.Contains("%") || ans.ToLower().Contains("improved")) { score += 1.5m; measurableResults++; }
                if (!string.IsNullOrWhiteSpace(fb) && fb.Length > 30) score += 1.0m;
                score = Math.Min(score, 10.0m);
                totalScore += score;
                scored++;
            }

            var overall = scored > 0 ? Math.Round(totalScore / scored, 1) : 5.0m;

            // Build concise 1-paragraph summary
            var strengths = detailedAnswers > 0 ? "The candidate provided detailed, thoughtful answers" : "The candidate answered all questions";
            var performance = measurableResults > 0 ? " with quantifiable outcomes and clear results" : " demonstrating solid understanding";
            var summary = $"{strengths}{performance}. For the {request.Role ?? "target"} role in a {request.InterviewType ?? "technical"} setting, the candidate shows " +
                         (overall >= 8 ? "strong technical competency and communication skills." : 
                          overall >= 6 ? "adequate skills with room for deeper technical depth." :
                          "foundational understanding but needs further development.") +
                         $" Overall interview score: {overall}/10.";

            return new InterviewSummaryResult
            {
                Summary = summary,
                OverallScore = overall,
                TopStrengths = new List<string>(),
                MainImprovements = new List<string>(),
                PerQuestionSummary = new List<string>()
            };
        }

        /// <summary>
        /// Generate interview questions based on role and interview type
        /// </summary>
        public async Task<List<string>> GenerateInterviewQuestionsAsync(string role, string interviewType, int questionCount)
        {
            // If an OpenRouter API key is configured in env, call the model. Otherwise use the local mock.
            // Ensure role and interviewType are not null
            role = role ?? "Software Engineer";
            interviewType = interviewType ?? "Technical";

            Console.WriteLine("[AI DEBUG] GenerateInterviewQuestionsAsync called. Checking for API key...");
            var apiKey = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY");
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                Console.WriteLine($"[AI DEBUG] OpenRouter API key loaded from environment (OPENROUTER_API_KEY): key length={apiKey.Length}");
            }
            else
            {
                Console.WriteLine("[AI DEBUG] OPENROUTER_API_KEY environment variable is empty or null.");
            }

            if (string.IsNullOrWhiteSpace(apiKey) && _config != null)
            {
                Console.WriteLine("[AI DEBUG] Attempting to load from configuration (OpenRouter:ApiKey)...");
                apiKey = _config["OpenRouter:ApiKey"];
                if (!string.IsNullOrWhiteSpace(apiKey))
                {
                    Console.WriteLine($"[AI DEBUG] OpenRouter API key loaded from configuration (OpenRouter:ApiKey): key length={apiKey.Length}");
                }
                else
                {
                    Console.WriteLine("[AI DEBUG] Configuration key OpenRouter:ApiKey is empty or null.");
                }
            }

            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                Console.WriteLine("[AI DEBUG] API key found. Proceeding with OpenRouter call.");
                try
                {
                    using var http = new System.Net.Http.HttpClient();
                    http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

                    var systemPrompt = "You are an interview assistant. Generate concise, role-specific interview questions and return only a JSON array of strings (no extra text).";
                    var userPrompt = $"Generate {questionCount} {interviewType} interview questions for the role '{role}'. Include one role-specific opening question and the rest focused on {interviewType.ToLower()} topics. Return output as a JSON array of strings.";

                    var payload = new
                    {
                        model = "mistralai/mistral-7b-instruct",
                        messages = new[] {
                            new { role = "system", content = systemPrompt },
                            new { role = "user", content = userPrompt }
                        },
                        temperature = 0.7,
                        max_tokens = 512
                    };

                    var json = System.Text.Json.JsonSerializer.Serialize(payload);
                    using var content = new System.Net.Http.StringContent(json, System.Text.Encoding.UTF8, "application/json");

                    Console.WriteLine("[AI DEBUG] Sending request to OpenRouter...");
                    var resp = await http.PostAsync("https://openrouter.ai/api/v1/chat/completions", content);
                    var respBody = await resp.Content.ReadAsStringAsync();
                    Console.WriteLine($"[AI DEBUG] OpenRouter response status: {resp.StatusCode}");

                    if (!resp.IsSuccessStatusCode)
                    {
                        // fall back to mock if API call fails
                        Console.WriteLine($"[AI DEBUG] OpenRouter request failed: {resp.StatusCode} {respBody?.Substring(0, Math.Min(200, respBody?.Length ?? 0))}");
                        System.Diagnostics.Debug.WriteLine($"OpenRouter request failed: {resp.StatusCode} {respBody}");
                        return GenerateInterviewQuestionsMock(role, interviewType, questionCount);
                    }

                    // Try to parse the response for a generated text
                    Console.WriteLine($"[AI DEBUG] Parsing OpenRouter response (length={respBody?.Length})...");
                    try
                    {
                        using var doc = System.Text.Json.JsonDocument.Parse(respBody);
                        // Common shape: { choices: [ { message: { content: "..." } } ] }
                        if (doc.RootElement.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
                        {
                            var first = choices[0];
                            if (first.TryGetProperty("message", out var message) && message.TryGetProperty("content", out var contentElem))
                            {
                                var text = contentElem.GetString() ?? string.Empty;
                                // expect JSON array inside text
                                Console.WriteLine($"[AI DEBUG] Found message.content (length={text.Length}): {text.Substring(0, Math.Min(200, text.Length))}");
                                var parsed = TryExtractJsonArray(text);
                                if (parsed != null && parsed.Any())
                                {
                                    Console.WriteLine($"[AI DEBUG] Successfully parsed {parsed.Count} questions from OpenRouter.");
                                    return parsed.Take(questionCount).ToList();
                                }
                                else
                                {
                                    Console.WriteLine("[AI DEBUG] Failed to extract JSON array from message.content.");
                                }
                            }
                            // some providers return 'content' directly on choices[0]
                            if (first.TryGetProperty("content", out var contentAlt))
                            {
                                var text = contentAlt.GetString() ?? string.Empty;
                                var parsed = TryExtractJsonArray(text);
                                if (parsed != null && parsed.Any())
                                    return parsed.Take(questionCount).ToList();
                            }
                        }

                        // As a last resort, try to find any string in the response and split into lines
                        var allText = doc.RootElement.ToString();
                        var fallbackList = allText.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(s => s.Trim()).Where(s => s.Length > 10).Take(questionCount).ToList();
                        if (fallbackList.Any())
                            return fallbackList;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"OpenRouter parse error: {ex.Message}");
                        return GenerateInterviewQuestionsMock(role, interviewType, questionCount);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"OpenRouter call failed: {ex.Message}");
                    return GenerateInterviewQuestionsMock(role, interviewType, questionCount);
                }
            }

            // Fallback mock generation
            Console.WriteLine("[AI DEBUG] No API key or OpenRouter call failed. Using mock fallback.");
            return GenerateInterviewQuestionsMock(role, interviewType, questionCount);
        }

        // Helper: Try to extract a JSON array of strings from a text blob
        private static List<string>? TryExtractJsonArray(string text)
        {
            text = text.Trim();
            // If text starts with '[' assume it's a JSON array
            try
            {
                if (text.StartsWith("["))
                {
                    var arr = System.Text.Json.JsonSerializer.Deserialize<List<string>>(text);
                    return arr;
                }

                // Try to locate first '[' and last ']' and parse the substring
                var start = text.IndexOf('[');
                var end = text.LastIndexOf(']');
                if (start >= 0 && end > start)
                {
                    var sub = text.Substring(start, end - start + 1);
                    var arr = System.Text.Json.JsonSerializer.Deserialize<List<string>>(sub);
                    return arr;
                }
            }
            catch { }

            return null;
        }

        // Helper: Try to extract a JSON array of QuizMCQQuestionDto objects from a text blob
        private static List<QuizMCQQuestionDto>? TryExtractQuizMCQArray(string text)
        {
            text = text.Trim();
            try
            {
                if (text.StartsWith("["))
                {
                    return System.Text.Json.JsonSerializer.Deserialize<List<QuizMCQQuestionDto>>(text);
                }
                var start = text.IndexOf('[');
                var end = text.LastIndexOf(']');
                if (start >= 0 && end > start)
                {
                    var sub = text.Substring(start, end - start + 1);
                    return System.Text.Json.JsonSerializer.Deserialize<List<QuizMCQQuestionDto>>(sub);
                }
            }
            catch { }
            return null;
        }

        // Existing mock extraction kept as fallback
        private List<string> GenerateInterviewQuestionsMock(string role, string interviewType, int questionCount)
        {
            // Ensure role and interviewType are not null
            role = role ?? "Software Engineer";
            interviewType = interviewType ?? "Technical";

            var questions = new List<string>
            {
                $"Can you describe your experience with the key technologies and skills required for a {role} position?",
                $"Tell us about a complex project you've worked on as a {role} and how you approached solving it."
            };

            if (interviewType.ToLower() == "technical")
            {
                questions.Add("Can you walk us through your approach to system design and architecture?");
                questions.Add("How do you approach problem-solving and debugging when facing complex technical challenges?");
            }
            else if (interviewType.ToLower() == "behavioral")
            {
                questions.Add("Describe a situation where you had to work with a difficult team member. How did you handle it?");
                questions.Add("Tell us about a time you failed and what you learned from it.");
            }

            return questions.Take(questionCount).ToList();
        }

        /// <summary>
        /// Get career path recommendations
        /// </summary>
        public async Task<CareerPathResult> RecommendCareerPathAsync(CareerPathRequest request)
        {
            var system = "You are a career path advisor. Given the user's current role, skills and experience, return a JSON object matching CareerPathResult: { RecommendedRoles, SkillsToLearn, Certifications, ActionPlan }. Return ONLY the JSON object.";
            var user = $"CurrentRole: {request?.CurrentRole}\nSkills: {string.Join(",", request?.Skills ?? new List<string>())}\nExperienceYears: {request?.ExperienceYears}\nDesiredField: {request?.DesiredField}";
            var reply = await CallOpenRouterAsync(system, user, 0.7, 800);
            if (!string.IsNullOrWhiteSpace(reply))
            {
                var parsed = TryDeserializeObject<CareerPathResult>(reply);
                if (parsed != null)
                    return parsed;
            }

            await Task.Delay(700);

            return new CareerPathResult
            {
                RecommendedRoles = new List<string>
                {
                    "Senior Software Engineer",
                    "Solutions Architect",
                    "Technical Lead",
                    "Engineering Manager",
                    "Staff Engineer"
                },
                SkillsToLearn = new List<string>
                {
                    "System Design",
                    "Leadership and Management",
                    "Cloud Architecture (AWS/Azure)",
                    "Microservices Architecture",
                    "DevOps and CI/CD"
                },
                Certifications = new List<string>
                {
                    "AWS Solutions Architect Professional",
                    "Microsoft Azure Administrator",
                    "Kubernetes Administrator (CKA)"
                },
                ActionPlan = new List<string>
                {
                    "Month 1-2: Focus on system design fundamentals",
                    "Month 3-4: Complete AWS architect certification",
                    "Month 5-6: Lead a major architectural project",
                    "Month 7-8: Develop leadership skills through team mentoring",
                    "Month 9-10: Work on cross-functional initiatives",
                    "Month 11-12: Plan for senior engineering or management transition"
                }
            };
        }

        /// <summary>
        /// Identify skill gaps
        /// </summary>
        public async Task<List<SkillGap>> IdentifySkillGapsAsync(List<string> currentSkills, string targetRole)
        {
            var system = "You are a skills advisor. Given a list of current skills and a target role, return a JSON array of SkillGap objects: { Skill, Proficiency, ResourceUrl, EstimatedMonths }. Return ONLY the JSON array.";
            var user = $"CurrentSkills: {string.Join(",", currentSkills ?? new List<string>())}\nTargetRole: {targetRole}";
            var reply = await CallOpenRouterAsync(system, user, 0.7, 600);
            if (!string.IsNullOrWhiteSpace(reply))
            {
                var parsed = TryDeserializeObject<List<SkillGap>>(reply);
                if (parsed != null)
                    return parsed;
            }

            await Task.Delay(400);

            return new List<SkillGap>
            {
                new SkillGap
                {
                    Skill = "Kubernetes",
                    Proficiency = "Beginner",
                    ResourceUrl = "https://kubernetes.io/docs/",
                    EstimatedMonths = 3
                },
                new SkillGap
                {
                    Skill = "Docker",
                    Proficiency = "Intermediate",
                    ResourceUrl = "https://docs.docker.com/",
                    EstimatedMonths = 2
                },
                new SkillGap
                {
                    Skill = "Terraform",
                    Proficiency = "Beginner",
                    ResourceUrl = "https://www.terraform.io/docs/",
                    EstimatedMonths = 2
                },
                new SkillGap
                {
                    Skill = "System Design",
                    Proficiency = "Intermediate",
                    ResourceUrl = "https://github.com/donnemartin/system-design-primer",
                    EstimatedMonths = 4
                }
            };
        }

        /// <summary>
        /// Send custom prompt to AI
        /// </summary>
        public async Task<AIPromptResponse> SendPromptAsync(AIPromptRequest request)
        {
            var system = request?.SystemPrompt ?? "You are an assistant.";
            var user = request?.Prompt ?? string.Empty;
            var reply = await CallOpenRouterAsync(system, user, request?.Temperature ?? 0.7, request?.MaxTokens ?? 1000);
            if (!string.IsNullOrWhiteSpace(reply))
            {
                return new AIPromptResponse
                {
                    Response = reply,
                    TokensUsed = 0,
                    GeneratedAt = DateTime.UtcNow
                };
            }

            await Task.Delay(1000);
            return new AIPromptResponse
            {
                Response = "This is a mock response from the AI service. In production, this would be connected to an actual AI provider like OpenRouter.",
                TokensUsed = 0,
                GeneratedAt = DateTime.UtcNow
            };
        }

        #region CV Builder Features

        /// <summary>
        /// Generate a new CV from user information
        /// </summary>
        public async Task<GenerateCVResult> GenerateCVAsync(GenerateCVRequest request)
        {
            await Task.Delay(1500);

            var cvContent = $@"
{request.FullName}
{request.Location}

PROFESSIONAL SUMMARY
{request.Summary}

EXPERIENCE
Senior Full Stack Developer | 3+ Years
- Led development of scalable applications
- Mentored junior developers
- Improved system performance by 40%

SKILLS
{string.Join(", ", request.Skills)}

EDUCATION
Bachelor of Science in Computer Science
";

            return new GenerateCVResult
            {
                CVContent = cvContent,
                CVHtmlFormat = GenerateCVHtml(cvContent, request),
                TemplateName = request.CVTemplate,
                DownloadUrl = "/cv/download/template-" + DateTime.UtcNow.Ticks,
                Success = true,
                Message = "CV generated successfully using " + request.CVTemplate + " template"
            };
        }

        /// <summary>
        /// Improve an existing CV with AI suggestions
        /// </summary>
        public async Task<ImproveCVResult> ImproveCVAsync(ImproveCVRequest request)
        {
            await Task.Delay(1500);

            var suggestions = new List<ImprovementSuggestion>
            {
                new ImprovementSuggestion
                {
                    Section = "summary",
                    CurrentText = "Have 5 years of experience",
                    SuggestedText = "Accomplished Full Stack Developer with 5+ years of experience building scalable web applications using modern technologies and best practices",
                    Reason = "More compelling and action-oriented",
                    Priority = 1
                },
                new ImprovementSuggestion
                {
                    Section = "experience",
                    CurrentText = "Worked on various projects",
                    SuggestedText = "Led cross-functional team to deliver 5 major projects, resulting in 30% improvement in user engagement and 25% reduction in system latency",
                    Reason = "Use metrics and quantify achievements",
                    Priority = 1
                },
                new ImprovementSuggestion
                {
                    Section = "skills",
                    CurrentText = "Know programming languages",
                    SuggestedText = "Proficient in: C#, ASP.NET Core, React, SQL Server; Experienced with: Azure, AWS, Docker",
                    Reason = "Be specific about skill levels and technologies",
                    Priority = 2
                }
            };

            var improvedContent = request.CurrentCVText + "\n\n[AI Suggestions Applied]";

            return new ImproveCVResult
            {
                ImprovedCVContent = improvedContent,
                Suggestions = suggestions,
                Summary = "Your CV has been analyzed. We found 3 opportunities for improvement. With these changes, your CV would be 35% more compelling to recruiters.",
                ImprovementScore = 75,
                ChangesApplied = new List<string>
                {
                    "Enhanced professional summary with action verbs",
                    "Quantified achievements with metrics",
                    "Added specific skill levels and certifications",
                    "Improved formatting and readability"
                }
            };
        }

        /// <summary>
        /// Parse CV content and extract structured information
        /// </summary>
        public async Task<ParseCVResult> ParseCVAsync(ParseCVRequest request)
        {
            await Task.Delay(1500);

            return new ParseCVResult
            {
                PersonalInfo = new PersonalInfo
                {
                    FullName = "John Doe",
                    Email = "john.doe@example.com",
                    PhoneNumber = "+1-234-567-8900",
                    Location = "San Francisco, CA",
                    LinkedInUrl = "linkedin.com/in/johndoe",
                    GithubUrl = "github.com/johndoe",
                    PortfolioUrl = "johndoe.dev"
                },
                ProfessionalSummary = new ProfessionalSummary
                {
                    Summary = "Full Stack Developer with 5+ years of experience building scalable web applications",
                    YearsOfExperience = 5,
                    CurrentRole = "Senior Full Stack Developer",
                    CareerFocus = "Cloud Architecture and Microservices"
                },
                WorkExperience = new List<WorkExperience>
                {
                    new WorkExperience
                    {
                        CompanyName = "Tech Corporation",
                        JobTitle = "Senior Full Stack Developer",
                        StartDate = new DateTime(2021, 1, 15),
                        EndDate = null,
                        IsCurrentRole = true,
                        DurationMonths = 36,
                        Description = "Led development of microservices architecture",
                        KeyAchievements = new List<string> { "30% performance improvement", "Led team of 4 developers" },
                        SkillsUsed = new List<string> { "C#", "ASP.NET Core", "Azure", "Kubernetes" }
                    }
                },
                Education = new List<Education>
                {
                    new Education
                    {
                        Institution = "State University",
                        Degree = "Bachelor of Science",
                        FieldOfStudy = "Computer Science",
                        GraduationYear = 2018,
                        GPA = "3.8",
                        Coursework = new List<string> { "Algorithms", "Data Structures", "Cloud Computing" }
                    }
                },
                Skills = new List<string> { "C#", "ASP.NET Core", "React", "SQL Server", "Azure", "AWS", "Docker", "Kubernetes" },
                Certifications = new List<Certification>
                {
                    new Certification
                    {
                        CertificationName = "AWS Solutions Architect Professional",
                        IssuingOrganization = "Amazon Web Services",
                        IssuedYear = 2022,
                        ExpirationYear = 2025,
                        CredentialUrl = "aws.amazon.com/certification"
                    }
                },
                Languages = new List<string> { "English (Fluent)", "Spanish (Intermediate)" },
                ParseQuality = "excellent",
                ParseConfidenceScore = 92
            };
        }

        #endregion

        #region Job Description Parser Features

        /// <summary>
        /// Parse job description and extract structured information
        /// </summary>
        public async Task<ParseJobDescriptionResult> ParseJobDescriptionAsync(ParseJobDescriptionRequest request)
        {
            var system = "You are a job parser. Given a job description and optional job title/company, return a JSON object matching ParseJobDescriptionResult with fields: BasicInfo, RequiredSkills, PreferredSkills, Responsibilities, Requirements, Benefits, KeyTechnologies, SeniorityLevel, EmploymentType, ParseQuality, ParseConfidenceScore. Return ONLY the JSON object.";

            // Allow users to paste the entire job posting into the single `JobDescription` field.
            // If `JobTitle`/`CompanyName` are empty, try to extract them from the pasted text.
            var rawDescription = request?.JobDescription ?? string.Empty;
            var jobTitle = request?.JobTitle;
            var companyName = request?.CompanyName;

            if (string.IsNullOrWhiteSpace(jobTitle) && string.IsNullOrWhiteSpace(companyName) && !string.IsNullOrWhiteSpace(rawDescription))
            {
                // Normalize line endings and split into non-empty lines
                var lines = rawDescription.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                                      .Select(l => l.Trim())
                                      .Where(l => !string.IsNullOrWhiteSpace(l))
                                      .ToList();

                // Common patterns: first line may be title, second line company. Also support lines like "JobTitle: ..." or "CompanyName: ..."
                if (lines.Count > 0)
                {
                    // If first line contains a prefix like 'JobTitle:' or 'Title:' extract it
                    if (lines[0].StartsWith("JobTitle:", StringComparison.OrdinalIgnoreCase) || lines[0].StartsWith("Title:", StringComparison.OrdinalIgnoreCase))
                    {
                        var parts = lines[0].Split(new[] { ':' }, 2);
                        if (parts.Length == 2) { jobTitle = parts[1].Trim(); lines.RemoveAt(0); }
                    }
                    else if (string.IsNullOrWhiteSpace(jobTitle) && lines[0].Length <= 200 && !lines[0].Contains("@") && lines[0].Split(' ').Length <= 10)
                    {
                        // Heuristic: short first line without an email and not overly long — treat as title
                        jobTitle = lines[0]; lines.RemoveAt(0);
                    }
                }

                if (lines.Count > 0)
                {
                    if (lines[0].StartsWith("CompanyName:", StringComparison.OrdinalIgnoreCase) || lines[0].StartsWith("Company:", StringComparison.OrdinalIgnoreCase))
                    {
                        var parts = lines[0].Split(new[] { ':' }, 2);
                        if (parts.Length == 2) { companyName = parts[1].Trim(); lines.RemoveAt(0); }
                    }
                    else if (string.IsNullOrWhiteSpace(companyName) && lines[0].Length <= 200 && lines[0].Split(' ').Length <= 6)
                    {
                        // Heuristic: treat the next short line as company name
                        companyName = lines[0]; lines.RemoveAt(0);
                    }
                }

                rawDescription = string.Join("\n", lines);
            }

            var user = $"JobTitle: {jobTitle}\nCompanyName: {companyName}\n\nJobDescription:\n{rawDescription}";
            var reply = await CallOpenRouterAsync(system, user, 0.7, 1200);
            System.Diagnostics.Debug.WriteLine($"[AI DEBUG] Job parser prompt:\n{user}");
            Console.WriteLine($"[AI DEBUG] Job parser prompt:\n{user}");
            System.Diagnostics.Debug.WriteLine($"[AI DEBUG] Raw LLM response for job parsing: {reply}");
            Console.WriteLine($"[AI DEBUG] Raw LLM response for job parsing: {reply}");
            if (!string.IsNullOrWhiteSpace(reply))
            {
                var parsed = TryDeserializeObject<ParseJobDescriptionResult>(reply);
                if (parsed != null)
                    return parsed;
                // Log parsing failure so we can diagnose why fallback was used
                System.Diagnostics.Debug.WriteLine($"[AI DEBUG] ParseJobDescription: parsed result was null — using fallback. Raw reply length={reply.Length}");
                Console.WriteLine($"[AI DEBUG] ParseJobDescription: parsed result was null — using fallback. Raw reply length={reply.Length}");
                System.Diagnostics.Debug.WriteLine($"[AI DEBUG] ParseJobDescription: raw reply preview: {reply?.Substring(0, Math.Min(800, reply.Length))}");
                Console.WriteLine($"[AI DEBUG] ParseJobDescription: raw reply preview: {reply?.Substring(0, Math.Min(800, reply.Length))}");

                // Attempt a tolerant mapping from the raw JSON to the DTO (handles PascalCase, extra fields, etc.)
                try
                {
                    var tolerant = TryMapJsonToParseJobDescriptionResult(reply!);
                    if (tolerant != null)
                    {
                        System.Diagnostics.Debug.WriteLine("[AI DEBUG] ParseJobDescription: tolerant JSON mapping succeeded — returning mapped result.");
                        Console.WriteLine("[AI DEBUG] ParseJobDescription: tolerant JSON mapping succeeded — returning mapped result.");
                        return tolerant;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[AI DEBUG] ParseJobDescription: tolerant mapping threw: {ex.Message}");
                    Console.WriteLine($"[AI DEBUG] ParseJobDescription: tolerant mapping threw: {ex.Message}");
                }
            }

            // If we reached here, either OpenRouter returned empty or parsing failed.
            // Try a second, stricter call with an explicit JSON schema and example to encourage a well-formed JSON response.
            try
            {
                var strictSystem = "You are a job parser. Return ONLY a single JSON object matching the schema exactly. Do NOT include any extra text. Use the field names exactly as shown. Wrap the JSON in triple backticks. Schema: { BasicInfo: { JobTitle, CompanyName, Location, IsRemote, SalaryRangeMin, SalaryRangeMax, SalaryCurrency, SalaryPeriod, ExperienceYearsRequired, Department, ReportingLine }, RequiredSkills: [string], PreferredSkills: [string], Responsibilities: [string], Requirements: [string], Benefits: { Benefits: [string], HealthInsurance, RetirementPlan, PaidTimeOff, ProfessionalDevelopment, RemoteWorkOptions, FlexibleSchedule }, KeyTechnologies: [string], SeniorityLevel, EmploymentType, ParseQuality, ParseConfidenceScore }";

                                var example = @"
```
{
    ""BasicInfo"": { ""JobTitle"": ""UI/UX Designer"", ""CompanyName"": ""Example Co"", ""Location"": ""Remote"", ""IsRemote"": true, ""SalaryRangeMin"": ""40000"", ""SalaryRangeMax"": ""60000"", ""SalaryCurrency"": ""USD"", ""SalaryPeriod"": ""annual"", ""ExperienceYearsRequired"": 2, ""Department"": ""Design"", ""ReportingLine"": [""Head of Design""] },
    ""RequiredSkills"": [""UI Design"", ""Figma"", ""Prototyping""],
    ""PreferredSkills"": [""HTML"", ""CSS"", ""JavaScript""],
    ""Responsibilities"": [""Create wireframes"", ""Design prototypes""],
    ""Requirements"": [""3+ years experience"", ""Portfolio""],
    ""Benefits"": { ""Benefits"": [""Health insurance"", ""Paid time off""], ""HealthInsurance"": ""Yes"", ""RetirementPlan"": ""401(k)"", ""PaidTimeOff"": 20, ""ProfessionalDevelopment"": ""$1000/year"", ""RemoteWorkOptions"": true, ""FlexibleSchedule"": true },
    ""KeyTechnologies"": [""Figma"", ""Sketch"", ""HTML5""],
    ""SeniorityLevel"": ""mid"",
    ""EmploymentType"": ""full-time"",
    ""ParseQuality"": ""good"",
    ""ParseConfidenceScore"": 90
}
```
";

                var strictUser = $"JobTitle: {jobTitle}\nCompanyName: {companyName}\n\nJobDescription:\n{rawDescription}\n\nPlease respond with the JSON only, as in the example above.";

                Console.WriteLine("[AI DEBUG] ParseJobDescription: making strict follow-up call to OpenRouter to encourage JSON output.");
                var strictReply = await CallOpenRouterAsync(strictSystem + "\nExample:" + example, strictUser, 0.0, 1400);
                Console.WriteLine($"[AI DEBUG] Strict follow-up raw reply: {strictReply}");
                if (!string.IsNullOrWhiteSpace(strictReply))
                {
                    var parsed2 = TryDeserializeObject<ParseJobDescriptionResult>(strictReply);
                    if (parsed2 != null) return parsed2;

                    try
                    {
                        var tolerant2 = TryMapJsonToParseJobDescriptionResult(strictReply);
                        if (tolerant2 != null) return tolerant2;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[AI DEBUG] ParseJobDescription strict mapping threw: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AI DEBUG] ParseJobDescription: strict follow-up call failed: {ex.Message}");
            }

            await Task.Delay(2000);

            return new ParseJobDescriptionResult
            {
                BasicInfo = new JobBasicInfo
                {
                    JobTitle = "Senior Full Stack Developer",
                    CompanyName = "Tech Startup",
                    Location = "San Francisco, CA",
                    IsRemote = true,
                    SalaryRangeMin = "$120,000",
                    SalaryRangeMax = "$160,000",
                    SalaryCurrency = "USD",
                    SalaryPeriod = "annual",
                    ExperienceYearsRequired = 5,
                    Department = "Engineering",
                    ReportingLine = new List<string> { "Engineering Manager", "VP of Engineering" }
                },
                RequiredSkills = new List<string> { "C#", "ASP.NET Core", "React", "SQL Server", "Azure", "Microservices" },
                PreferredSkills = new List<string> { "Kubernetes", "Docker", "AWS", "GraphQL", "RabbitMQ" },
                Responsibilities = new List<string>
                {
                    "Design and develop scalable backend services",
                    "Build responsive frontend applications",
                    "Implement CI/CD pipelines",
                    "Mentor junior developers",
                    "Participate in code reviews"
                },
                Requirements = new List<string>
                {
                    "5+ years of software development experience",
                    "Strong knowledge of .NET ecosystem",
                    "Experience with cloud platforms (Azure or AWS)",
                    "Familiarity with microservices architecture",
                    "Excellent problem-solving skills"
                },
                Benefits = new JobBenefits
                {
                    Benefits = new List<string> { "Competitive salary", "Health insurance", "401(k) matching", "Remote work", "Professional development" },
                    HealthInsurance = "Full coverage for employee and dependents",
                    RetirementPlan = "401(k) with 4% match",
                    PaidTimeOff = 20,
                    ProfessionalDevelopment = "$2000/year training budget",
                    RemoteWorkOptions = true,
                    FlexibleSchedule = true
                },
                KeyTechnologies = new List<string> { "C#", ".NET Core", "React", "Azure", "Kubernetes", "Docker", "SQL Server" },
                SeniorityLevel = "senior",
                EmploymentType = "full-time",
                ParseQuality = "excellent",
                ParseConfidenceScore = 95
            };
        }

        /// <summary>
        /// Extract required and preferred skills from job description
        /// </summary>
        public async Task<ExtractJobSkillsResult> ExtractJobSkillsAsync(ExtractJobSkillsRequest request)
        {
            var system = "You are a job skills extractor. Given a job description, return a JSON object matching ExtractJobSkillsResult: { RequiredSkills: [SkillDetail], PreferredSkills: [SkillDetail], KeyTechnologies, TotalSkillsIdentified }. Return ONLY the JSON object.";
            var user = request?.JobDescription ?? string.Empty;
            var reply = await CallOpenRouterAsync(system, user, 0.0, 600);
            if (!string.IsNullOrWhiteSpace(reply))
            {
                var parsed = TryDeserializeObject<ExtractJobSkillsResult>(reply);
                if (parsed != null)
                    return parsed;
            }

            await Task.Delay(1000);

            return new ExtractJobSkillsResult
            {
                RequiredSkills = new List<SkillDetail>
                {
                    new SkillDetail
                    {
                        SkillName = "C#",
                        Category = "technical",
                        ProficiencyLevel = "advanced",
                        ExperienceLevel = "3-5 years",
                        RelevanceScore = 95,
                        Importance = "critical"
                    },
                    new SkillDetail
                    {
                        SkillName = "ASP.NET Core",
                        Category = "technical",
                        ProficiencyLevel = "advanced",
                        ExperienceLevel = "3-5 years",
                        RelevanceScore = 92,
                        Importance = "critical"
                    },
                    new SkillDetail
                    {
                        SkillName = "React",
                        Category = "technical",
                        ProficiencyLevel = "intermediate",
                        ExperienceLevel = "1-3 years",
                        RelevanceScore = 85,
                        Importance = "important"
                    }
                },
                PreferredSkills = new List<SkillDetail>
                {
                    new SkillDetail
                    {
                        SkillName = "Kubernetes",
                        Category = "technical",
                        ProficiencyLevel = "intermediate",
                        ExperienceLevel = "1-3 years",
                        RelevanceScore = 70,
                        Importance = "nice-to-have"
                    },
                    new SkillDetail
                    {
                        SkillName = "Docker",
                        Category = "technical",
                        ProficiencyLevel = "intermediate",
                        ExperienceLevel = "1-3 years",
                        RelevanceScore = 65,
                        Importance = "nice-to-have"
                    }
                },
                KeyTechnologies = new List<string> { ".NET", "Azure", "SQL Server", "Microservices", "REST APIs" },
                TotalSkillsIdentified = 8
            };
        }

        /// <summary>
        /// Match candidate skills to job requirements
        /// </summary>
        public async Task<MatchSkillsToJobResult> MatchSkillsToJobAsync(MatchSkillsToJobRequest request)
        {
            var system = "You are a skill-matching assistant. Given candidate skills and a job description, return a JSON object matching MatchSkillsToJobResult including MatchedSkills, MissingRequiredSkills, MissingPreferredSkills, MatchPercentageRequired, MatchPercentagePreferred, OverallMatchPercentage, SkillGaps, RecommendedLearningPath. Return ONLY the JSON object.";
            var user = $"CandidateSkills: {string.Join(",", request?.CandidateSkills ?? new List<string>())}\n\nJobDescription:\n{request?.JobDescription}";
            var reply = await CallOpenRouterAsync(system, user, 0.7, 1000);
            if (!string.IsNullOrWhiteSpace(reply))
            {
                var parsed = TryDeserializeObject<MatchSkillsToJobResult>(reply);
                if (parsed != null)
                    return parsed;
            }

            await Task.Delay(1500);
            var jobSkills = ExtractSkillsFromText(request?.JobDescription ?? string.Empty);
            var matchedSkills = (request?.CandidateSkills ?? new List<string>()).Intersect(jobSkills, StringComparer.OrdinalIgnoreCase).ToList();
            var missingRequired = jobSkills.Except(matchedSkills, StringComparer.OrdinalIgnoreCase).ToList();

            return new MatchSkillsToJobResult
            {
                MatchedSkills = matchedSkills,
                MissingRequiredSkills = missingRequired.Take(4).ToList(),
                MissingPreferredSkills = new List<string> { "Kubernetes", "GraphQL", "System Design" },
                MatchPercentageRequired = (decimal)matchedSkills.Count / 6 * 100,
                MatchPercentagePreferred = (decimal)matchedSkills.Count / 3 * 100,
                OverallMatchPercentage = 78,
                SkillGaps = new List<SkillGapWithPriority>
                {
                    new SkillGapWithPriority
                    {
                        SkillName = "Kubernetes",
                        Importance = "important",
                        EstimatedLearningTimeWeeks = 8,
                        LearningResources = new List<string> { "Kubernetes.io official docs", "Udemy course: Kubernetes Complete Guide", "Linux Academy" },
                        CertificationPath = "CKA (Certified Kubernetes Administrator)"
                    },
                    new SkillGapWithPriority
                    {
                        SkillName = "System Design",
                        Importance = "critical",
                        EstimatedLearningTimeWeeks = 12,
                        LearningResources = new List<string> { "Designing Data-Intensive Applications book", "System Design Interview course", "LeetCode System Design" },
                        CertificationPath = "None standard; focus on interview preparation"
                    }
                },
                RecommendedLearningPath = new List<string>
                {
                    "Week 1-2: Review system design fundamentals",
                    "Week 3-6: Learn Kubernetes architecture and deployment",
                    "Week 7-8: Work on a side project using Kubernetes",
                    "Week 9-12: Practice system design interviews and scenarios"
                }
            };
        }

        #endregion

        /// <summary>
        /// Helper method to extract skills from text
        /// </summary>
        private List<string> ExtractSkillsFromText(string text)
        {
            text = text ?? string.Empty;
            var commonSkills = new[]
            {
                "C#", "ASP.NET Core", "React", "SQL Server", "Azure", "AWS",
                "Docker", "Kubernetes", "Git", "JavaScript", "Python", "Java"
            };

            return commonSkills.Where(skill => text.IndexOf(skill, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
        }

        /// <summary>
        /// Helper method to generate CV HTML format
        /// </summary>
        private string GenerateCVHtml(string content, GenerateCVRequest request)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <title>{request.FullName} - CV</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 40px; }}
        h1 {{ color: #333; }}
        .section {{ margin-top: 20px; }}
        .skill {{ display: inline-block; background: #e7f3fe; padding: 5px 10px; margin: 2px; border-radius: 3px; }}
    </style>
</head>
<body>
    <h1>{request.FullName}</h1>
    <p>{request.CurrentRole}</p>
    <div class=""section"">
        <h2>Summary</h2>
        <p>{request.Summary}</p>
    </div>
    <div class=""section"">
        <h2>Skills</h2>
        {string.Join("", request.Skills.Select(s => $"<span class='skill'>{s}</span>"))}
    </div>
</body>
</html>";
        }

        public async Task<List<QuizMCQQuestionDto>> GenerateQuizMCQQuestionsAsync(List<string> interests, int questionCount)
        {
            // Ensure interests is not null
            interests = interests ?? new List<string>();

            // Build system prompt for OpenRouter
            var system = "You are a quiz generator AI. Given a list of user interests, generate a JSON array of objects, each with 'QuestionText' and 'Choices' (an array of 4 options). Questions should be relevant to the interests and suitable for a career guidance quiz. Return ONLY the JSON array, no extra text.";
            var user = $"Interests: {string.Join(", ", interests)}\nQuestionCount: {questionCount}";

            var reply = await CallOpenRouterAsync(system, user, 0.7, 600);
            System.Diagnostics.Debug.WriteLine($"[AI DEBUG] Raw LLM response for quiz questions: {reply}");

            if (!string.IsNullOrWhiteSpace(reply))
            {
                var parsed = TryExtractQuizMCQArray(reply);
                if (parsed != null && parsed.Count > 0 && parsed.All(q => !string.IsNullOrWhiteSpace(q.QuestionText) && q.Choices?.Count == 4))
                    return parsed.Take(questionCount).ToList();
            }

            // Fallback: simple mock
            await Task.Delay(300);
            var questions = new List<QuizMCQQuestionDto>();
            for (int i = 0; i < questionCount; i++)
            {
                questions.Add(new QuizMCQQuestionDto
                {
                    QuestionText = $"Sample MCQ question {i + 1} for interests: {string.Join(", ", interests)}?",
                    Choices = new List<string> { "Option A", "Option B", "Option C", "Option D" }
                });
            }
            return questions;
        }
    }
}
