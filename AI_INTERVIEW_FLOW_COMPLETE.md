# AI Interview System - Complete Flow Documentation

## Overview
The AI Interview system in your SmartCareerPath project follows a **CQRS (Command Query Responsibility Segregation)** pattern with MediatR. It's organized in layers following **Onion Architecture**.

---

## 🏗️ Architecture Layers

```
┌─────────────────────────────────────────────────────────┐
│  API Layer (Controllers)                                 │
│  /SmartCareerPath.APIs/Controllers/AIController.cs      │
└────────────────────┬────────────────────────────────────┘
                     ↓
┌─────────────────────────────────────────────────────────┐
│  Application Layer (Commands/Handlers)                   │
│  /SmartCareerPath.Application/Features/AI/AICommands.cs │
└────────────────────┬────────────────────────────────────┘
                     ↓
┌─────────────────────────────────────────────────────────┐
│  Domain Layer (Contracts/Entities)                       │
│  - IUnitOfWork (repository pattern)                      │
│  - Domain Entities (InterviewSession, InterviewQuestion) │
└────────────────────┬────────────────────────────────────┘
                     ↓
┌─────────────────────────────────────────────────────────┐
│  Infrastructure Layer (AI Service Implementation)        │
│  /SmartCareerPath.Infrastructure.Persistence/           │
│    Services/AI/AIService.cs                             │
└────────────────────┬────────────────────────────────────┘
                     ↓
┌─────────────────────────────────────────────────────────┐
│  Data Layer (Database)                                   │
│  SQL Server (InterviewSession, InterviewQuestion tables) │
└─────────────────────────────────────────────────────────┘
```

---

## 📋 Interview AI System - Complete Flow

### **STEP 1: Generate Interview Questions**

#### 1.1 User Request (Frontend → API)
```
POST /api/ai/generate-interview-questions
Content-Type: application/json

{
  "role": "Senior Full Stack Developer",
  "interviewType": "Technical",
  "questionCount": 5
}
```

#### 1.2 API Controller Layer
**File:** `/SmartCareerPath.APIs/Controllers/AIController.cs`

```csharp
[HttpPost("generate-interview-questions")]
public async Task<IActionResult> GenerateInterviewQuestions(
    [FromBody] GenerateInterviewQuestionsCommand command)
{
    var result = await _mediator.Send(command);
    
    if (!result.Success)
        return BadRequest(result);
    
    return Ok(result);
}
```

**Flow:**
1. Receives `GenerateInterviewQuestionsCommand` from request body
2. Uses MediatR to send command to handler
3. Returns response with generated questions

#### 1.3 MediatR Handler (Application Layer)
**File:** `/SmartCareerPath.Application/Features/AI/AICommands.cs`

```csharp
public class GenerateInterviewQuestionsCommand : IRequest<BaseResponse<List<string>>>
{
    public string? Role { get; set; }              // "Senior Full Stack Developer"
    public string? InterviewType { get; set; }     // "Technical"
    public int QuestionCount { get; set; } = 5;   // Number of questions
}

public class GenerateInterviewQuestionsCommandHandler : 
    IRequestHandler<GenerateInterviewQuestionsCommand, BaseResponse<List<string>>>
{
    private readonly IAIService _aiService;
    
    public async Task<BaseResponse<List<string>>> Handle(
        GenerateInterviewQuestionsCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Call AI Service to generate questions
            var questions = await _aiService.GenerateInterviewQuestionsAsync(
                request.Role ?? "Software Engineer",
                request.InterviewType ?? "Technical",
                request.QuestionCount);
            
            if (questions == null || !questions.Any())
                return BaseResponse<List<string>>.FailureResult(
                    "Failed to generate questions");
            
            return BaseResponse<List<string>>.SuccessResult(
                questions, 
                "Questions generated successfully");
        }
        catch (Exception ex)
        {
            return BaseResponse<List<string>>.FailureResult(
                $"Generation failed: {ex.Message}");
        }
    }
}
```

#### 1.4 AI Service Layer (Infrastructure)
**File:** `/SmartCareerPath.Infrastructure.Persistence/Services/AI/AIService.cs`

```csharp
public async Task<List<string>> GenerateInterviewQuestionsAsync(
    string role, 
    string interviewType, 
    int questionCount)
{
    await Task.Delay(600); // Simulate AI processing time
    
    // In production, would call actual AI provider (OpenAI, Mistral, etc.)
    var allQuestions = new List<string>
    {
        "Can you describe your experience with the key technologies...",
        "Tell us about a complex project you've worked on...",
        "How do you stay updated with the latest trends...",
        "Describe a situation where you had to work with difficult team member...",
        "What are your strengths and how have they contributed...",
        // ... more questions
    };
    
    // Return requested number of questions
    return allQuestions.Take(questionCount).ToList();
}
```

