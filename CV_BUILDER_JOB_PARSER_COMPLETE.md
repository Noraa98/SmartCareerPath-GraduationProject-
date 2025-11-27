# SmartCareerPath - CV Builder & Job Parser Complete System Guide

## Overview

This guide documents the complete implementation and flow of three interconnected AI features in your SmartCareerPath application:

1. **CV Builder** - Generate, improve, and parse CVs
2. **Job Description Parser** - Extract structured data from job postings
3. **Skill Matching** - Match candidate skills to job requirements

---

## Architecture Overview

### 5-Layer Onion Architecture

```
┌─────────────────────────────────────────────────────────────┐
│ 1. API Layer                                                │
│ /SmartCareerPath.APIs/Controllers/AIController.cs           │
│ (HTTP Endpoints: POST /api/ai/generate-cv, parse-job, etc)  │
└─────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────┐
│ 2. Application Layer                                        │
│ /SmartCareerPath.Application/Features/AI/AICommands.cs      │
│ (Commands & MediatR Handlers)                               │
└─────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────┐
│ 3. Domain Layer                                             │
│ /SmartCareerPath.Domain/                                    │
│ (Business logic, entities, interfaces)                      │
└─────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────┐
│ 4. Infrastructure Layer                                     │
│ /SmartCareerPath.Infrastructure.Persistence/Services/AI/    │
│ AIService.cs (Actual implementations)                       │
└─────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────┐
│ 5. Data/Abstraction Layer                                   │
│ /SmartCareerPath.Application.Abstraction/DTOs/AI/           │
│ (Request/Response DTOs)                                     │
└─────────────────────────────────────────────────────────────┘
```

---

## Feature 1: CV Builder System

### Overview
The CV Builder allows users to:
- **Generate** new CVs from scratch using templates
- **Improve** existing CVs with AI-powered suggestions
- **Parse** CV content to extract structured information

### Data Flow Diagram

```
User Request
    ↓
[API Endpoint: generate-cv / improve-cv / parse-cv]
    ↓
[AIController validates request]
    ↓
[MediatR Command Router: GenerateCVCommand, ImproveCVCommand, ParseCVCommand]
    ↓
[Command Handlers process business logic]
    ↓
[IAIService methods: GenerateCVAsync, ImproveCVAsync, ParseCVAsync]
    ↓
[Mock AI Response (ready for real AI integration)]
    ↓
[Response sent back to client with DTOs]
    ↓
JSON Response (CVContent, Suggestions, ParsedData, etc.)
```

### API Endpoints

#### 1. Generate CV
**Endpoint:** `POST /api/ai/generate-cv`

**Request Body:**
```json
{
  "fullName": "John Doe",
  "currentRole": "Senior Full Stack Developer",
  "experienceYears": 5,
  "summary": "Accomplished full-stack developer with expertise in cloud architecture",
  "skills": ["C#", "ASP.NET Core", "React", "Azure", "Docker"],
  "cvTemplate": "professional"
}
```

**Response:**
```json
{
  "success": true,
  "message": "CV generated successfully using professional template",
  "data": {
    "cvContent": "John Doe\n\nPROFESSIONAL SUMMARY\n...",
    "cvHtmlFormat": "<html><body>...",
    "templateName": "professional",
    "downloadUrl": "/cv/download/template-637123456789",
    "success": true,
    "message": "CV generated successfully"
  },
  "errors": []
}
```

**Code Flow:**
```
POST /api/ai/generate-cv
  ↓
[AIController.GenerateCV() receives GenerateCVCommand]
  ↓
command.UserId = GetCurrentUserId();  // Set from JWT token
  ↓
_mediator.Send(command)  // MediatR routing
  ↓
[GenerateCVCommandHandler.Handle()]
  ↓
_aiService.GenerateCVAsync(GenerateCVRequest)
  ↓
[AIService.GenerateCVAsync()]
  - await Task.Delay(1500);  // Simulate AI processing
  - Generate CV content from request data
  - Generate HTML formatted version
  - Return GenerateCVResult
  ↓
BaseResponse<GenerateCVResult>.SuccessResult()
  ↓
JSON Response (200 OK)
```

