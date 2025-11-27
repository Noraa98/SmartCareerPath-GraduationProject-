````markdown
# AI Interview — Documentation

## Overview
This document describes the AI Interview feature in the SmartCareerPath project. It's written in the same style and level of detail as `AI_INTERVIEW_FLOW_COMPLETE.md` and focuses on the interview endpoints, data flow, DTO contracts, testing instructions (including OpenRouter usage), and troubleshooting.

---

## 🧭 Purpose
The AI Interview feature enables the application to:
- Generate interview questions for a specific role and interview type.
- Accept and store user answers per interview session.
- Analyze each answer with an AI backend and return structured feedback, scores, strengths, and improvement suggestions.

The implementation follows Onion Architecture and uses MediatR for commands and handlers. The AI integration is implemented in `AIService` in the Infrastructure layer and currently supports real LLM calls (OpenRouter) with safe mock fallbacks.

---

## 🏗️ Architecture (Quick Map)

```
API Layer
  `- /SmartCareerPath.APIs/Controllers/AIController.cs
Application Layer
  `- /SmartCareerPath.Application/Features/AI/AICommands.cs (MediatR commands & handlers)
Domain / Abstraction
  `- InterviewSession, InterviewQuestion entities, `IAIService` interface
Infrastructure
  `- /SmartCareerPath.Infrastructure.Persistence/Services/AI/AIService.cs (AI implementation)
Data Layer
  `- SQL Server tables: InterviewSession, InterviewQuestion
```

---

## Endpoints (Primary)

- `POST /api/ai/generate-interview-questions`
  - Purpose: Generate a list of interview questions for a role and interview type.
  - Request body (JSON):

```json
{
  "role": "Data Scientist",
  "interviewType": "Technical",
  "questionCount": 5
}
```

  - Response: `BaseResponse<List<string>>` (success, message, data = list of questions)

- `POST /api/ai/analyze-interview-answer`
  - Purpose: Analyze a user's answer to a specific interview question and return structured feedback.
  - Request body (JSON):

```json
{
  "sessionId": 1,
  "questionId": 10,
  "userAnswer": "I led the migration of our monolith to microservices..."
}
```

  - Response: `BaseResponse<InterviewAnalysisResult>` (contains `OverallScore`, `Feedback`, `Strengths`, `Improvements`, etc.)

---

## DTOs / Contracts (important fields)

- GenerateInterviewQuestionsCommand
  - `string? Role`
  - `string? InterviewType`
  - `int QuestionCount`

- AnalyzeInterviewAnswerCommand
  - `int SessionId`
  - `int QuestionId`
  - `string? UserAnswer`

- InterviewAnalysisRequest (sent to `IAIService`)
  - `string Question`
  - `string UserAnswer`
  - `string InterviewType`

- InterviewAnalysisResult (returned by `IAIService`)
  - `decimal OverallScore`  // 0-10 scale
  - `string Feedback`
  - `List<string> Strengths`
  - `List<string> Improvements`

---

## Flow: Generate → Ask → Answer → Analyze (concise)

1. Frontend calls `POST /api/ai/generate-interview-questions`.
2. `AIController` sends `GenerateInterviewQuestionsCommand` to MediatR.
3. Command handler calls `IAIService.GenerateInterviewQuestionsAsync(...)`.
   - If `OPENROUTER_API_KEY` is set, service attempts an OpenRouter call using `CallOpenRouterAsync(...)`.
   - If the call fails or returns unparsable content, the method falls back to a mock question list.
4. Frontend displays questions and creates an `InterviewSession` + `InterviewQuestion` rows.
5. For each answer, frontend posts `AnalyzeInterviewAnswerCommand`.
6. Handler loads the `InterviewSession` and `InterviewQuestion` from DB and calls `IAIService.AnalyzeInterviewAnswerAsync(...)`.
7. `IAIService` attempts OpenRouter analysis; if successful, returns structured `InterviewAnalysisResult`. Otherwise, returns a mock analysis.
8. Handler updates the `InterviewQuestion` with `UserAnswer`, `Score`, `FeedbackJson`, `AnsweredAt`, persists, and returns the analysis to frontend.

---

## AI Integration Details (OpenRouter)

- Endpoint used: `https://openrouter.ai/api/v1/chat/completions`
- Model configured in service: `mistralai/mistral-7b-instruct` (call from `CallOpenRouterAsync`)
- API Key: provided via env var `OPENROUTER_API_KEY` (set before starting the API)
- Behavior: The service builds a `system` prompt that explains the expected JSON schema and a `user` prompt containing the question/description. The desired pattern is to ask the model to "Return ONLY the JSON object." The code attempts to deserialize the reply into the expected DTO and falls back to mock data if parsing fails.

