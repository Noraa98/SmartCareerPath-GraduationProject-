import { Injectable } from '@angular/core';
import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { Router } from '@angular/router';
import { catchError } from 'rxjs/operators';

const AUTH_TOKEN_KEY = 'scp_auth_token';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  constructor(private router: Router) {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    // Look for tokens in several common storage keys
    const token = localStorage.getItem(AUTH_TOKEN_KEY)
      || sessionStorage.getItem(AUTH_TOKEN_KEY)
      || localStorage.getItem('token')
      || sessionStorage.getItem('token')
      || localStorage.getItem('accessToken')
      || sessionStorage.getItem('accessToken')
      || localStorage.getItem('authToken')
      || sessionStorage.getItem('authToken');

    // Debug log token presence (do NOT log tokens in production)
    console.log('[AuthInterceptor] ===== REQUEST INTERCEPT =====');
    console.log('[AuthInterceptor] Full URL:', req.url);
    console.log('[AuthInterceptor] URL object test:', {
      url: req.url,
      method: req.method,
      body: req.body
    });
    console.log('[AuthInterceptor] Token present:', !!token);
    if (token) {
      console.log('[AuthInterceptor] Token length:', token.length);
      console.log('[AuthInterceptor] Token first 50 chars:', token.substring(0, 50));
    }

    // Determine request path - try multiple approaches
    let pathname = '';
    let isApiRequest = false;

    // Approach 1: Direct URL parse
    try {
      const url = new URL(req.url);
      pathname = url.pathname;
      console.log('[AuthInterceptor] Approach 1 (URL parse):', { pathname, href: url.href });
    } catch (e) {
      console.log('[AuthInterceptor] Approach 1 failed:', e);
    }

    // Approach 2: Relative URL with origin
    if (!pathname) {
      try {
        const url = new URL(req.url, window.location.origin);
        pathname = url.pathname;
        console.log('[AuthInterceptor] Approach 2 (relative URL):', { pathname, href: url.href });
      } catch (e) {
        console.log('[AuthInterceptor] Approach 2 failed:', e);
      }
    }

    // Approach 3: String matching (fallback)
    if (!pathname) {
      pathname = req.url;
      console.log('[AuthInterceptor] Approach 3 (string):', { pathname });
    }

    // Check if this is an API request multiple ways
    isApiRequest = pathname.includes('/api') || req.url.includes('/api/Auth') || req.url.includes('/api/payment');
    
    console.log('[AuthInterceptor] Final pathname:', pathname);
    console.log('[AuthInterceptor] Is API request:', isApiRequest);

    // If token exists, attach header for API requests
    if (token && isApiRequest) {
      // Normalize 'Bearer ' prefix
      const raw = token.startsWith('Bearer ') ? token.slice(7) : token;

      // Decode payload to check expiry and log claims
      try {
        const parts = raw.split('.');
        if (parts.length >= 2) {
          const payload = parts[1];
          const base64 = payload.replace(/-/g, '+').replace(/_/g, '/');
          const padded = base64 + '==='.slice((base64.length + 3) % 4);
          const json = decodeURIComponent(atob(padded).split('').map(function(c) {
            return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
          }).join(''));
          const claims = JSON.parse(json);
          
          // Log decoded claims for debugging
          console.log('[AuthInterceptor] Decoded claims:', {
            sub: claims.sub,
            email: claims.email,
            iat: new Date(claims.iat * 1000).toISOString(),
            exp: new Date(claims.exp * 1000).toISOString(),
            iss: claims.iss,
            aud: claims.aud
          });

          // Check if token is expired
          const now = Math.floor(Date.now() / 1000);
          if (claims && typeof claims.exp === 'number' && claims.exp < now) {
            console.warn('[AuthInterceptor] Token expired at', new Date(claims.exp * 1000).toISOString());
            // clear stored token and redirect to login
            localStorage.removeItem(AUTH_TOKEN_KEY);
            sessionStorage.removeItem(AUTH_TOKEN_KEY);
            localStorage.removeItem('token');
            sessionStorage.removeItem('token');
            localStorage.removeItem('accessToken');
            sessionStorage.removeItem('accessToken');
            localStorage.removeItem('authToken');
            sessionStorage.removeItem('authToken');
            console.log('[AuthInterceptor] Cleared all tokens, redirecting to /login');
            this.router.navigate(['/login']);
            return throwError(() => new HttpErrorResponse({ status: 401, statusText: 'Token expired' }));
          }
        }
      } catch (err) {
        // decoding failed - continue but log
        console.warn('[AuthInterceptor] Failed to decode token:', err);
      }

      const headerValue = `Bearer ${raw}`;
      console.log('[AuthInterceptor] ✓ ATTACHING Authorization header');
      console.log('[AuthInterceptor] Header value:', headerValue.substring(0, 20) + '...');
      
      const cloned = req.clone({ setHeaders: { Authorization: headerValue } });
      
      console.log('[AuthInterceptor] Cloned request headers:', {
        authorization: cloned.headers.get('Authorization') ? 'Present' : 'Missing',
        allHeaders: cloned.headers.keys()
      });
      console.log('[AuthInterceptor] ===== END INTERCEPT (WITH HEADER) =====');
      
      // Handle 401 responses
      return next.handle(cloned).pipe(
        catchError((error: HttpErrorResponse) => {
          if (error.status === 401) {
            console.error('[AuthInterceptor] Received 401 Unauthorized response:', error);
            // Clear all tokens
            localStorage.removeItem(AUTH_TOKEN_KEY);
            sessionStorage.removeItem(AUTH_TOKEN_KEY);
            localStorage.removeItem('token');
            sessionStorage.removeItem('token');
            localStorage.removeItem('accessToken');
            sessionStorage.removeItem('accessToken');
            localStorage.removeItem('authToken');
            sessionStorage.removeItem('authToken');
            console.log('[AuthInterceptor] Cleared all tokens after 401, redirecting to /login');
            this.router.navigate(['/login']);
          }
          return throwError(() => error);
        })
      );
    }

    console.log('[AuthInterceptor] ✗ NO HEADER ATTACHED (token missing or not API request)');
    console.log('[AuthInterceptor] ===== END INTERCEPT (NO HEADER) =====');
    
    // Even if no token, handle 401 responses
    return next.handle(req).pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.status === 401) {
          console.error('[AuthInterceptor] Received 401 Unauthorized response (no token case):', error);
          // Clear all tokens
          localStorage.removeItem(AUTH_TOKEN_KEY);
          sessionStorage.removeItem(AUTH_TOKEN_KEY);
          localStorage.removeItem('token');
          sessionStorage.removeItem('token');
          localStorage.removeItem('accessToken');
          sessionStorage.removeItem('accessToken');
          localStorage.removeItem('authToken');
          sessionStorage.removeItem('authToken');
          console.log('[AuthInterceptor] Cleared all tokens after 401, redirecting to /login');
          this.router.navigate(['/login']);
        }
        return throwError(() => error);
      })
    );
  }
}