#### 2. Improve CV
**Endpoint:** `POST /api/ai/improve-cv/{resumeId}`

**Request Body:**
```json
{
  "targetRole": "Principal Engineer",
  "improvementArea": "overall"
}
```

**Response:**
```json
{
  "success": true,
  "message": "CV improvement suggestions generated successfully",
  "data": {
    "improvedCVContent": "[Original CV with improvements applied]",
    "suggestions": [
      {
        "section": "summary",
        "currentText": "Have 5 years of experience",
        "suggestedText": "Accomplished Full Stack Developer with 5+ years of experience",
        "reason": "More compelling and action-oriented",
        "priority": 1
      }
    ],
    "summary": "Your CV has been analyzed. We found 3 opportunities for improvement.",
    "improvementScore": 75,
    "changesApplied": ["Enhanced professional summary", "Quantified achievements", "Added specific skill levels"]
  }
}
```

**Code Flow:**
```
POST /api/ai/improve-cv/1
  ↓
[AIController.ImproveCV() receives ImproveCVCommand]
  ↓
command.UserId = GetCurrentUserId();
command.ResumeId = 1;
  ↓
_mediator.Send(command)
  ↓
[ImproveCVCommandHandler.Handle()]
  ↓
_unitOfWork.Repository<Resume>().FirstOrDefaultAsync(r => r.Id == 1 && r.UserId == userId)
  ↓
[Get resume from database]
  ↓
_aiService.ImproveCVAsync(ImproveCVRequest)
  ↓
[AIService.ImproveCVAsync()]
  - Analyze CV content
  - Generate 3+ improvement suggestions
  - Rate improvement opportunities
  - Return ImproveCVResult
  ↓
BaseResponse<ImproveCVResult>.SuccessResult()
  ↓
JSON Response (200 OK)
```

#### 3. Parse CV
**Endpoint:** `POST /api/ai/parse-cv/{resumeId}`

**Response:**
```json
{
  "success": true,
  "message": "CV parsed successfully",
  "data": {
    "personalInfo": {
      "fullName": "John Doe",
      "email": "john.doe@example.com",
      "phoneNumber": "+1-234-567-8900",
      "location": "San Francisco, CA",
      "linkedInUrl": "linkedin.com/in/johndoe",
      "githubUrl": "github.com/johndoe",
      "portfolioUrl": "johndoe.dev"
    },
    "professionalSummary": {
      "summary": "Full Stack Developer with 5+ years of experience",
      "yearsOfExperience": 5,
      "currentRole": "Senior Full Stack Developer",
      "careerFocus": "Cloud Architecture and Microservices"
    },
    "workExperience": [
      {
        "companyName": "Tech Corporation",
        "jobTitle": "Senior Full Stack Developer",
        "startDate": "2021-01-15",
        "endDate": null,
        "isCurrentRole": true,
        "durationMonths": 36,
        "description": "Led development of microservices architecture",
        "keyAchievements": ["30% performance improvement", "Led team of 4 developers"],
        "skillsUsed": ["C#", "ASP.NET Core", "Azure", "Kubernetes"]
      }
    ],
    "education": [...],
    "skills": ["C#", "ASP.NET Core", "React", "SQL Server", "Azure", "AWS", "Docker"],
    "certifications": [...],
    "languages": ["English (Fluent)", "Spanish (Intermediate)"],
    "parseQuality": "excellent",
    "parseConfidenceScore": 92
  }
}
```

### Database Schema Impact

CVs are already stored in the `Resume` entity:

```csharp
public class Resume : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }  // CV content stored here
    public string FileUrl { get; set; }
    public string FileType { get; set; }
    public bool IsActive { get; set; }
    public bool IsPrimary { get; set; }
    // Navigation properties...
}
```

---

## Feature 2: Job Description Parser System

