#!/bin/bash

# API Test Script - Register, Login, Create Payment Session
# This script tests the complete flow with comprehensive logging

set -e  # Exit on error

BASE_URL="http://localhost:5164/api"
TIMESTAMP=$(date +%s%N)
TEST_EMAIL="testuser_${TIMESTAMP}@test.com"
TEST_PASSWORD="TestPassword123!"

echo "=========================================="
echo "SmartCareerPath Payment Session Test"
echo "=========================================="
echo ""

# Step 1: Register
echo "[1/3] REGISTERING user: $TEST_EMAIL"
REGISTER_RESPONSE=$(curl -s -X POST "$BASE_URL/auth/register" \
  -H "Content-Type: application/json" \
  -d "{\"email\":\"$TEST_EMAIL\",\"password\":\"$TEST_PASSWORD\",\"confirmPassword\":\"$TEST_PASSWORD\",\"firstName\":\"Test\",\"lastName\":\"User\",\"fullName\":\"Test User\",\"phone\":\"1234567890\"}")

echo "Response:"
echo "$REGISTER_RESPONSE" | jq . 2>/dev/null || echo "$REGISTER_RESPONSE"
echo ""

# Extract token from registration response
TOKEN=$(echo "$REGISTER_RESPONSE" | jq -r '.token // empty' 2>/dev/null)

if [ -z "$TOKEN" ]; then
  echo "ERROR: Failed to extract JWT token from registration response"
  exit 1
fi

echo "[2/3] TOKEN OBTAINED FROM REGISTRATION"
echo "✓ Token obtained (truncated): ${TOKEN:0:40}..."
echo ""

# Step 3: Create Payment Session
echo "[3/3] CREATING PAYMENT SESSION (Subscription mode: Monthly, CVBuilderSubscription)"
echo "Request payload:"
cat <<'PAYLOAD'
{
  "userId": 0,
  "productType": 2,
  "paymentProvider": 1,
  "currency": 2,
  "billingCycle": 1,
  "successUrl": "http://localhost:4200/paymob/response",
  "cancelUrl": "http://localhost:4200/paymob/cancel"
}
PAYLOAD
echo ""

CREATE_RESPONSE=$(curl -s -X POST "$BASE_URL/payment/create-session" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"userId":0,"productType":2,"paymentProvider":1,"currency":2,"billingCycle":1,"successUrl":"http://localhost:4200/paymob/response","cancelUrl":"http://localhost:4200/paymob/cancel"}')

echo "Response:"
echo "$CREATE_RESPONSE" | jq . 2>/dev/null || echo "$CREATE_RESPONSE"
echo ""

# Extract session ID
SESSION_ID=$(echo "$CREATE_RESPONSE" | jq -r '.providerReference // empty' 2>/dev/null)
CHECKOUT_URL=$(echo "$CREATE_RESPONSE" | jq -r '.checkoutUrl // empty' 2>/dev/null)

if [ -n "$SESSION_ID" ]; then
  echo "✓ Session created successfully!"
  echo "  Session ID: $SESSION_ID"
  echo "  Checkout URL: $CHECKOUT_URL"
else
  echo "✗ Failed to create session"
fi
echo ""

echo "=========================================="
echo "Now check the API logs (tail -f api_nohup.log) for:"
echo "  - STEP 1: SuccessUrl processing"
echo "  - STEP 2: CancelUrl processing"
echo "  - STEP 5: Session options configured"
echo "  - STEP 8: Verifying session and building response"
echo "=========================================="
