import { Injectable } from '@angular/core';
import { HttpClient, HttpParams, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';

export interface PricingTier {
  billingCycleId: number;
  billingCycle: string;
  amount: number;
  displayAmount: string;
  currency?: string;
  discountPercentage?: number;
}

export interface PricingProduct {
  productTypeId: number;
  productType: string;
  displayName: string;
  description?: string;
  tiers: PricingTier[];
  features?: string[];
}

export interface CreateSessionRequest {
  userId?: string;
  productType: number;
  paymentProvider: number;
  currency: number;
  billingCycle: number;
  successUrl: string;
  cancelUrl: string;
}

export interface CreateSessionResponse {
  transactionId: string;
  providerReference: string;
  checkoutUrl: string;
  amount: number;
  currency: number;
  expiresAt?: string;
}

@Injectable({ providedIn: 'root' })
export class PaymentService {
  // Use full backend URL instead of relying on proxy
  private base = 'http://localhost:5164/api/payment';
  
  // Storage key for tracking Stripe checkout session
  private readonly SESSION_KEY = 'last_checkout_session_id';

  constructor(private http: HttpClient, private auth: AuthService) {}

  getPricing(currency = 1): Observable<PricingProduct[]> {
    const params = new HttpParams().set('currency', currency.toString());
    return this.http.get<PricingProduct[]>(`${this.base}/pricing`, { params });
  }

  createSession(payload: CreateSessionRequest): Observable<CreateSessionResponse> {
    console.log('[PaymentService] createSession called with payload:', payload);
    console.log('[PaymentService] Payload stringified:', JSON.stringify(payload));
    return this.http.post<CreateSessionResponse>(`${this.base}/create-session`, payload);
  }

  verify(providerReference: string): Observable<any> {
    // Backend expects: POST /api/payment/verify with { providerReference: string }
    // IMPORTANT: verify endpoint is AllowAnonymous on the backend — do NOT send Authorization header.
    const headers = new HttpHeaders({ 'Content-Type': 'application/json' });

    console.log('[PaymentService] verify() called (no auth) with providerReference:', providerReference);

    const body = {
      providerReference
    };

    return this.http.post<any>(`${this.base}/verify`, body, { headers });
  }

  getHistory(userId: string, pageNumber = 1, pageSize = 20): Observable<any> {
    return this.http.get(`${this.base}/history/${encodeURIComponent(userId)}?pageNumber=${pageNumber}&pageSize=${pageSize}`);
  }

  getDetails(id: string): Observable<any> {
    return this.http.get(`${this.base}/${encodeURIComponent(id)}`);
  }

  refundRequest(payload: { paymentTransactionId: string; userId: string; refundAmount: number; reason: string; }): Observable<any> {
    return this.http.post(`${this.base}/refund-request`, payload);
  }

  /**
   * Track the Stripe checkout session ID before opening the Stripe Checkout modal.
   * This allows us to retrieve the session ID after redirect from Stripe.
   * IMPORTANT: Call this BEFORE opening Stripe Checkout.
   */
  trackCheckoutSession(sessionId: string): void {
    if (!sessionId) {
      console.warn('[PaymentService] trackCheckoutSession called with empty sessionId');
      return;
    }
    try {
      sessionStorage.setItem(this.SESSION_KEY, sessionId);
      console.log('[PaymentService] Tracked checkout session:', sessionId);
    } catch (e) {
      console.warn('[PaymentService] Failed to track checkout session:', e);
    }
  }

  /**
   * Retrieve the tracked Stripe session ID from sessionStorage.
   * Returns the session ID that was saved before opening Stripe Checkout.
   */
  retrieveTrackedSessionId(): string | null {
    try {
      const sessionId = sessionStorage.getItem(this.SESSION_KEY);
      if (sessionId) {
        console.log('[PaymentService] Retrieved tracked session:', sessionId);
      }
      return sessionId;
    } catch (e) {
      console.warn('[PaymentService] Failed to retrieve tracked session:', e);
      return null;
    }
  }

  /**
   * Clear the tracked Stripe session ID from sessionStorage.
   * Call this after payment verification completes (success or failure).
   */
  clearTrackedSessionId(): void {
    try {
      sessionStorage.removeItem(this.SESSION_KEY);
      console.log('[PaymentService] Cleared tracked session');
    } catch (e) {
      console.warn('[PaymentService] Failed to clear tracked session:', e);
    }
  }
}