#### 1.5 Response Sent to Frontend

```json
{
  "success": true,
  "message": "Questions generated successfully",
  "data": [
    "Can you describe your experience with the key technologies...",
    "Tell us about a complex project you've worked on...",
    "How do you stay updated with the latest trends...",
    "Describe a situation where you had to work with difficult team member...",
    "What are your strengths and how have they contributed..."
  ],
  "errors": []
}
```

---

### **STEP 2: User Answers Questions**

#### 2.1 Frontend Workflow
The frontend receives the questions and:
1. **Displays each question** to the user one by one
2. **Records the user's answer** (text or audio)
3. **Stores in database** via API call

#### 2.2 Database Storage
**Entities Used:**
- `InterviewSession` - Represents an interview session
- `InterviewQuestion` - Individual questions with answers

```
InterviewSession (1 session per interview)
├── Id: 1
├── UserId: 5
├── InterviewType: "Technical"
├── Status: "In Progress"
└── CreatedAt: 2025-11-17

InterviewQuestion (Multiple questions per session)
├── Id: 1
├── InterviewSessionId: 1
├── QuestionText: "Can you describe your experience..."
├── UserAnswer: "[User's answer will be stored here]"
├── Score: null (until analyzed)
├── FeedbackJson: null (until analyzed)
├── OrderIndex: 1
├── AskedAt: 2025-11-17
├── AnsweredAt: null (until answered)
└── TimeSpentSeconds: null
```

---

### **STEP 3: Analyze Interview Answer**

#### 3.1 User Submits Answer (Frontend → API)
```
POST /api/ai/analyze-interview-answer
Content-Type: application/json

{
  "sessionId": 1,
  "questionId": 1,
  "userAnswer": "I have 5 years of production experience building scalable APIs with ASP.NET Core..."
}
```

#### 3.2 API Controller Layer
**File:** `/SmartCareerPath.APIs/Controllers/AIController.cs`

```csharp
[HttpPost("analyze-interview-answer")]
public async Task<IActionResult> AnalyzeInterviewAnswer(
    [FromBody] AnalyzeInterviewAnswerCommand command)
{
    var result = await _mediator.Send(command);
    
    if (!result.Success)
        return BadRequest(result);
    
    return Ok(result);
}
```

#### 3.3 MediatR Handler (Application Layer)
**File:** `/SmartCareerPath.Application/Features/AI/AICommands.cs`

```csharp
public class AnalyzeInterviewAnswerCommand : IRequest<BaseResponse<InterviewAnalysisResult>>
{
    public int SessionId { get; set; }      // Which interview session
    public int QuestionId { get; set; }     // Which question
    public string? UserAnswer { get; set; } // User's answer to analyze
}

public class AnalyzeInterviewAnswerCommandHandler : 
    IRequestHandler<AnalyzeInterviewAnswerCommand, BaseResponse<InterviewAnalysisResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAIService _aiService;
    
    public async Task<BaseResponse<InterviewAnalysisResult>> Handle(
        AnalyzeInterviewAnswerCommand request,
        CancellationToken cancellationToken)
    {
        // STEP 1: Get interview session from database
        var session = await _unitOfWork.Repository<InterviewSession>()
            .FirstOrDefaultAsync(s => s.Id == request.SessionId);
        
        if (session == null)
            return BaseResponse<InterviewAnalysisResult>
                .FailureResult("Interview session not found");
        
        // STEP 2: Get the question from database
        var question = await _unitOfWork.Repository<InterviewQuestion>()
            .FirstOrDefaultAsync(q => q.Id == request.QuestionId);
        
        if (question == null)
            return BaseResponse<InterviewAnalysisResult>
                .FailureResult("Question not found");
        
        try
        {
            // STEP 3: Send to AI for analysis
            var analysisResult = await _aiService.AnalyzeInterviewAnswerAsync(
                new InterviewAnalysisRequest
                {
                    Question = question.QuestionText,           // The question asked
                    UserAnswer = request.UserAnswer,            // User's response
                    InterviewType = session.InterviewType       // Type of interview
                });
            
            if (analysisResult == null)
                return BaseResponse<InterviewAnalysisResult>
                    .FailureResult("AI analysis failed");
            
            // STEP 4: Update question in database with analysis results
            question.UserAnswer = request.UserAnswer;
            question.Score = (int?)analysisResult.OverallScore;
            question.FeedbackJson = System.Text.Json.JsonSerializer.Serialize(analysisResult);
            question.AnsweredAt = DateTime.UtcNow;
            
            // STEP 5: Save to database
            await _unitOfWork.Repository<InterviewQuestion>().UpdateAsync(question);
            await _unitOfWork.SaveChangesAsync();
            
            // STEP 6: Return analysis result to frontend
            return BaseResponse<InterviewAnalysisResult>.SuccessResult(
                analysisResult,
                "Answer analyzed successfully");
        }
        catch (Exception ex)
        {
            return BaseResponse<InterviewAnalysisResult>
                .FailureResult($"Analysis failed: {ex.Message}");
        }
    }
}
```

