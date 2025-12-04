// Enhanced payment-success.component.ts with better diagnostics
// This shows what to add to the existing file for better debugging

// ADD THIS METHOD to PaymentSuccessComponent class:

/**
 * Log detailed diagnostic info to help debug Stripe redirect issues
 * Call this at the start of ngOnInit or on component load
 */
private logDiagnostics(): void {
  console.log('\n' + '='.repeat(80));
  console.log('[PaymentSuccess] DIAGNOSTIC INFORMATION');
  console.log('='.repeat(80));
  
  // Current URL
  console.log('📍 Current URL:', window.location.href);
  console.log('📍 Current Origin:', window.location.origin);
  console.log('📍 Current Pathname:', window.location.pathname);
  console.log('📍 Query Params:', new URLSearchParams(window.location.search).toString());
  
  // Get session_id from URL
  const urlParams = new URLSearchParams(window.location.search);
  const sessionIdFromUrl = urlParams.get('session_id');
  console.log('📍 Session ID from URL:', sessionIdFromUrl || '(NOT FOUND)');
  
  // Check sessionStorage
  console.log('\n📦 SessionStorage Contents:');
  const stripeSessionId = sessionStorage.getItem('stripeSessionId');
  const paymentTransaction = sessionStorage.getItem('payment_transaction');
  const paymentTransactionRaw = sessionStorage.getItem('payment_transaction_raw');
  const paymentSuccess = sessionStorage.getItem('payment_success');
  
  console.log('  stripeSessionId:', stripeSessionId || '(empty)');
  console.log('  payment_transaction:', paymentTransaction ? JSON.parse(paymentTransaction) : '(empty)');
  console.log('  payment_transaction_raw:', paymentTransactionRaw ? JSON.parse(paymentTransactionRaw) : '(empty)');
  console.log('  payment_success:', paymentSuccess ? JSON.parse(paymentSuccess) : '(empty)');
  
  // Analyze what we have
  console.log('\n🔍 Analysis:');
  if (sessionIdFromUrl) {
    console.log('  ✓ Session ID found in URL → Ready to verify');
  } else if (stripeSessionId) {
    console.log('  ⚠ Session ID not in URL, but found in sessionStorage → Will use stored value');
  } else if (paymentTransaction) {
    const parsed = JSON.parse(paymentTransaction);
    if (parsed.providerReference?.startsWith('cs_')) {
      console.log('  ⚠ Session ID not in URL, but found in payment_transaction → Will use fallback');
    } else {
      console.log('  ✗ No valid session ID found anywhere');
    }
  } else {
    console.log('  ✗ NO SESSION ID FOUND - Payment cannot be verified');
    console.log('  ℹ This usually means:');
    console.log('    1. Stripe did not redirect to success_url (backend issue)');
    console.log('    2. User manually navigated to /paymob/response without session_id');
    console.log('    3. Browser refresh cleared sessionStorage');
  }
  
  // Check auth state
  console.log('\n👤 Auth State:');
  console.log('  Logged in:', this.authService.isLoggedIn());
  console.log('  Current role:', this.authService.getUserRole());
  console.log('  Token:', this.authService.getToken() ? '(present)' : '(missing)');
  
  console.log('='.repeat(80) + '\n');
}

// MODIFY the ngOnInit() method to add logging at the start:
ngOnInit(): void {
  console.log('[PaymentSuccess.ngOnInit] Component initialized');
  
  // ADD THIS LINE at the very start of ngOnInit:
  this.logDiagnostics();
  
  // ... rest of existing code ...
}

// ALSO ADD THIS to the verifyPaymentWithFallback method for better error details:
private verifyPaymentWithFallback(reference: string): void {
  if (this.verifying) {
    console.log('[PaymentSuccess.verify] Verification already in progress; ignoring duplicate call');
    return;
  }
  this.verifying = true;
  this.loading = true;
  
  console.log('[PaymentSuccess.verify] ===== VERIFY START =====');
  console.log('[PaymentSuccess.verify] Reference:', reference);
  console.log('[PaymentSuccess.verify] Calling /api/payment/verify endpoint...');

  this.payment.verify(reference).subscribe({
    next: (res) => {
      console.log('[PaymentSuccess.verify] ✓ SUCCESS - Verification response:', res);
      console.log('[PaymentSuccess.verify] Response status code would be 200');
      // ... rest of existing success handler ...
    },
    error: (err) => {
      console.log('[PaymentSuccess.verify] ✗ ERROR - Verification failed');
      console.log('[PaymentSuccess.verify] HTTP Status:', err?.status);
      console.log('[PaymentSuccess.verify] Error Response:', err?.error);
      console.log('[PaymentSuccess.verify] Error Message:', err?.message);
      console.log('[PaymentSuccess.verify] Full Error Object:', err);
      // ... rest of existing error handler ...
    }
  });
}
