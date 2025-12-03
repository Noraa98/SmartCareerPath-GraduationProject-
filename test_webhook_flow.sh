#!/bin/bash

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

BASE_URL="http://localhost:5164"
FRONTEND_URL="http://localhost:4200"

echo -e "${BLUE}=== E2E Payment & Webhook Test ===${NC}\n"

# 1. Register test user
echo -e "${BLUE}Step 1: Registering test user...${NC}"
TEST_EMAIL="webhook-test-$(date +%s)@example.com"
REGISTER_RESPONSE=$(curl -s -X POST "$BASE_URL/api/auth/register" \
  -H "Content-Type: application/json" \
  -d "{
    \"email\": \"$TEST_EMAIL\",
    \"password\": \"TestPass123!\",
    \"confirmPassword\": \"TestPass123!\",
    \"fullName\": \"Webhook Test User\",
    \"phone\": \"1234567890\"
  }")

USER_ID=$(echo "$REGISTER_RESPONSE" | jq -r '.data.userId // .userId // empty' 2>/dev/null || echo "")
if [ -z "$USER_ID" ]; then
  echo -e "${RED}✗ Registration failed${NC}"
  echo "$REGISTER_RESPONSE" | jq . 2>/dev/null || echo "$REGISTER_RESPONSE"
  exit 1
fi
echo -e "${GREEN}✓ User registered: ID=$USER_ID${NC}\n"

# 2. Login to get token
echo -e "${BLUE}Step 2: Logging in to get auth token...${NC}"
LOGIN_RESPONSE=$(curl -s -X POST "$BASE_URL/api/auth/login" \
  -H "Content-Type: application/json" \
  -d "{\"email\": \"$TEST_EMAIL\", \"password\": \"TestPass123!\"}")

TOKEN=$(echo "$LOGIN_RESPONSE" | jq -r '.data.accessToken // empty' 2>/dev/null)
if [ -z "$TOKEN" ]; then
  echo -e "${RED}✗ Login failed${NC}"
  echo "$LOGIN_RESPONSE" | jq . 2>/dev/null || echo "$LOGIN_RESPONSE"
  exit 1
fi
echo -e "${GREEN}✓ Login successful. Token acquired.${NC}\n"

# 3. Create payment session
echo -e "${BLUE}Step 3: Creating Stripe payment session...${NC}"
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
echo -e "${GREEN}✓ Session created:${NC}"
echo "  TransactionId: $TRANSACTION_ID"
echo "  SessionId: $SESSION_ID\n"

# 4. Check user role BEFORE webhook
echo -e "${BLUE}Step 4: Checking user role BEFORE webhook processing...${NC}"
USER_BEFORE=$(curl -s -X GET "$BASE_URL/api/auth/me" \
  -H "Authorization: Bearer $TOKEN")
ROLE_BEFORE=$(echo "$USER_BEFORE" | jq -r '.data.role // "Unknown"' 2>/dev/null)
echo -e "  Current role: ${BLUE}$ROLE_BEFORE${NC}\n"

# 5. Send webhook event
echo -e "${BLUE}Step 5: Sending minimal webhook event to trigger verification...${NC}"
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

WEBHOOK_RESPONSE=$(curl -s -X POST "$BASE_URL/api/payment/stripe/webhook" \
  -H "Content-Type: application/json" \
  -H "Stripe-Signature: t=123,v1=testsig" \
  -d "$WEBHOOK_PAYLOAD")

HTTP_STATUS=$(curl -s -o /dev/null -w "%{http_code}" -X POST "$BASE_URL/api/payment/stripe/webhook" \
  -H "Content-Type: application/json" \
  -H "Stripe-Signature: t=123,v1=testsig" \
  -d "$WEBHOOK_PAYLOAD")

if [ "$HTTP_STATUS" = "200" ]; then
  echo -e "${GREEN}✓ Webhook processed successfully (HTTP $HTTP_STATUS)${NC}\n"
else
  echo -e "${RED}✗ Webhook returned HTTP $HTTP_STATUS${NC}"
  echo "$WEBHOOK_RESPONSE" | jq . 2>/dev/null || echo "$WEBHOOK_RESPONSE"
  echo ""
fi

# 6. Check user role AFTER webhook
echo -e "${BLUE}Step 6: Checking user role AFTER webhook processing...${NC}"
sleep 1
USER_AFTER=$(curl -s -X GET "$BASE_URL/api/auth/me" \
  -H "Authorization: Bearer $TOKEN")
ROLE_AFTER=$(echo "$USER_AFTER" | jq -r '.data.role // "Unknown"' 2>/dev/null)
echo -e "  Current role: ${BLUE}$ROLE_AFTER${NC}\n"

# 7. Check payment status
echo -e "${BLUE}Step 7: Verifying payment transaction status...${NC}"
PAYMENT_STATUS=$(curl -s -X GET "$BASE_URL/api/payment/$TRANSACTION_ID" \
  -H "Authorization: Bearer $TOKEN")
TX_STATUS=$(echo "$PAYMENT_STATUS" | jq -r '.status // "Unknown"' 2>/dev/null)
echo -e "  Transaction status: ${BLUE}$TX_STATUS${NC}\n"

# 8. Summary
echo -e "${BLUE}=== Test Summary ===${NC}"
if [ "$ROLE_AFTER" = "Premium" ] || [ "$ROLE_AFTER" = "premium" ]; then
  echo -e "${GREEN}✓ SUCCESS: User role changed from '$ROLE_BEFORE' to '$ROLE_AFTER'${NC}"
  echo -e "${GREEN}✓ Payment workflow completed successfully!${NC}"
  exit 0
else
  echo -e "${RED}✗ FAILED: User role is still '$ROLE_AFTER' (expected 'Premium')${NC}"
  echo -e "${RED}  Role before webhook: $ROLE_BEFORE${NC}"
  echo -e "${RED}  Role after webhook: $ROLE_AFTER${NC}"
  echo -e "${RED}  Payment status: $TX_STATUS${NC}"
  exit 1
fi