### Overview
The Job Parser allows:
- **Parse** job descriptions to extract structured information
- **Extract** required and preferred skills with proficiency levels
- **Match** candidate skills to job requirements with gap analysis

### API Endpoints

#### 1. Parse Job Description
**Endpoint:** `POST /api/ai/parse-job-description`

**Request Body:**
```json
{
  "jobDescription": "Senior Full Stack Developer position responsible for designing and implementing scalable web applications using C#, ASP.NET Core, React, and SQL Server. Must have 5+ years experience with cloud platforms like Azure or AWS...",
  "jobTitle": "Senior Full Stack Developer",
  "companyName": "Tech Startup"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Job description parsed successfully",
  "data": {
    "basicInfo": {
      "jobTitle": "Senior Full Stack Developer",
      "companyName": "Tech Startup",
      "location": "San Francisco, CA",
      "isRemote": true,
      "salaryRangeMin": "$120,000",
      "salaryRangeMax": "$160,000",
      "salaryCurrency": "USD",
      "salaryPeriod": "annual",
      "experienceYearsRequired": 5,
      "department": "Engineering",
      "reportingLine": ["Engineering Manager", "VP of Engineering"]
    },
    "requiredSkills": ["C#", "ASP.NET Core", "React", "SQL Server", "Azure", "Microservices"],
    "preferredSkills": ["Kubernetes", "Docker", "AWS", "GraphQL", "RabbitMQ"],
    "responsibilities": [
      "Design and develop scalable backend services",
      "Build responsive frontend applications",
      "Implement CI/CD pipelines",
      "Mentor junior developers"
    ],
    "requirements": [
      "5+ years of software development experience",
      "Strong knowledge of .NET ecosystem",
      "Experience with cloud platforms (Azure or AWS)"
    ],
    "benefits": {
      "benefits": ["Competitive salary", "Health insurance", "401(k) matching", "Remote work"],
      "healthInsurance": "Full coverage",
      "retirementPlan": "401(k) with 4% match",
      "paidTimeOff": 20,
      "professionalDevelopment": "$2000/year training budget",
      "remoteWorkOptions": true,
      "flexibleSchedule": true
    },
    "keyTechnologies": ["C#", ".NET Core", "React", "Azure", "Kubernetes"],
    "seniorityLevel": "senior",
    "employmentType": "full-time",
    "parseQuality": "excellent",
    "parseConfidenceScore": 95
  }
}
```

#### 2. Extract Job Skills
**Endpoint:** `POST /api/ai/extract-job-skills`

**Request Body:**
```json
{
  "jobDescription": "Senior Full Stack Developer with C#, ASP.NET Core, React, SQL Server expertise required..."
}
```

**Response:**
```json
{
  "success": true,
  "message": "Skills extracted successfully",
  "data": {
    "requiredSkills": [
      {
        "skillName": "C#",
        "category": "technical",
        "proficiencyLevel": "advanced",
        "experienceLevel": "3-5 years",
        "relevanceScore": 95,
        "importance": "critical"
      },
      {
        "skillName": "ASP.NET Core",
        "category": "technical",
        "proficiencyLevel": "advanced",
        "experienceLevel": "3-5 years",
        "relevanceScore": 92,
        "importance": "critical"
      }
    ],
    "preferredSkills": [
      {
        "skillName": "Kubernetes",
        "category": "technical",
        "proficiencyLevel": "intermediate",
        "experienceLevel": "1-3 years",
        "relevanceScore": 70,
        "importance": "nice-to-have"
      }
    ],
    "keyTechnologies": [".NET", "Azure", "SQL Server", "Microservices"],
    "totalSkillsIdentified": 8
  }
}
```

#### 3. Match Skills to Job
**Endpoint:** `POST /api/ai/match-skills-to-job`

**Request Body:**
```json
{
  "candidateSkills": ["C#", "ASP.NET Core", "SQL Server", "React", "Git"],
  "jobDescription": "Senior Full Stack Developer position requiring C#, ASP.NET Core, React, SQL Server, Azure, Kubernetes..."
}
```

