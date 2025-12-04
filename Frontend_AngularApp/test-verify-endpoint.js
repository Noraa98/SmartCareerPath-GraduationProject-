#!/usr/bin/env node

/**
 * Test script to verify the /api/payment/verify endpoint
 * Run with: node test-verify-endpoint.js
 */

const http = require('http');

// Test data - replace these with actual values from your payment session
const TEST_SESSION_ID = 'cs_test_a14oQIkLsh6qCl0OmaiGhMLWTWeRLS6RfcznEHgLs0b4UzNFWPTNgYp7wO';
const AUTH_TOKEN = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjIyIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvZW1haWxhZGRyZXNzIjoiYWJkZWxyYWhtYW4uYWxpOTI5MjkxQGdtYWlsLmNvbSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IlVzZXIiLCJzdWIiOiIyMiIsImVtYWlsIjoiYWJkZWxyYWhtYW4uYWxpOTI5MjkxQGdtYWlsLmNvbSIsImlhdCI6IjIwMjUtMTItMDFUMTI6NTQ6NTQuMDAwWiIsImV4cCI6IjIwMjUtMTItMDFUMTM6NTQ6NTQuMDAwWiIsImlzcyI6IlNtYXJ0Q2FyZWVyUGF0aCJ9.qRaFfwQQ9bXZJJ8z7QRWX7GFXGX4X5X6X7X8X9X0';

function makeRequest(path, body, headers = {}) {
  return new Promise((resolve, reject) => {
    const options = {
      hostname: 'localhost',
      port: 5164,
      path: path,
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${AUTH_TOKEN}`,
        ...headers
      }
    };

    const req = http.request(options, (res) => {
      let data = '';
      res.on('data', chunk => { data += chunk; });
      res.on('end', () => {
        resolve({
          status: res.statusCode,
          headers: res.headers,
          body: data
        });
      });
    });

    req.on('error', reject);
    if (body) req.write(JSON.stringify(body));
    req.end();
  });
}

async function runTests() {
  console.log('Testing /api/payment/verify endpoint...\n');

  const attempts = [
    {
      name: 'Query param: session_id',
      path: `/api/payment/verify?session_id=${encodeURIComponent(TEST_SESSION_ID)}`,
      body: null,
      headers: { 'Content-Type': '' }
    },
    {
      name: 'Query param: transactionId',
      path: `/api/payment/verify?transactionId=${encodeURIComponent(TEST_SESSION_ID)}`,
      body: null,
      headers: { 'Content-Type': '' }
    },
    {
      name: 'JSON body: transactionId',
      path: '/api/payment/verify',
      body: { transactionId: TEST_SESSION_ID }
    },
    {
      name: 'JSON body: providerReference',
      path: '/api/payment/verify',
      body: { providerReference: TEST_SESSION_ID }
    },
    {
      name: 'JSON body: reference',
      path: '/api/payment/verify',
      body: { reference: TEST_SESSION_ID }
    },
    {
      name: 'JSON body: session_id',
      path: '/api/payment/verify',
      body: { session_id: TEST_SESSION_ID }
    },
    {
      name: 'JSON body: sessionId',
      path: '/api/payment/verify',
      body: { sessionId: TEST_SESSION_ID }
    }
  ];

  for (let i = 0; i < attempts.length; i++) {
    const attempt = attempts[i];
    try {
      console.log(`\n[Attempt ${i + 1}/${attempts.length}] ${attempt.name}`);
      console.log(`  Path: ${attempt.path}`);
      if (attempt.body) console.log(`  Body: ${JSON.stringify(attempt.body)}`);

      const result = await makeRequest(attempt.path, attempt.body, attempt.headers);
      console.log(`  Status: ${result.status}`);
      
      let responseBody = '';
      try {
        responseBody = JSON.stringify(JSON.parse(result.body), null, 2);
      } catch {
        responseBody = result.body.substring(0, 200);
      }
      console.log(`  Response: ${responseBody}`);

      if (result.status === 200 || result.status === 201) {
        console.log(`\n✓ SUCCESS on attempt ${i + 1}!`);
        return;
      }
    } catch (err) {
      console.log(`  Error: ${err.message}`);
    }
  }

  console.log('\n✗ All attempts failed. Backend endpoint may not be accepting any of these payload formats.');
}

runTests().catch(console.error);