#### 3.4 AI Service Analysis (Infrastructure)
**File:** `/SmartCareerPath.Infrastructure.Persistence/Services/AI/AIService.cs`

```csharp
public async Task<InterviewAnalysisResult> AnalyzeInterviewAnswerAsync(
    InterviewAnalysisRequest request)
{
    await Task.Delay(500); // Simulate AI processing
    
    // In production, would send to real AI provider for analysis
    return new InterviewAnalysisResult
    {
        OverallScore = 8.2m,  // Score out of 10
        Feedback = "Excellent answer demonstrating strong understanding of the topic",
        Strengths = new List<string>
        {
            "Clear explanation of concepts",
            "Relevant examples provided",
            "Shows practical experience"
        },
        Improvements = new List<string>
        {
            "Could mention specific performance metrics",
            "Consider adding more recent technologies"
        }
    };
}
```

#### 3.5 Response to Frontend

```json
{
  "success": true,
  "message": "Answer analyzed successfully",
  "data": {
    "overallScore": 8.2,
    "feedback": "Excellent answer demonstrating strong understanding of the topic",
    "strengths": [
      "Clear explanation of concepts",
      "Relevant examples provided",
      "Shows practical experience"
    ],
    "improvements": [
      "Could mention specific performance metrics",
      "Consider adding more recent technologies"
    ]
  },
  "errors": []
}
```

#### 3.6 Database Updated
The `InterviewQuestion` record is updated:

```
InterviewQuestion After Analysis
├── Id: 1
├── QuestionText: "..."
├── UserAnswer: "I have 5 years of production experience..."  ✅ STORED
├── Score: 8                                                    ✅ STORED
├── FeedbackJson: "{...feedback...}"                           ✅ STORED
├── AnsweredAt: "2025-11-17 10:30:45"                         ✅ STORED
└── TimeSpentSeconds: 125
```

---

## 📊 Complete Interview Flow Diagram

```
┌──────────────────────────────────────────────────────────────────────┐
│                    FRONTEND (User Interface)                          │
│                                                                       │
│  1. User clicks "Start Interview"                                    │
│     ↓                                                                 │
│  2. Display Question 1/5                                             │
│     ↓                                                                 │
│  3. User types answer                                                │
│     ↓                                                                 │
│  4. User clicks "Submit Answer"                                      │
│     ↓                                                                 │
│  5. Display AI Feedback & Score                                      │
│     ↓                                                                 │
│  6. Load Question 2/5... (Repeat steps 2-5)                         │
└──────────────────┬───────────────────────────────────────────────────┘
                   │
                   ↓
    ┌──────────────────────────────────┐
    │  REQUEST 1: Generate Questions   │
    │  POST /api/ai/generate-interview- │
    │  questions                        │
    │                                  │
    │  Body: {                         │
    │    role: "Senior Dev",           │
    │    interviewType: "Technical",   │
    │    questionCount: 5              │
    │  }                               │
    │                                  │
    │  Returns: 5 questions             │
    └───────────────────────┬──────────┘
                            │
    ┌───────────────────────↓──────────┐
    │     REQUEST 2-6: Submit Answers  │
    │     POST /api/ai/analyze-        │
    │     interview-answer              │
    │                                  │
    │     Body: {                      │
    │       sessionId: 1,              │
    │       questionId: 1,             │
    │       userAnswer: "..."          │
    │     }                            │
    │                                  │
    │     Returns: Analysis & Score     │
    │                                  │
    │     (Repeat 5 times for 5 Qs)    │
    └───────────────────────┬──────────┘
                            │
            ┌───────────────↓───────────────┐
            │  API LAYER                    │
            │  AIController.cs              │
            │  - GenerateInterviewQuestions │
            │  - AnalyzeInterviewAnswer     │
            └───────────────┬───────────────┘
                            │
            ┌───────────────↓───────────────────────┐
            │  APPLICATION LAYER (MediatR Handlers) │
            │  GenerateInterviewQuestionsCommand    │
            │  AnalyzeInterviewAnswerCommand        │
            └───────────────┬───────────────────────┘
                            │
            ┌───────────────↓────────────────────┐
            │  INFRASTRUCTURE LAYER              │
            │  AIService                         │
            │  - GenerateInterviewQuestionsAsync │
            │  - AnalyzeInterviewAnswerAsync     │
            │                                    │
            │  (Would call OpenAI/Mistral API)   │
            └───────────────┬────────────────────┘
                            │
            ┌───────────────↓──────────────────┐
            │  DATA LAYER (SQL Server)         │
            │                                  │
            │  InterviewSession table:         │
            │  ├── Id, UserId, Type, Status   │
            │                                  │
            │  InterviewQuestion table:        │
            │  ├── Id, SessionId               │
            │  ├── QuestionText                │
            │  ├── UserAnswer ✅               │
            │  ├── Score ✅                    │
            │  ├── FeedbackJson ✅             │
            │  ├── AnsweredAt ✅               │
            │  └── ...                         │
            └──────────────────────────────────┘
```

