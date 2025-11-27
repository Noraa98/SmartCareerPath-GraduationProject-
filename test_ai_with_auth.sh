#!/bin/zsh

# Color codes for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

BASE_URL="http://localhost:5164/api"
TIMESTAMP=$(date +%s%N)
TEST_EMAIL="testuser_${TIMESTAMP}@test.com"
TEST_PASSWORD="TestPassword123!"

echo "${BLUE}===============================================${NC}"
echo "${BLUE}SmartCareerPath AI Controller Full Test Suite${NC}"
echo "${BLUE}===============================================${NC}"
echo ""

# Step 1: Register a test user
echo "${CYAN}[STEP 1] Registering test user...${NC}"
echo "Email: $TEST_EMAIL"
echo "Password: $TEST_PASSWORD"
echo ""

REGISTER_RESPONSE=$(curl -s -X POST "$BASE_URL/auth/register" \
  -H "Content-Type: application/json" \
  -d "{
    \"email\": \"$TEST_EMAIL\",
    \"password\": \"$TEST_PASSWORD\",
    \"firstName\": \"Test\",
    \"lastName\": \"User\"
  }")

echo "Response: $REGISTER_RESPONSE"
echo ""

# Step 2: Login to get JWT token
echo "${CYAN}[STEP 2] Logging in to get JWT token...${NC}"
LOGIN_RESPONSE=$(curl -s -X POST "$BASE_URL/auth/login" \
  -H "Content-Type: application/json" \
  -d "{
    \"email\": \"$TEST_EMAIL\",
    \"password\": \"$TEST_PASSWORD\"
  }")

echo "Login Response: $LOGIN_RESPONSE"

# Extract token from response
TOKEN=$(echo "$LOGIN_RESPONSE" | jq -r '.data.token // .accessToken // empty' 2>/dev/null)

if [ -z "$TOKEN" ]; then
  echo "${RED}Error: Could not extract JWT token from login response${NC}"
  echo "Trying alternative extraction methods..."
  TOKEN=$(echo "$LOGIN_RESPONSE" | jq -r '.token // empty' 2>/dev/null)
fi

if [ -z "$TOKEN" ]; then
  echo "${RED}Error: Failed to obtain JWT token${NC}"
  echo "Full response: $LOGIN_RESPONSE"
  exit 1
fi

echo "${GREEN}✓ Successfully obtained JWT token${NC}"
echo "Token (first 50 chars): ${TOKEN:0:50}..."
echo ""

# Now test AI endpoints with the token
AUTH_HEADER="Authorization: Bearer $TOKEN"

echo "${CYAN}[STEP 3] Testing AI Endpoints with authentication...${NC}"
echo ""

# Test 1: Extract Skills (Simplest test, only needs text)
echo "${BLUE}Test 1: Extract Skills from Resume${NC}"
RESPONSE=$(curl -s -X POST "$BASE_URL/ai/extract-skills" \
  -H "Content-Type: application/json" \
  -H "$AUTH_HEADER" \
  -d '{
    "resumeText": "Senior Software Engineer with 5 years of experience in C#, ASP.NET Core, React, and SQL Server. Expertise in cloud architecture with AWS and Azure."
  }')

HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" -X POST "$BASE_URL/ai/extract-skills" \
  -H "Content-Type: application/json" \
  -H "$AUTH_HEADER" \
  -d '{"resumeText": "Senior Software Engineer with 5 years of experience in C#, ASP.NET Core, React, and SQL Server."}')

echo "HTTP Status: ${CYAN}$HTTP_CODE${NC}"
echo "Response: "
echo "$RESPONSE" | jq . 2>/dev/null || echo "$RESPONSE"
echo ""

# Test 2: Resume Suggestions
echo "${BLUE}Test 2: Get Resume Suggestions${NC}"
RESPONSE=$(curl -s -X POST "$BASE_URL/ai/resume-suggestions" \
  -H "Content-Type: application/json" \
  -H "$AUTH_HEADER" \
  -d '{
    "resumeText": "I worked as a developer for 3 years. I know programming languages."
  }')

HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" -X POST "$BASE_URL/ai/resume-suggestions" \
  -H "Content-Type: application/json" \
  -H "$AUTH_HEADER" \
  -d '{"resumeText": "test"}')

echo "HTTP Status: ${CYAN}$HTTP_CODE${NC}"
echo "Response: "
echo "$RESPONSE" | jq . 2>/dev/null || echo "$RESPONSE"
echo ""

# Test 3: Career Recommendations (GET request)
echo "${BLUE}Test 3: Get Career Recommendations${NC}"
RESPONSE=$(curl -s -X GET "$BASE_URL/ai/career-recommendations" \
  -H "$AUTH_HEADER")

HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" -X GET "$BASE_URL/ai/career-recommendations" \
  -H "$AUTH_HEADER")

echo "HTTP Status: ${CYAN}$HTTP_CODE${NC}"
echo "Response: "
echo "$RESPONSE" | jq . 2>/dev/null || echo "$RESPONSE"
echo ""

# Test 4: Generate Interview Questions
echo "${BLUE}Test 4: Generate Interview Questions${NC}"
RESPONSE=$(curl -s -X POST "$BASE_URL/ai/generate-interview-questions" \
  -H "Content-Type: application/json" \
  -H "$AUTH_HEADER" \
  -d '{
    "jobDescription": "Senior Full Stack Developer with expertise in C#, ASP.NET Core, and React",
    "skillsRequired": ["C#", "ASP.NET Core", "React", "SQL"]
  }')

HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" -X POST "$BASE_URL/ai/generate-interview-questions" \
  -H "Content-Type: application/json" \
  -H "$AUTH_HEADER" \
  -d '{"jobDescription": "test", "skillsRequired": ["test"]}')

echo "HTTP Status: ${CYAN}$HTTP_CODE${NC}"
echo "Response: "
echo "$RESPONSE" | jq . 2>/dev/null || echo "$RESPONSE"
echo ""

# Test 5: Analyze Interview Answer
echo "${BLUE}Test 5: Analyze Interview Answer${NC}"
RESPONSE=$(curl -s -X POST "$BASE_URL/ai/analyze-interview-answer" \
  -H "Content-Type: application/json" \
  -H "$AUTH_HEADER" \
  -d '{
    "question": "What is your experience with ASP.NET Core?",
    "answer": "I have 5 years of production experience building scalable APIs with ASP.NET Core, implementing entity framework ORM, and managing databases.",
    "skillRequired": "ASP.NET Core"
  }')

HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" -X POST "$BASE_URL/ai/analyze-interview-answer" \
  -H "Content-Type: application/json" \
  -H "$AUTH_HEADER" \
  -d '{"question": "test", "answer": "test", "skillRequired": "test"}')

echo "HTTP Status: ${CYAN}$HTTP_CODE${NC}"
echo "Response: "
echo "$RESPONSE" | jq . 2>/dev/null || echo "$RESPONSE"
echo ""

# Test 6: Identify Skill Gaps
echo "${BLUE}Test 6: Identify Skill Gaps${NC}"
RESPONSE=$(curl -s -X POST "$BASE_URL/ai/identify-skill-gaps" \
  -H "Content-Type: application/json" \
  -H "$AUTH_HEADER" \
  -d '{
    "targetRole": "Senior Backend Architect",
    "currentSkills": ["C#", "ASP.NET Core", "SQL Server"]
  }')

HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" -X POST "$BASE_URL/ai/identify-skill-gaps" \
  -H "Content-Type: application/json" \
  -H "$AUTH_HEADER" \
  -d '{"targetRole": "test", "currentSkills": ["test"]}')

echo "HTTP Status: ${CYAN}$HTTP_CODE${NC}"
echo "Response: "
echo "$RESPONSE" | jq . 2>/dev/null || echo "$RESPONSE"
echo ""

echo "${BLUE}===============================================${NC}"
echo "${GREEN}✓ Test Suite Completed!${NC}"
echo "${BLUE}===============================================${NC}"