**Response:**
```json
{
  "success": true,
  "message": "Skills matched successfully",
  "data": {
    "matchedSkills": ["C#", "ASP.NET Core", "React", "SQL Server"],
    "missingRequiredSkills": ["Azure", "Microservices", "Docker"],
    "missingPreferredSkills": ["Kubernetes", "GraphQL", "System Design"],
    "matchPercentageRequired": 66.67,
    "matchPercentagePreferred": 0,
    "overallMatchPercentage": 78,
    "skillGaps": [
      {
        "skillName": "Kubernetes",
        "importance": "important",
        "estimatedLearningTimeWeeks": 8,
        "learningResources": ["Kubernetes.io official docs", "Udemy course", "Linux Academy"],
        "certificationPath": "CKA (Certified Kubernetes Administrator)"
      },
      {
        "skillName": "System Design",
        "importance": "critical",
        "estimatedLearningTimeWeeks": 12,
        "learningResources": ["Designing Data-Intensive Applications", "System Design Interview"],
        "certificationPath": "None standard"
      }
    ],
    "recommendedLearningPath": [
      "Week 1-2: Review system design fundamentals",
      "Week 3-6: Learn Kubernetes architecture",
      "Week 7-8: Work on Kubernetes side project",
      "Week 9-12: Practice system design interviews"
    ]
  }
}
```

### Database Integration

Job posting data is stored in the `JobPosting` entity:

```csharp
public class JobPosting : BaseEntity
{
    public int EmployerId { get; set; }
    public User Employer { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }  // Parsed here
    public string Company { get; set; }
    public string Location { get; set; }
    public string SalaryRange { get; set; }
    public string JobType { get; set; }
    public string ExperienceLevel { get; set; }
    public bool IsActive { get; set; }
    public bool IsRemote { get; set; }
    // Additional fields...
}
```

Job matching results are stored in `JobAnalysis`:

```csharp
public class JobAnalysis : BaseEntity
{
    public int UserId { get; set; }
    public int JobPostingId { get; set; }
    public int? ResumeId { get; set; }
    public int MatchPercentage { get; set; }
    public string SkillGapsJson { get; set; }      // Skill gaps stored as JSON
    public string RecommendedActionsJson { get; set; }
    public string StrengthsJson { get; set; }
    public DateTime AnalyzedAt { get; set; }
}
```

---

## Implementation Code Walkthrough

### 1. Service Interface (Application.Abstraction)

**File:** `/SmartCareerPath.Application.Abstraction/ServicesContracts/AI/IAIService.cs`

```csharp
public interface IAIService
{
    // ... existing methods ...

    #region CV Builder Features
    
    Task<GenerateCVResult> GenerateCVAsync(GenerateCVRequest request);
    Task<ImproveCVResult> ImproveCVAsync(ImproveCVRequest request);
    Task<ParseCVResult> ParseCVAsync(ParseCVRequest request);
    
    #endregion

    #region Job Description Parser Features
    
    Task<ParseJobDescriptionResult> ParseJobDescriptionAsync(ParseJobDescriptionRequest request);
    Task<ExtractJobSkillsResult> ExtractJobSkillsAsync(ExtractJobSkillsRequest request);
    Task<MatchSkillsToJobResult> MatchSkillsToJobAsync(MatchSkillsToJobRequest request);
    
    #endregion
}
```

### 2. Data Transfer Objects (DTOs)

**Files:**
- `/SmartCareerPath.Application.Abstraction/DTOs/AI/CVBuilderDto.cs`
- `/SmartCareerPath.Application.Abstraction/DTOs/AI/JobParserDto.cs`

These contain all request/response classes with proper nullable reference type handling.

### 3. Service Implementation (Infrastructure)

**File:** `/SmartCareerPath.Infrastructure.Persistence/Services/AI/AIService.cs`

Each method follows this pattern:

