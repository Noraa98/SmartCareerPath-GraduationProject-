import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { ButtonComponent } from '../ui/button/button.component';
import { CardComponent, CardContentComponent } from '../ui/card/card.component';
import { IconService } from '../../services/icon.service';
import { PaymentService, PricingProduct, PricingTier } from '../../services/payment.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-plans-page',
  standalone: true,
  imports: [CommonModule, RouterModule, ButtonComponent, CardComponent, CardContentComponent],
  templateUrl: './plans-page.component.html',
  styleUrl: './plans-page.component.css'
})
export class PlansPageComponent implements OnInit {
  loading = false;
  products: PricingProduct[] = [];
  selectedProduct?: PricingProduct;
  selectedTier?: PricingTier;
  error: string | null = null;

  constructor(
    private payment: PaymentService,
    public iconService: IconService,
    private router: Router,
    private auth: AuthService
  ) {}

  ngOnInit(): void {
    this.loadPricing();
  }

  private decodeJwt(token: string): any | null {
    try {
      const parts = token.split('.');
      if (parts.length < 2) return null;
      const payload = parts[1];
      // add padding if needed
      const base64 = payload.replace(/-/g, '+').replace(/_/g, '/');
      const padded = base64 + '==='.slice((base64.length + 3) % 4);
      const json = decodeURIComponent(atob(padded).split('').map(function(c) {
        return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
      }).join(''));
      return JSON.parse(json);
    } catch (e) {
      console.warn('Failed to decode JWT', e);
      return null;
    }
  }

  loadPricing(): void {
    this.loading = true;
    this.payment.getPricing(1).subscribe({
      next: (data) => {
        console.log('[PlansPage] Pricing data from backend (raw):', data);
        // Simplify UI: force a single product "Careera Pro" with one Monthly tier at 30 EGP
        const forcedProduct: PricingProduct = {
          productTypeId: 1,
          productType: 'CareeraPro',
          displayName: 'Careera Pro',
          description: 'Get premium access to AI-powered career tools, job recommendations, and interview preparation to boost your career.',
          tiers: [
            {
              billingCycleId: 1,
              billingCycle: 'Monthly',
              amount: 30,
              displayAmount: '30 EGP',
              currency: 'EGP'
            }
          ],
          features: [
            'AI-powered career recommendations',
            'Unlimited interview practice sessions'
          ]
        };

        this.products = [forcedProduct];
        // Set defaults so the UI and checkout use the single product/tier
        this.selectedProduct = this.products[0];
        this.selectedTier = this.selectedProduct.tiers[0];
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load pricing';
        console.error(err);
        this.loading = false;
      }
    });
  }

  selectProduct(p: PricingProduct, tier: PricingTier): void {
    this.selectedProduct = p;
    this.selectedTier = tier;
    this.startCheckout();
  }