---

## 🔄 Data Flow for Single Answer Analysis

```
USER SUBMITS ANSWER
        ↓
┌─────────────────────────────────────────────┐
│ Frontend sends:                              │
│ {                                            │
│   sessionId: 1                               │
│   questionId: 1                              │
│   userAnswer: "I have 5 years exp..."       │
│ }                                            │
└────────────────┬────────────────────────────┘
                 ↓
┌─────────────────────────────────────────────┐
│ AIController receives request                │
│ Creates AnalyzeInterviewAnswerCommand        │
│ Sends via MediatR                            │
└────────────────┬────────────────────────────┘
                 ↓
┌─────────────────────────────────────────────┐
│ Handler Executes:                            │
│ 1. Query: Get InterviewSession (id=1)       │
│ 2. Query: Get InterviewQuestion (id=1)      │
│ 3. Validate: Both exist                     │
│ 4. Call: AIService.AnalyzeInterviewAnswerAsync
└────────────────┬────────────────────────────┘
                 ↓
┌─────────────────────────────────────────────┐
│ AIService.AnalyzeInterviewAnswerAsync():    │
│ 1. Receive question text, answer, type      │
│ 2. Send to AI provider (OpenAI/Mistral)     │
│ 3. Get back score, feedback, strengths      │
│ 4. Return InterviewAnalysisResult            │
└────────────────┬────────────────────────────┘
                 ↓
┌─────────────────────────────────────────────┐
│ Handler Updates Database:                    │
│ UPDATE InterviewQuestion SET                │
│   UserAnswer = '...',                        │
│   Score = 8,                                │
│   FeedbackJson = '{...}',                   │
│   AnsweredAt = NOW()                        │
│ WHERE Id = 1                                │
└────────────────┬────────────────────────────┘
                 ↓
┌─────────────────────────────────────────────┐
│ Handler Returns Response:                    │
│ {                                            │
│   success: true,                             │
│   data: {                                    │
│     overallScore: 8.2,                       │
│     feedback: "Excellent...",               │
│     strengths: [...],                        │
│     improvements: [...]                      │
│   }                                          │
│ }                                            │
└────────────────┬────────────────────────────┘
                 ↓
         USER SEES FEEDBACK
         & MOVES TO NEXT Q
```

---

## 📁 Key Files & Their Roles

| File | Layer | Purpose |
|------|-------|---------|
| `AIController.cs` | API | HTTP endpoints for interview operations |
| `AICommands.cs` | Application | MediatR commands & handlers for interview logic |
| `AIService.cs` | Infrastructure | AI service implementation (mock in current version) |
| `InterviewSession.cs` | Domain | Entity representing an interview session |
| `InterviewQuestion.cs` | Domain | Entity representing individual questions |
| `IAIService.cs` | Domain (Abstraction) | Contract/interface for AI operations |

---

## 🚀 Production Integration

**Current State:** Mock implementation (simulates AI with hardcoded responses)

**To Integrate Real AI:**

Replace the mock implementation in `AIService.cs` with actual API calls:

```csharp
// Instead of:
public async Task<InterviewAnalysisResult> AnalyzeInterviewAnswerAsync(
    InterviewAnalysisRequest request)
{
    await Task.Delay(500);
    return new InterviewAnalysisResult { /* mock data */ };
}

// Would become:
public async Task<InterviewAnalysisResult> AnalyzeInterviewAnswerAsync(
    InterviewAnalysisRequest request)
{
    // Call OpenAI or Mistral API
    var response = await _httpClient.PostAsync(
        "https://api.openai.com/v1/chat/completions",
        new StringContent(JsonSerializer.Serialize(new {
            model = "gpt-4",
            messages = new[] { ... }
        }))
    );
    
    // Parse response and return analysis
    return ParseAIResponse(response);
}
```

---

## 📝 Summary

**The AI Interview Flow:**
1. **Generate**: Frontend requests questions → Controller → Handler → AIService → Returns 5 questions
2. **Display**: Frontend shows questions to user
3. **Answer**: User types answer → Submits → Goes to next question
4. **Analyze**: Each answer → Controller → Handler → AIService → Analyzes → Stores in DB
5. **Feedback**: AI analysis returned to frontend → User sees score & feedback
6. **Repeat**: Steps 2-5 for all questions
7. **Complete**: After all answers, user sees complete results/recommendations

