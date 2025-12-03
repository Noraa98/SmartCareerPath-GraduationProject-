#!/bin/bash

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

BASE_URL="http://localhost:5164"
FRONTEND_URL="http://localhost:4200"

# Use existing verified test user
TEST_EMAIL="omar@gmail.com"
TEST_PASSWORD="Abdo@246810"

echo -e "${BLUE}=== E2E Payment & Webhook Test ===${NC}\n"

# 1. Login to get token
echo -e "${BLUE}Step 1: Logging in to get auth token...${NC}"
LOGIN_RESPONSE=$(curl -s -X POST "$BASE_URL/api/auth/login" \
  -H "Content-Type: application/json" \
  -d "{\"email\": \"$TEST_EMAIL\", \"password\": \"$TEST_PASSWORD\"}")

TOKEN=$(echo "$LOGIN_RESPONSE" | jq -r '.token // empty' 2>/dev/null)
USER_ID=$(echo "$LOGIN_RESPONSE" | jq -r '.userId // empty' 2>/dev/null)

if [ -z "$TOKEN" ] || [ -z "$USER_ID" ]; then
  echo -e "${RED}✗ Login failed${NC}"
  echo "$LOGIN_RESPONSE" | jq . 2>/dev/null || echo "$LOGIN_RESPONSE"
  exit 1
fi
echo -e "${GREEN}✓ Login successful${NC}"
echo "  User ID: $USER_ID"
echo "  Token: ${TOKEN:0:50}...${NC}\n"

# 2. Create payment session
echo -e "${BLUE}Step 2: Creating Stripe payment session...${NC}"
SESSION_RESPONSE=$(curl -s -X POST "$BASE_URL/api/payment/create-session" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d "{
    \"userId\": $USER_ID,
    \"productType\": 1,
    \"paymentProvider\": 1,
    \"currency\": 1,
    \"billingCycle\": 1,
    \"successUrl\": \"$FRONTEND_URL/paymob/response\",
    \"cancelUrl\": \"$FRONTEND_URL/paymob/cancel\"
  }")

SESSION_ID=$(echo "$SESSION_RESPONSE" | jq -r '.providerReference // empty' 2>/dev/null)
TRANSACTION_ID=$(echo "$SESSION_RESPONSE" | jq -r '.transactionId // empty' 2>/dev/null)

if [ -z "$SESSION_ID" ] || [ -z "$TRANSACTION_ID" ]; then
  echo -e "${RED}✗ Session creation failed${NC}"
  echo "$SESSION_RESPONSE" | jq . 2>/dev/null || echo "$SESSION_RESPONSE"
  exit 1
fi
echo -e "${GREEN}✓ Session created${NC}"
echo "  TransactionId: $TRANSACTION_ID"
echo "  SessionId: ${SESSION_ID:0:40}...${NC}\n"

# 3. Check user role BEFORE webhook
echo -e "${BLUE}Step 3: Checking user role BEFORE webhook processing...${NC}"
USER_BEFORE=$(curl -s -X GET "$BASE_URL/api/auth/me" \
  -H "Authorization: Bearer $TOKEN")
ROLE_BEFORE=$(echo "$USER_BEFORE" | jq -r '.role // "Unknown"' 2>/dev/null)
echo -e "  Current role: ${BLUE}$ROLE_BEFORE${NC}\n"

# 4. Send webhook event
echo -e "${BLUE}Step 4: Sending minimal webhook event to trigger verification...${NC}"
WEBHOOK_PAYLOAD="{
  \"type\": \"checkout.session.completed\",
  \"data\": {
    \"object\": {
      \"id\": \"$SESSION_ID\",
      \"payment_status\": \"paid\",
      \"amount_total\": 19999,
      \"currency\": \"egp\"
    }
  }
}"

HTTP_STATUS=$(curl -s -o /dev/null -w "%{http_code}" -X POST "$BASE_URL/api/payment/stripe/webhook" \
  -H "Content-Type: application/json" \
  -H "Stripe-Signature: t=123,v1=testsig" \
  -d "$WEBHOOK_PAYLOAD")

if [ "$HTTP_STATUS" = "200" ]; then
  echo -e "${GREEN}✓ Webhook processed successfully (HTTP $HTTP_STATUS)${NC}\n"
else
  echo -e "${RED}✗ Webhook returned HTTP $HTTP_STATUS${NC}\n"
fi

# 5. Wait a moment for DB updates
sleep 1

# 6. Re-login to get new token with updated role
echo -e "${BLUE}Step 6: Re-logging in to get fresh token with updated role...${NC}"
LOGIN_AFTER=$(curl -s -X POST "$BASE_URL/api/auth/login" \
  -H "Content-Type: application/json" \
  -d "{\"email\": \"$TEST_EMAIL\", \"password\": \"$TEST_PASSWORD\"}")

NEW_TOKEN=$(echo "$LOGIN_AFTER" | jq -r '.token // empty' 2>/dev/null)
if [ -z "$NEW_TOKEN" ]; then
  echo -e "${RED}✗ Re-login failed${NC}"
  exit 1
fi

# 7. Check user role AFTER webhook (using new token)
echo -e "${BLUE}Step 7: Checking user role AFTER webhook processing (fresh token)...${NC}"
USER_AFTER=$(curl -s -X GET "$BASE_URL/api/auth/me" \
  -H "Authorization: Bearer $NEW_TOKEN")
ROLE_AFTER=$(echo "$USER_AFTER" | jq -r '.role // "Unknown"' 2>/dev/null)
echo -e "  Current role: ${BLUE}$ROLE_AFTER${NC}\n"

# 8. Check payment status
echo -e "${BLUE}Step 8: Verifying payment transaction status...${NC}"
PAYMENT_STATUS=$(curl -s -X GET "$BASE_URL/api/payment/$TRANSACTION_ID" \
  -H "Authorization: Bearer $NEW_TOKEN")
TX_STATUS=$(echo "$PAYMENT_STATUS" | jq -r '.status // "Unknown"' 2>/dev/null)
echo -e "  Transaction status: ${BLUE}$TX_STATUS${NC}\n"

# 8. Summary
echo -e "${BLUE}=== Test Summary ===${NC}"
if [ "$ROLE_AFTER" = "Premium" ] || [ "$ROLE_AFTER" = "premium" ]; then
  echo -e "${GREEN}✓✓✓ SUCCESS ✓✓✓${NC}"
  echo -e "${GREEN}✓ User role changed from '$ROLE_BEFORE' → '$ROLE_AFTER'${NC}"
  echo -e "${GREEN}✓ Payment webhook processed successfully!${NC}"
  echo -e "${GREEN}✓ Payment workflow complete!${NC}"
  exit 0
else
  echo -e "${RED}✗ Role change failed${NC}"
  echo -e "${RED}  Expected: Premium${NC}"
  echo -e "${RED}  Got: $ROLE_AFTER${NC}"
  echo -e "${RED}  Payment status: $TX_STATUS${NC}"
  exit 1
fi