```csharp
public async Task<GenerateCVResult> GenerateCVAsync(GenerateCVRequest request)
{
    // Simulate AI processing time
    await Task.Delay(1500);

    // In production: Replace with real AI provider call
    // var aiResponse = await openAiClient.CreateChatCompletionAsync(prompt);

    var cvContent = $@"
{request.FullName}
...
";

    return new GenerateCVResult
    {
        CVContent = cvContent,
        CVHtmlFormat = GenerateCVHtml(cvContent, request),
        TemplateName = request.CVTemplate,
        DownloadUrl = "/cv/download/template-" + DateTime.UtcNow.Ticks,
        Success = true,
        Message = "CV generated successfully"
    };
}
```

### 4. Commands and Handlers (Application Layer)

**File:** `/SmartCareerPath.Application/Features/AI/AICommands.cs`

Example command structure:

```csharp
public class GenerateCVCommand : IRequest<BaseResponse<GenerateCVResult>>
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string CurrentRole { get; set; } = string.Empty;
    // ... other properties ...
}

public class GenerateCVCommandHandler : IRequestHandler<GenerateCVCommand, BaseResponse<GenerateCVResult>>
{
    private readonly IAIService _aiService;

    public async Task<BaseResponse<GenerateCVResult>> Handle(
        GenerateCVCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var cvRequest = new GenerateCVRequest
            {
                FullName = request.FullName,
                // ... map properties ...
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
```

### 5. API Endpoints (Controller)

**File:** `/SmartCareerPath.APIs/Controllers/AIController.cs`

```csharp
[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class AIController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IAIService _aiService;

    #region CV Builder Features

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

    [HttpPost("improve-cv/{resumeId}")]
    public async Task<IActionResult> ImproveCV(int resumeId, [FromBody] ImproveCVCommand command)
    {
        // ... similar implementation ...
    }

    [HttpPost("parse-cv/{resumeId}")]
    public async Task<IActionResult> ParseCV(int resumeId)
    {
        // ... similar implementation ...
    }

    #endregion

    #region Job Description Parser Features

    [HttpPost("parse-job-description")]
    public async Task<IActionResult> ParseJobDescription([FromBody] ParseJobDescriptionCommand command)
    {
        // ... implementation ...
    }

    [HttpPost("extract-job-skills")]
    public async Task<IActionResult> ExtractJobSkills([FromBody] ExtractJobSkillsCommand command)
    {
        // ... implementation ...
    }

    [HttpPost("match-skills-to-job")]
    public async Task<IActionResult> MatchSkillsToJob([FromBody] MatchSkillsToJobCommand command)
    {
        // ... implementation ...
    }

    #endregion
}
```

---

## Production Integration

### Real AI Provider Integration

To integrate with a real AI provider (OpenAI, Mistral, etc.), update the methods in `AIService.cs`:

```csharp
public async Task<GenerateCVResult> GenerateCVAsync(GenerateCVRequest request)
{
    // BEFORE (Mock):
    // await Task.Delay(1500);
    // return new GenerateCVResult { ... };

    // AFTER (Real AI):
    var prompt = $@"Generate a professional CV for:
Name: {request.FullName}
Role: {request.CurrentRole}
Experience: {request.ExperienceYears} years
Skills: {string.Join(", ", request.Skills)}
Template: {request.CVTemplate}";

    var response = await _openAiClient.CreateChatCompletionAsync(new CreateChatCompletionRequest
    {
        Model = "gpt-4",
        Messages = new List<ChatCompletionRequestMessage>
        {
            new ChatCompletionRequestSystemMessage("You are a professional CV writer."),
            new ChatCompletionRequestUserMessage(prompt)
        },
        Temperature = 0.7m,
        MaxTokens = 2000
    });

    var cvContent = response.Choices[0].Message.Content;

    return new GenerateCVResult
    {
        CVContent = cvContent,
        CVHtmlFormat = GenerateCVHtml(cvContent, request),
        TemplateName = request.CVTemplate,
        DownloadUrl = $"/cv/download/{Guid.NewGuid()}",
        Success = true,
        Message = "CV generated successfully"
    };
}
```

