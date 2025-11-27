# SmartCareerPath AI Controllers - Swagger Test Guide

The app is running at: **http://localhost:5164**
Swagger UI: **http://localhost:5164/swagger/index.html**

---

## AI Controller Endpoints - JSON Body Examples

### 1. **Analyze Resume**
**Endpoint:** `POST /api/ai/analyze-resume/{resumeId}`
**URL Example:** `/api/ai/analyze-resume/1`

**Request Body:**
```json
{
  "targetRole": "Senior Software Engineer"
}
```

**Notes:**
- `resumeId` goes in the URL path (replace `{resumeId}` with an actual ID like `1`)
- `targetRole` is optional but recommended for better analysis
- Example roles: "Software Engineer", "Data Scientist", "Product Manager"

---

### 2. **Extract Skills from Resume**
**Endpoint:** `POST /api/ai/extract-skills`

**Request Body:**
```json
{
  "resumeText": "Senior Software Engineer with 5 years of experience in C#, ASP.NET Core, React, and SQL Server. Expertise in cloud architecture with AWS and Azure. Proven track record of leading cross-functional teams."
}
```

**Notes:**
- `resumeText` is required
- Can be any length, paste actual resume text here
- Returns: Array of extracted skills

---

### 3. **Get Resume Suggestions**
**Endpoint:** `POST /api/ai/resume-suggestions`

**Request Body:**
```json
{
  "resumeText": "I worked as a developer for 3 years. I know programming languages and databases. I have good communication skills."
}
```

**Notes:**
- `resumeText` is required
- AI analyzes and returns improvement suggestions
- Returns suggestions with categories and priorities (High, Medium, Low)

---

### 4. **Calculate Job Match**
**Endpoint:** `POST /api/ai/job-match/{jobId}`
**URL Example:** `/api/ai/job-match/1`

**Request Body:**
```json
{
  "resumeId": 1
}
```

**Notes:**
- `jobId` goes in the URL path (replace `{jobId}` with actual ID)
- `resumeId` is optional but recommended (required for matching)
- Returns: Job match score and skill gap analysis

---

### 5. **Generate Cover Letter**
**Endpoint:** `POST /api/ai/generate-cover-letter/{jobId}`
**URL Example:** `/api/ai/generate-cover-letter/1`

**Request Body:**
```json
{
  "resumeId": 1
}
```

**Notes:**
- `jobId` goes in the URL path
- `resumeId` is optional (needed for generating tailored cover letter)
- Returns: Generated cover letter text

---

### 6. **Generate Interview Questions**
**Endpoint:** `POST /api/ai/generate-interview-questions`

**Request Body:**
```json
{
  "jobDescription": "Senior Full Stack Developer position responsible for designing and implementing scalable web applications using C#, ASP.NET Core, React, and SQL Server. Must have 5+ years experience with cloud platforms.",
  "skillsRequired": ["C#", "ASP.NET Core", "React", "SQL Server", "AWS"]
}
```

**Notes:**
- `jobDescription` is required - paste the full job description
- `skillsRequired` is an array of strings - list all required skills
- Returns: Array of interview questions tailored to the role

---

### 7. **Analyze Interview Answer**
**Endpoint:** `POST /api/ai/analyze-interview-answer`

**Request Body:**
```json
{
  "question": "What is your experience with ASP.NET Core?",
  "answer": "I have 5 years of production experience building scalable APIs with ASP.NET Core. I've implemented complex entity framework ORM queries, managed database migrations, and built RESTful services. I'm proficient with dependency injection, middleware, and async/await patterns.",
  "skillRequired": "ASP.NET Core"
}
```

**Notes:**
- All fields are required
- `question` - the interview question asked
- `answer` - the candidate's response
- `skillRequired` - the skill being evaluated
- Returns: Analysis with score and feedback

---

### 8. **Get Career Recommendations** ⭐ (GET Request)
**Endpoint:** `GET /api/ai/career-recommendations`
**Optional Query Parameter:** `desiredField=Cloud%20Architecture`

**No Request Body Needed** (GET request)

**Notes:**
- This is a `GET` request - no JSON body required
- Optional query parameter: `desiredField` (e.g., "Cloud Architecture", "Data Science")
- Returns: Career path recommendations, skills to learn, certifications

---

### 9. **Identify Skill Gaps**
**Endpoint:** `POST /api/ai/identify-skill-gaps`

**Request Body:**
```json
{
  "currentSkills": ["C#", "ASP.NET Core", "SQL Server", "Git"],
  "targetRole": "Senior Backend Architect"
}
```

**Notes:**
- Both fields are required
- `currentSkills` - array of skills the user currently has
- `targetRole` - the desired position/role
- Returns: Skill gaps with proficiency levels and learning resources

---

### 10. **Send Custom Prompt** (Admin Only)
**Endpoint:** `POST /api/ai/prompt`
**Authorization:** Requires Admin role

**Request Body:**
```json
{
  "prompt": "What are the latest trends in C# development in 2024?",
  "systemPrompt": "You are an expert software developer. Provide detailed technical insights.",
  "maxTokens": 1000,
  "temperature": 0.7
}
```

**Notes:**
- `prompt` is required - your question/prompt
- `systemPrompt` is optional - system context (default: empty)
- `maxTokens` is optional - response length (default: 1000)
- `temperature` is optional - creativity level 0.0-1.0 (default: 0.7)
- **Currently disabled** - requires authentication and admin role

---

## Quick Testing in Swagger

1. Go to **http://localhost:5164/swagger/index.html**
2. Find the endpoint you want to test
3. Click "Try it out"
4. Paste the example JSON from above into the request body
5. Replace path parameters (like `{resumeId}`) with actual values
6. Click "Execute"
7. View the response

## Testing Tips

- **Start Simple:** Test `extract-skills` first (easiest to see results)
- **Path Parameters:** Always replace `{jobId}` or `{resumeId}` with actual numbers
- **GET vs POST:** Career recommendations is GET (no body), others are POST
- **Arrays:** Skills are arrays `["skill1", "skill2"]`
- **Strings:** Text fields use quotes: `"text value"`