Notes:
- Because LLMs may include extra text, `AIService` contains helpers that try to extract an embedded JSON object or array from the model text before deserializing.
- If you want to run without an LLM, simply do not set `OPENROUTER_API_KEY` — the service will use mock fallbacks.

---

## Local Testing Instructions

1. Export your OpenRouter API key (optional; required for real LLM responses):

```bash
export OPENROUTER_API_KEY="your_openrouter_api_key_here"
```

2. Build and run the API from repository root:

```bash
cd /Users/abdelrahmanali/Projects/SmartCareerPath-GraduationProject-
# build (optional)
dotnet build SmartCareerPath.sln
# run the API project
dotnet run --project SmartCareerPath.APIs/SmartCareerPath.APIs.csproj
```

3. Example curl to generate questions (replace host/port if different):

```bash
curl -X POST http://localhost:5164/api/ai/generate-interview-questions \
  -H "Content-Type: application/json" \
  -d '{"role":"Data Scientist","interviewType":"Technical","questionCount":5}'
```

4. Example curl to analyze an answer:

```bash
curl -X POST http://localhost:5164/api/ai/analyze-interview-answer \
  -H "Content-Type: application/json" \
  -d '{"sessionId":1,"questionId":1,"userAnswer":"I built several ML models..."}'
```

If the analysis step fails with "Interview session not found", either:
- Create a session + questions first using the generation flow and persist them, or
- Reapply the ad-hoc analyze fallback in the application handler so `AnalyzeInterviewAnswer` will accept a direct `Question` + `InterviewType` payload (I can reapply this change on request).

---

## Example: Request/Response (Generate)

Request:

```json
{ "role": "Data Scientist", "interviewType": "Technical", "questionCount": 3 }
```

Sample successful response (when LLM or mock returns data):

```json
{
  "success": true,
  "message": "Questions generated successfully",
  "data": [
    "Explain the difference between supervised and unsupervised learning.",
    "How would you handle missing values in a dataset?",
    "Describe a machine learning project you led and its outcome."
  ],
  "errors": []
}
```

---

## Example: Request/Response (Analyze)

Request body sent to API/Handler: see `AnalyzeInterviewAnswerCommand` example above.

Sample `InterviewAnalysisResult`:

```json
{
  "overallScore": 7.5,
  "feedback": "Good structure, clear examples; add more metrics and results.",
  "strengths": ["Clear explanation", "Relevant examples"],
  "improvements": ["Mention evaluation metrics", "Add performance numbers"]
}
```

---

## Key Files (where to look / change)

- `SmartCareerPath.APIs/Controllers/AIController.cs` — API endpoints.
- `SmartCareerPath.Application/Features/AI/AICommands.cs` — MediatR commands & handlers.
- `SmartCareerPath.Infrastructure.Persistence/Services/AI/AIService.cs` — AI logic including OpenRouter calls and fallback mocks.
- `SmartCareerPath.Application.Abstraction/DTOs/AI` — DTO shapes used for parsing and return types.

---

## Troubleshooting & Recommendations

- If generate or analyze returns mock data unexpectedly:
  - Confirm `OPENROUTER_API_KEY` is exported in the shell used to start the API.
  - Check API logs for OpenRouter call failures or parse exceptions.

- Add a feature toggle to `appsettings.*.json` (e.g., `UseRealAI: true|false`) to avoid accidental LLM calls in CI/dev.

- Add structured logging around `CallOpenRouterAsync(...)` to capture request/response and parsing errors.

- Add basic retry + exponential backoff for OpenRouter requests to handle transient network errors.

- If you want ad-hoc analysis (without persisted session/question), I can reapply the previously proposed change in `AICommands.cs` so handlers accept `Question` + `InterviewType` and skip DB lookups when those are supplied.

---

## Next Steps (suggested)

- Add a feature-flag to toggle LLM usage in `appsettings`.
- Add unit tests for `AIService` parsing helpers (JSON extraction and DTO deserialization).
- Add integration tests that run against a recorded OpenRouter response (or mock HTTP handler) so CI doesn't require a live key.

---

## Summary
This document is a compact, same-style companion to `AI_INTERVIEW_FLOW_COMPLETE.md` focused on practical usage, testing, and the OpenRouter-backed AI integration. If you'd like, I can:
- Reapply the `AnalyzeInterviewAnswer` ad-hoc fallback in `AICommands.cs` and restart the API,
- Add the `UseRealAI` toggle and basic logging, or
- Generate a Postman collection with sample requests.

``` ````