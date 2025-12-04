#!/bin/bash

# Test Backend Connectivity Script
# This script helps diagnose backend auth issues

echo "====== Backend Connectivity Tests ======"
echo ""

# Test 1: Check if backend is running on 5164
echo "[1] Checking if backend is running on localhost:5164..."
curl -s -o /dev/null -w "HTTP Status: %{http_code}\n" http://localhost:5164/api/health || echo "Connection failed"
echo ""

# Test 2: Test login with test credentials
echo "[2] Testing login with test credentials..."
echo "Endpoint: http://localhost:5164/api/Auth/login"
echo "Payload: {\"email\":\"test@example.com\",\"password\":\"Test123!\"}"
echo ""

RESPONSE=$(curl -s -X POST http://localhost:5164/api/Auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Test123!"
  }')

echo "Response:"
echo "$RESPONSE" | jq '.' 2>/dev/null || echo "$RESPONSE"
echo ""

# Test 3: Test register endpoint
echo "[3] Testing register endpoint..."
echo "Endpoint: http://localhost:5164/api/Auth/register"
REGISTER_RESPONSE=$(curl -s -X POST http://localhost:5164/api/Auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "newuser@example.com",
    "password": "Test123!",
    "confirmPassword": "Test123!",
    "fullName": "Test User",
    "phone": "1234567890",
    "roleName": "user"
  }')

echo "Response:"
echo "$REGISTER_RESPONSE" | jq '.' 2>/dev/null || echo "$REGISTER_RESPONSE"
echo ""

# Test 4: Check backend logs location
echo "[4] Checking for backend process..."
ps aux | grep -i "dotnet\|aspnet" | grep -v grep || echo "No .NET process found"
echo ""

echo "====== Diagnostic Summary ======"
echo "1. Make sure backend is running on localhost:5164"
echo "2. Check backend logs for auth failures"
echo "3. Verify database has test users"
echo "4. Check Auth controller expects email/password format"
echo "5. Verify JWT signing key is configured on backend"