  startCheckout(): void {
    console.log('[PlansPage] startCheckout() called');
    if (!this.selectedProduct || !this.selectedTier) return;

    // Ensure user is logged in before creating session
    if (!this.auth.isLoggedIn()) {
      // redirect to login and possibly return
      this.router.navigate(['/login']);
      return;
    }

    // Check token expiry and extract user id if available
    const token = this.auth.getToken();
    console.log('[PlansPage] Token:', token);
    console.log('[PlansPage] Token from localStorage:', localStorage.getItem('scp_auth_token'));
    console.log('[PlansPage] Token from sessionStorage:', sessionStorage.getItem('scp_auth_token'));
    
    if (token) {
      const payloadObj = this.decodeJwt(token);
      if (payloadObj) {
        const now = Math.floor(Date.now() / 1000);
        if (payloadObj.exp && payloadObj.exp < now) {
          this.error = 'Session expired. Please log in again.';
          this.router.navigate(['/login']);
          return;
        }
      }
    }

    this.loading = true;
    
    // Use the numeric IDs from the backend (productTypeId and billingCycleId)
    const productTypeNum = this.selectedProduct.productTypeId || this.selectedProduct.productType;
    const billingCycleNum = this.selectedTier.billingCycleId || this.selectedTier.billingCycle;
    
    // Ensure they are numbers
    let productTypeId = typeof productTypeNum === 'number' ? productTypeNum : parseInt(productTypeNum, 10);
    let billingCycleId = typeof billingCycleNum === 'number' ? billingCycleNum : parseInt(billingCycleNum, 10);
    
    if (isNaN(productTypeId)) {
      console.warn('[PlansPage] Could not parse productTypeId:', productTypeNum);
      productTypeId = 1;
    }
    if (isNaN(billingCycleId)) {
      console.warn('[PlansPage] Could not parse billingCycleId:', billingCycleNum);
      billingCycleId = 1;
    }
    
    const payload: any = {
      productType: productTypeId,
      paymentProvider: 1,
      currency: 1,
      billingCycle: billingCycleId,
      // Include the placeholder so Stripe substitutes the session id into the redirect URL
      successUrl: `${window.location.origin}/paymob/response?session_id={CHECKOUT_SESSION_ID}`,
      cancelUrl: `${window.location.origin}/paymob/cancel`
    };

    // Attach userId from token claims if present (common claim keys)
    try {
      if (token) {
        const claims = this.decodeJwt(token) || {};
        // common claim names: sub, nameid, http://schemas.xml.../nameidentifier
        const userId = claims.sub || claims.nameid || claims['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'];
        if (userId) payload.userId = userId;
      }
    } catch (e) {
      // ignore decode errors, backend may derive user from token
    }

    console.log('[PlansPage] Creating session with payload:', payload);
    console.log('[PlansPage] Payload JSON:', JSON.stringify(payload, null, 2));
    console.log('[PlansPage] Payload details:', {
      productType: { type: typeof payload.productType, value: payload.productType },
      billingCycle: { type: typeof payload.billingCycle, value: payload.billingCycle },
      paymentProvider: payload.paymentProvider,
      currency: payload.currency,
      userId: payload.userId,
      successUrl: payload.successUrl,
      cancelUrl: payload.cancelUrl
    });
    this.payment.createSession(payload).subscribe({
      next: (res) => {
        // CRITICAL: Track the Stripe session ID BEFORE redirecting to Stripe Checkout
        // This allows us to retrieve the session ID after Stripe redirects back to our success page
        if (res && res.providerReference) {
          console.log('[PlansPage] Tracking Stripe session before redirect:', res.providerReference);
          this.payment.trackCheckoutSession(res.providerReference);
        }

        // redirect to checkoutUrl
        if (res && res.checkoutUrl) {
          // Log full response for debugging and store raw response for inspection after redirect
          console.log('[PlansPage] createSession response (full):', res);
          try {
            sessionStorage.setItem('payment_transaction_raw', JSON.stringify(res));
          } catch {}

          // Optionally store transaction id and providerReference for later verification
          try {
            sessionStorage.setItem('payment_transaction', JSON.stringify({
              transactionId: res.transactionId,
              providerReference: res.providerReference,
              checkoutUrl: res.checkoutUrl
            }));
          } catch (e) {
            console.warn('[PlansPage] Failed to persist payment_transaction:', e);
          }

          console.log('[PlansPage] Redirecting to checkout URL:', res.checkoutUrl);
          window.location.href = res.checkoutUrl;
          
          // FALLBACK: If Stripe checkout doesn't redirect within 5 seconds,
          // manually redirect to the success page (frontend will poll backend)
          const redirectTimeout = setTimeout(() => {
            console.warn('[PlansPage] Stripe checkout did not complete redirect within 5s timeout');
            console.log('[PlansPage] Manually navigating to payment success page with tracked session');
            const trackedSession = this.payment.retrieveTrackedSessionId();
            if (trackedSession) {
              // Redirect to success page - it will handle verification
              window.location.href = `http://localhost:4200/paymob/response?session_id=${trackedSession}`;
            } else {
              console.error('[PlansPage] No tracked session available for manual redirect');
              window.location.href = 'http://localhost:4200/paymob/response';
            }
          }, 5000);
          
          // If the page is unloaded (Stripe redirecting), cancel the timeout
          window.addEventListener('beforeunload', () => clearTimeout(redirectTimeout));
        } else {
          this.error = 'Invalid checkout response';
          console.error('[PlansPage] No checkoutUrl in response:', res);
        }
        this.loading = false;
      },
      error: (err) => {
        // Log full error response for backend debugging
        console.error('[PlansPage] createSession error - Full response:', err);
        console.error('[PlansPage] Error status:', err?.status);
        console.error('[PlansPage] Error body:', err?.error);
        console.error('[PlansPage] Error message:', err?.message);
        
        // Prefer server-provided message when available
        const serverMessage = err?.error?.message || err?.error?.detail || err?.message;
        this.error = serverMessage || 'Could not start payment session';
        this.loading = false;
      }
    });
  }

  getCycleLabel(billingCycle?: number | string, index?: number): string {
    // Handle numeric ID
    if (typeof billingCycle === 'number') {
      if (billingCycle === 1) return 'Monthly';
      if (billingCycle === 2) return 'Yearly';
      if (billingCycle === 3) return 'Lifetime';
    }
    
    // Handle string display name
    if (typeof billingCycle === 'string') {
      return billingCycle; // e.g., 'Monthly', 'Yearly', 'Lifetime'
    }

    // Fallback: infer from index (0 -> Monthly, 1 -> Yearly, 2 -> Lifetime)
    if (typeof index === 'number') {
      if (index === 0) return 'Monthly';
      if (index === 1) return 'Yearly';
      return 'Lifetime';
    }

    return 'Lifetime';
  }

  getDisplayAmount(tier: PricingTier): string {
    // Prefer explicit displayAmount when it contains digits (a formatted price).
    if (tier.displayAmount && /\d/.test(tier.displayAmount)) {
      return tier.displayAmount;
    }

    // Fall back to numeric amount if available
    if (tier.amount !== undefined && tier.amount !== null) {
      // Assume amount is major currency units for now
      return tier.amount + ' USD';
    }

    // Last resort: show cycle label using numeric ID
    return this.getCycleLabel(tier.billingCycleId);
  }
}
