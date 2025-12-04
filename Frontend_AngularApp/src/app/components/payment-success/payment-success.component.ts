import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { ButtonComponent } from '../ui/button/button.component';
import { CardComponent, CardContentComponent } from '../ui/card/card.component';
import { PaymentService } from '../../services/payment.service';
import { IconService } from '../../services/icon.service';
import { AuthService } from '../../services/auth.service';
import { interval, timer, Subscription } from 'rxjs';
import { switchMap, takeUntil } from 'rxjs/operators';

@Component({
  selector: 'app-payment-success',
  standalone: true,
  imports: [CommonModule, RouterModule, ButtonComponent, CardComponent, CardContentComponent],
  templateUrl: './payment-success.component.html',
  styleUrl: './payment-success.component.css'
})
export class PaymentSuccessComponent implements OnInit {
  loading = true;
  result: any = null;
  error: string | null = null;
  message: string | null = null;
  private verifying = false; // Guard against multiple concurrent verify attempts
  private pollSub: Subscription | null = null;
  private readonly POLL_INTERVAL_MS = 2000;
  private readonly TIMEOUT_MS = 30000;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private payment: PaymentService,
    public iconService: IconService,
    private authService: AuthService
  ) {
    // Force page to become visible (in case Stripe checkout left it hidden)
    if (typeof document !== 'undefined') {
      document.body.style.display = 'block';
      document.body.style.visibility = 'visible';
      document.documentElement.style.display = 'block';
      document.documentElement.style.visibility = 'visible';
    }
  }

  ngOnInit(): void {
    // If payment already verified (check sessionStorage), don't re-verify to avoid loops
    try {
      const already_verified = sessionStorage.getItem('payment_success');
      if (already_verified) {
        console.log('[PaymentSuccess] Payment already verified; skipping re-verification');
        this.loading = false;
        this.result = JSON.parse(already_verified);
        return;
      }
    } catch {}

    // PRIMARY: Retrieve the tracked session ID we saved before opening Stripe Checkout
    // This is the MOST RELIABLE source since we control its storage.
    // NOTE: Stripe doesn't substitute the {CHECKOUT_SESSION_ID} placeholder in the redirect URL,
    // so we rely on our saved session ID from sessionStorage instead.
    const trackedSessionId = this.payment.retrieveTrackedSessionId();
    if (trackedSessionId) {
      console.log('[PaymentSuccess] Found tracked session ID from sessionStorage:', trackedSessionId);
      console.log('[PaymentSuccess] Using tracked session instead of URL placeholder (Stripe does not substitute {CHECKOUT_SESSION_ID})');
      this.verifyPaymentWithFallback(trackedSessionId);
      return;
    }

    // FALLBACK: Try to read payment transaction details from query params or sessionStorage
    this.route.queryParamMap.subscribe(params => {
      // Stripe returns 'session_id' as a query parameter
      const sessionId = params.get('session_id');
      
      // Store Stripe session_id for debugging and potential fallback verification
      if (sessionId) {
        try {
          sessionStorage.setItem('stripeSessionId', sessionId);
          console.log('[PaymentSuccess] Stored stripeSessionId from query param:', sessionId);
        } catch (e) {
          console.warn('[PaymentSuccess] Failed to store stripeSessionId', e);
        }
      }
      
      let storedRef: string | null = null;
      let storedTxnId: string | number | null = null;
      try {
        const raw = sessionStorage.getItem('payment_transaction');
        if (raw) {
          const parsed = JSON.parse(raw);
          // Extract both providerReference and transactionId
          storedRef = parsed?.providerReference || null;
          storedTxnId = parsed?.transactionId || null;
        }
      } catch (e) {
        console.warn('[PaymentSuccess] Failed to parse payment_transaction from sessionStorage', e);
      }

      console.log('[PaymentSuccess] Query params - sessionId:', sessionId, 'storedProviderReference:', storedRef, 'storedTransactionId:', storedTxnId);

      // ProviderReference must be Stripe session id (cs_...). Prefer, in order:
      // 1) session_id query param (Stripe redirect)
      // 2) stored stripeSessionId in sessionStorage
      // 3) stored payment_transaction.providerReference if it looks like a Stripe session id
      let primaryRef: string | null = null;
      if (sessionId) primaryRef = String(sessionId);
      if (!primaryRef) {
        const storedStripe = sessionStorage.getItem('stripeSessionId');
        if (storedStripe) primaryRef = String(storedStripe);
      }
      if (!primaryRef && storedRef && typeof storedRef === 'string' && storedRef.startsWith('cs_')) {
        primaryRef = String(storedRef);
      }

      if (primaryRef) {
        console.log('[PaymentSuccess] Using Stripe session providerReference for verification:', primaryRef);
        this.verifyPaymentWithFallback(primaryRef);
      } else {
        // Fallback: if we land on this page without params or stored reference, 
        // it may mean Safari blocked the redirect but the payment succeeded.
        // Wait a moment and check sessionStorage again (user may have navigated manually or redirected after ITP delay)
        console.log('[PaymentSuccess] No reference on initial load; will wait and retry in 2s');
        setTimeout(() => {
          this.retryVerificationFromStorage();
        }, 2000);
      }
    });
  }

  ngOnDestroy(): void {
    try { this.pollSub?.unsubscribe(); } catch {}
  }

  private retryVerificationFromStorage(): void {
    try {
      const raw = sessionStorage.getItem('payment_transaction');
      if (raw) {
        const parsed = JSON.parse(raw);
        // Try to extract Stripe session id from the stored payment info
        const providerRef = parsed?.providerReference;
        if (providerRef && typeof providerRef === 'string' && providerRef.startsWith('cs_')) {
          try { sessionStorage.setItem('stripeSessionId', providerRef); } catch {}
          console.log('[PaymentSuccess] Retrying verification with providerReference (stripe session):', providerRef);
          this.verifyPaymentWithFallback(String(providerRef));
          return;
        }
      }
    } catch {}
    this.loading = false;
    this.error = 'No payment reference provided. Unable to verify payment. Please try again or contact support.';
  }
  // Verify payment using the provided reference
  private verifyPaymentWithFallback(reference: string): void {
    if (this.verifying) {
      console.log('[PaymentSuccess] Verification already in progress; ignoring duplicate call');
      return;
    }
    this.verifying = true;
    this.loading = true;
    console.log('[PaymentSuccess] Starting polling verification for reference:', reference);

    const stop$ = timer(this.TIMEOUT_MS);

    this.pollSub = interval(this.POLL_INTERVAL_MS)
      .pipe(
        takeUntil(stop$),
        switchMap(() => this.payment.verify(reference))
      )
      .subscribe({
        next: (res: any) => {
          console.log('[PaymentSuccess] verify() response:', res);

          // Use a resilient helper to determine whether the verify response indicates success
          if (this.isVerificationSuccessful(res)) {
            console.log('[PaymentSuccess] Payment marked completed by backend (heuristic matched)');
            this.pollSub?.unsubscribe();
            this.onSuccessVerification(res, reference);
            return;
          }

          // Not yet successful: if backend indicates pending/processing, keep polling
          const pendingStatus = (res?.status || res?.paymentStatus || res?.result || res?.data?.status || '').toString().toLowerCase();
          if (pendingStatus === 'pending' || pendingStatus === 'processing' || !pendingStatus) {
            this.message = 'Payment processing...';
            return; // continue polling
          }

          // Otherwise treat as failure
          console.warn('[PaymentSuccess] Verification returned non-success status:', pendingStatus, res);
          this.pollSub?.unsubscribe();
          this.onFailureVerification(res);
        },
        error: (err) => {
          console.error('[PaymentSuccess] Verification error during polling', err);
          this.pollSub?.unsubscribe();
          this.verifying = false;
          this.loading = false;
          this.error = err?.error?.message || err?.message || 'Payment verification failed. Contact support.';
          // keep tracked session for manual retry
        }
      });

    // handle timeout
    stop$.subscribe(() => {
      console.warn('[PaymentSuccess] Verification polling timed out after', this.TIMEOUT_MS, 'ms');
      this.pollSub?.unsubscribe();
      this.verifying = false;
      this.loading = false;
      this.message = 'Verification timed out. If you completed payment, click "Retry" or contact support.';
      // keep tracked session for user-initiated retry
    });
  }

  private onSuccessVerification(res: any, reference: string): void {
    this.loading = false;
    this.result = res;
    try { sessionStorage.removeItem('payment_transaction'); } catch {}
    try { sessionStorage.setItem('payment_success', JSON.stringify({ reference, timestamp: new Date().toISOString(), verified: true })); } catch {}
    
    // If backend provided a new token with updated role, store it immediately
    // This allows the frontend to have the updated role without needing a full sign out/in
    if (res?.token) {
      console.log('[PaymentSuccess] Backend provided updated token. Storing immediately to get fresh role.');
      try {
        this.authService.setToken(res.token, true);
        console.log('[PaymentSuccess] Token stored. Role should now be updated.');
      } catch (e) {
        console.warn('[PaymentSuccess] Failed to store token:', e);
      }
    }

    // Refresh profile and wait for it to complete, then clear session and navigate
    console.log('[PaymentSuccess] Calling refreshUserProfile() to grab updated role');
    this.authService.refreshUserProfile().subscribe({
      next: () => {
        console.log('[PaymentSuccess] refreshUserProfile completed');
        try { this.payment.clearTrackedSessionId(); } catch {}
        setTimeout(() => this.router.navigate(['/dashboard']), 1000);
      },
      error: (err) => {
        console.warn('[PaymentSuccess] refreshUserProfile failed', err);
        try { this.payment.clearTrackedSessionId(); } catch {}
        setTimeout(() => this.router.navigate(['/dashboard']), 1000);
      }
    });
  }

  private onFailureVerification(res: any): void {
    this.loading = false;
    this.verifying = false;
    this.error = res?.message || res?.error || 'Payment verification failed. Contact support.';
    // Clear tracked session on explicit failure so user can try again
    try { this.payment.clearTrackedSessionId(); } catch {}
  }

  /**
   * Heuristic helper to detect whether the backend verify response indicates a successful payment.
   * This is intentionally permissive and checks multiple common fields that different backends may return.
   */
  private isVerificationSuccessful(res: any): boolean {
    if (!res) return false;

    try {
      // Direct boolean flags
      if (res.verified === true || res.success === true || res.isPaid === true || res.paymentSucceeded === true || res.paid === true) {
        console.debug('[PaymentSuccess.isVerificationSuccessful] matched boolean flag');
        return true;
      }

      // If backend returned a new token or access token, assume success (backend often returns an updated JWT)
      if (res.token || res.accessToken || res.data?.token || res.data?.accessToken) {
        console.debug('[PaymentSuccess.isVerificationSuccessful] matched token in response');
        return true;
      }

      // Role updated flag
      if (res.roleUpdated === true || res.roleUpdated === 'true') {
        console.debug('[PaymentSuccess.isVerificationSuccessful] matched roleUpdated flag');
        return true;
      }

      // Check common string status fields
      const candidates = [
        res.status,
        res.paymentStatus,
        res.result,
        res.data?.status,
        res.payment?.status,
        res.transactionStatus,
        res.checkout?.status,
        res.intent?.status,
        res.charge?.status
      ];

      for (const c of candidates) {
        if (!c) continue;
        const s = String(c).toLowerCase();
        if (['completed', 'succeeded', 'paid', 'success'].includes(s)) {
          console.debug('[PaymentSuccess.isVerificationSuccessful] matched status candidate:', s);
          return true;
        }
      }
    } catch (e) {
      console.warn('[PaymentSuccess.isVerificationSuccessful] error while inspecting response', e);
    }

    return false;
  }

  goHome(): void {
    this.router.navigate(['/home']);
  }

  goPlans(): void {
    this.router.navigate(['/plans']);
  }

  // Debug helper: show stored values and allow manual verification retry
  getDebugInfo(): any {
    try {
      const txn = sessionStorage.getItem('payment_transaction');
      const txnRaw = sessionStorage.getItem('payment_transaction_raw');
      const stripeSessionId = sessionStorage.getItem('stripeSessionId');
      return {
        payment_transaction: txn ? JSON.parse(txn) : null,
        payment_transaction_raw: txnRaw ? JSON.parse(txnRaw) : null,
        stripeSessionId
      };
    } catch (e) {
      return { error: String(e) };
    }
  }

  retryManually(): void {
    const info = this.getDebugInfo();
    const provider = info.stripeSessionId || info.payment_transaction?.providerReference;
    if (provider && typeof provider === 'string') {
      console.log('[PaymentSuccess] User clicked "Retry Now" - attempting verification with providerReference (stripe):', provider);
      this.error = null;
      this.verifyPaymentWithFallback(String(provider));
    } else {
      console.warn('[PaymentSuccess] Cannot retry - no stripe session id found in storage');
      this.error = 'No stripe session id found. Please go back and try the checkout again.';
    }
  }
}