### Configuration

Add to `appsettings.json`:

```json
{
  "OpenAI": {
    "ApiKey": "your-api-key",
    "Organization": "your-org",
    "Model": "gpt-4",
    "Temperature": 0.7,
    "MaxTokens": 2000
  }
}
```

---

## Complete Request/Response Examples

### Example 1: Generate CV with Professional Template

**cURL Request:**
```bash
curl -X POST http://localhost:5164/api/ai/generate-cv \
  -H "Content-Type: application/json" \
  -d '{
    "fullName": "Jane Smith",
    "currentRole": "DevOps Engineer",
    "experienceYears": 4,
    "summary": "Experienced DevOps professional specializing in cloud infrastructure and CI/CD pipelines",
    "skills": ["Kubernetes", "Docker", "AWS", "Terraform", "Jenkins", "GitLab CI"],
    "cvTemplate": "modern"
  }'
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "CV generated successfully using modern template",
  "data": {
    "cvContent": "Jane Smith\n\nDevOps Engineer\n\nPROFESSIONAL SUMMARY\nExperienced DevOps professional...",
    "cvHtmlFormat": "<html>...",
    "templateName": "modern",
    "downloadUrl": "/cv/download/template-637123456789",
    "success": true
  },
  "errors": []
}
```

### Example 2: Match Skills to Job

**cURL Request:**
```bash
curl -X POST http://localhost:5164/api/ai/match-skills-to-job \
  -H "Content-Type: application/json" \
  -d '{
    "candidateSkills": ["Java", "Spring Boot", "PostgreSQL", "Docker", "AWS"],
    "jobDescription": "Backend Engineer needed with expertise in Java, Spring Framework, Microservices, Kubernetes, Docker, AWS experience required..."
  }'
```

**Response:**
```json
{
  "success": true,
  "message": "Skills matched successfully",
  "data": {
    "matchedSkills": ["Java", "Docker", "AWS"],
    "missingRequiredSkills": ["Spring Framework", "Microservices", "Kubernetes"],
    "overallMatchPercentage": 65,
    "skillGaps": [
      {
        "skillName": "Kubernetes",
        "importance": "critical",
        "estimatedLearningTimeWeeks": 10,
        "learningResources": ["Kubernetes.io docs", "Linux Academy"],
        "certificationPath": "CKA"
      }
    ],
    "recommendedLearningPath": ["Week 1-2: Microservices fundamentals", "Week 3-8: Kubernetes deep dive"]
  }
}
```

---

## Testing & Deployment

### Build & Run
```bash
# Build
dotnet build

# Run with watch mode
dotnet watch run

# Test endpoints via Swagger
# Navigate to: http://localhost:5164/swagger/index.html
```

### Test New Endpoints in Swagger

1. Go to `http://localhost:5164/swagger/index.html`
2. Expand "AI" section
3. Find new endpoints:
   - `POST /api/ai/generate-cv`
   - `POST /api/ai/improve-cv/{resumeId}`
   - `POST /api/ai/parse-cv/{resumeId}`
   - `POST /api/ai/parse-job-description`
   - `POST /api/ai/extract-job-skills`
   - `POST /api/ai/match-skills-to-job`

---

## Summary

Your SmartCareerPath now has complete CV Builder and Job Description Parser features:

✅ **CV Builder:**
- Generate CVs with multiple templates
- Improve existing CVs with AI suggestions
- Parse CVs to extract structured data

✅ **Job Parser:**
- Parse job descriptions for key information
- Extract and categorize required/preferred skills
- Match candidate skills to job requirements

✅ **All features:**
- Follow Onion Architecture
- Use MediatR for command handling
- Have mock implementations ready for real AI provider integration
- Include comprehensive error handling
- Return strongly-typed responses

🎯 **Next Steps:**
1. Integrate with real AI provider (OpenAI/Mistral)
2. Add unit tests for handlers and services
3. Add data persistence for parsed results
4. Create frontend integration layer
