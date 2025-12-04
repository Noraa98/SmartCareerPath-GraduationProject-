#!/usr/bin/env node

/**
 * Diagnostic script to test /api/payment/verify endpoint
 * Shows: (1) what the endpoint expects, (2) what request format works
 */

const http = require('http');

function makeRequest(path, body, method = 'POST', headers = {}) {
  return new Promise((resolve, reject) => {
    const options = {
      hostname: 'localhost',
      port: 5164,
      path: path,
      method: method,
      headers: {
        'Content-Type': 'application/json',
        ...headers
      }
    };

    const req = http.request(options, (res) => {
      let data = '';
      res.on('data', chunk => { data += chunk; });
      res.on('end', () => {
        resolve({
          status: res.statusCode,
          statusText: res.statusMessage,
          body: data,
          headers: res.headers
        });
      });
    });

    req.on('error', reject);
    if (body) req.write(JSON.stringify(body));
    req.end();
  });
}

async function runDiagnostic() {
  console.log('='.repeat(80));
  console.log('PAYMENT VERIFY ENDPOINT DIAGNOSTIC');
  console.log('='.repeat(80));

  // Test 1: Check what the endpoint expects (missing required fields)
  console.log('\n[TEST 1] Minimal empty body (to see DTO requirements):\n');
  let result = await makeRequest('/api/payment/verify', {});
  console.log(`Status: ${result.status}`);
  console.log(`Body:\n${JSON.stringify(JSON.parse(result.body), null, 2)}\n`);

  // Test 2: Try with providerReference and empty request
  console.log('[TEST 2] { providerReference: "test", request: {} }:\n');
  result = await makeRequest('/api/payment/verify', { providerReference: 'test', request: {} });
  console.log(`Status: ${result.status}`);
  try {
    console.log(`Body:\n${JSON.stringify(JSON.parse(result.body), null, 2)}\n`);
  } catch {
    console.log(`Body:\n${result.body}\n`);
  }

  // Test 3: Try with numeric providerReference
  console.log('[TEST 3] { providerReference: 1, request: {} }:\n');
  result = await makeRequest('/api/payment/verify', { providerReference: 1, request: {} });
  console.log(`Status: ${result.status}`);
  try {
    console.log(`Body:\n${JSON.stringify(JSON.parse(result.body), null, 2)}\n`);
  } catch {
    console.log(`Body:\n${result.body}\n`);
  }

  // Test 4: Try with string providerReference and null request
  console.log('[TEST 4] { providerReference: "test", request: null }:\n');
  result = await makeRequest('/api/payment/verify', { providerReference: 'test', request: null });
  console.log(`Status: ${result.status}`);
  try {
    console.log(`Body:\n${JSON.stringify(JSON.parse(result.body), null, 2)}\n`);
  } catch {
    console.log(`Body:\n${result.body}\n`);
  }

  // Test 5: Try with providerReference only
  console.log('[TEST 5] { providerReference: "test" } (no request field):\n');
  result = await makeRequest('/api/payment/verify', { providerReference: 'test' });
  console.log(`Status: ${result.status}`);
  try {
    console.log(`Body:\n${JSON.stringify(JSON.parse(result.body), null, 2)}\n`);
  } catch {
    console.log(`Body:\n${result.body}\n`);
  }

  console.log('='.repeat(80));
  console.log('RECOMMENDATION:');
  console.log('='.repeat(80));
  console.log(`
Based on the responses above:
1. Check which status codes indicate the request format is correct (usually 200 or 4xx with clear error)
2. Look for patterns in error messages (e.g., missing fields, type mismatch)
3. If Status 400 with "not found", that's GOOD — it means format is accepted but reference doesn't exist
4. If Status 400 with "validation errors" or "deserialization", that's BAD — format is wrong

The frontend should send the providerReference from the backend's create-session response.
  `);
}

runDiagnostic().catch(console.error);
