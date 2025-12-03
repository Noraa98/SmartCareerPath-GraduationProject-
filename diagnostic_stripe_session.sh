#!/bin/bash

# Diagnostic script to check what Stripe actually stored in a session
# Usage: ./diagnostic_stripe_session.sh <SESSION_ID>

if [ -z "$1" ]; then
    echo "Usage: $0 <SESSION_ID>"
    echo "Example: $0 cs_test_a1ltpu74U7RoAlHidckbzulSox96oQ8YBDj8pTTKscBpG567Vt87hlBH4w"
    exit 1
fi

SESSION_ID="$1"

# Load Stripe API key from appsettings.json
STRIPE_KEY=$(grep -o '"SecretKey": "[^"]*"' SmartCareerPath.APIs/appsettings.json | cut -d'"' -f4)

if [ -z "$STRIPE_KEY" ]; then
    echo "Error: Could not find Stripe SecretKey in appsettings.json"
    exit 1
fi

echo "==========================================="
echo "Querying Stripe for Session: $SESSION_ID"
echo "==========================================="
echo ""

# Query Stripe API for the session details
curl -s -H "Authorization: Bearer $STRIPE_KEY" \
    "https://api.stripe.com/v1/checkout/sessions/$SESSION_ID" | jq '.'

echo ""
echo "==========================================="
echo "Key fields to check:"
echo "  - success_url: Should contain http://localhost:4200/paymob/response?session_id={CHECKOUT_SESSION_ID}"
echo "  - cancel_url: Should contain http://localhost:4200/paymob/cancel"
echo "  - client_reference_id: Should contain the userId (e.g., 2029)"
echo "==========================================="
