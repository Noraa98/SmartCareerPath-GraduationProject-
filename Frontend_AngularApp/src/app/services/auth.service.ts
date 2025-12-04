import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { UserDataService } from './user-data.service';
import { HttpClient } from '@angular/common/http';
import { Observable, map, tap, BehaviorSubject } from 'rxjs';

const AUTH_TOKEN_KEY = 'scp_auth_token';

interface RegisterRequest {
  email: string;
  password: string;
  confirmPassword: string;
  fullName: string;
  phone?: string;
  roleName?: string;
}

interface LoginRequest {
  email: string;
  password: string;
  rememberMe?: boolean;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  // Observables for reactive role and name updates
  private roleSubject = new BehaviorSubject<string | null>(null);
  public role$ = this.roleSubject.asObservable();

  constructor(
    private router: Router,
    private userData: UserDataService,
    private http: HttpClient
  ) {
    // On init, restore role from current token if available
    try {
      // First check if there's a cached role (from recent payment/login)
      const cachedRole = this.getCachedRole();
      if (cachedRole) {
        console.log('[AuthService.constructor] Restoring cached role:', cachedRole);
        this.roleSubject.next(cachedRole);
        return;
      }
      
      // Otherwise extract role from token
      const currentRole = this.getUserRole();
      if (currentRole) {
        console.log('[AuthService.constructor] Restoring role from token:', currentRole);
        this.roleSubject.next(currentRole);
      }
    } catch (e) {
      console.warn('[AuthService.constructor] Error restoring role:', e);
    }
  }

  register(payload: RegisterRequest): Observable<any> {
    // Ensure default roleName
    if (!payload.roleName) payload.roleName = 'user';
    return this.http.post('http://localhost:5164/api/Auth/register', payload);
  }

  login(payload: LoginRequest): Observable<boolean> {
    console.log('[AuthService] Attempting login for email:', payload.email);
    return this.http.post<any>('http://localhost:5164/api/Auth/login', payload).pipe(
      map(response => {
        console.log('[AuthService] Login response received:', response);
        return { raw: response };
      }),
      tap(wrapper => {
        const response = wrapper.raw;
        let token = response?.token || response?.data?.token || response?.accessToken || response?.data?.accessToken;

        // Normalize token: strip leading 'Bearer ' if present
        if (typeof token === 'string' && token.startsWith('Bearer ')) {
          token = token.slice(7);
        }

        // store token respecting rememberMe
        if (token) {
          console.log('[AuthService] Token received, storing in', payload.rememberMe ? 'localStorage' : 'sessionStorage');
          if (payload.rememberMe) {
            localStorage.setItem(AUTH_TOKEN_KEY, token);
          } else {
            sessionStorage.setItem(AUTH_TOKEN_KEY, token);
          }
          // Decode and log token claims for debugging
          const claims = this.decodeToken(token);
          console.log('[AuthService] Decoded token claims:', claims);
          
          // Extract and emit role from token so Sidebar and other components pick it up immediately
          // Check Microsoft path FIRST (this is where backend puts the role)
          const role = claims?.['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']
            || claims?.['role'] 
            || claims?.['Role'] 
            || claims?.['roleName'] 
            || claims?.['RoleName']
            || null;
          console.log('[AuthService] Login: Extracted role from token claims:', role);
          if (role) {
            console.log('[AuthService] Login: Emitting role to subscribers:', role);
            this.notifyRoleChange(role);
          } else {
            console.warn('[AuthService] Login: No role found in token claims');
            // If no role in token, call getProfile to refresh role from backend
            // This handles the case where backend doesn't include role in token but has it in DB
            console.log('[AuthService] Login: No role in token, calling getProfile to refresh from backend...');
            this.getProfile().subscribe({
              next: (profile) => {
                console.log('[AuthService] Profile response:', profile);
                const profileRole = profile?.role || profile?.data?.role || profile?.roleDto?.roleName || null;
                if (profileRole) {
                  console.log('[AuthService] Login: Got role from profile:', profileRole);
                  this.notifyRoleChange(profileRole);
                }
              },
              error: (err) => console.warn('[AuthService] Failed to fetch profile after login:', err)
            });
          }
        } else {
          console.warn('[AuthService] No token in response');
        }

        // set user full name if available
        const fullName = response?.fullName || response?.data?.fullName || response?.user?.fullName;
        if (fullName) {
          console.log('[AuthService] Setting user name:', fullName);
          this.setNameFromFullName(fullName);
        }
      }),
      map(wrapper => {
        const response = wrapper.raw;
        const token = response?.token || response?.data?.token || response?.accessToken || response?.data?.accessToken;
        return !!token;
      })
    );
  }

  logout(): void {
    localStorage.removeItem(AUTH_TOKEN_KEY);
    sessionStorage.removeItem(AUTH_TOKEN_KEY);
    localStorage.removeItem('scp_cached_role');
    localStorage.removeItem('scp_user_fullname');
    this.userData.setName('');
    this.notifyRoleChange(null);
    this.router.navigate(['/login']);
  }

  isLoggedIn(): boolean {
    return !!(localStorage.getItem(AUTH_TOKEN_KEY) || sessionStorage.getItem(AUTH_TOKEN_KEY));
  }

  getToken(): string | null {
    return localStorage.getItem(AUTH_TOKEN_KEY) || sessionStorage.getItem(AUTH_TOKEN_KEY);
  }

  // Decode a JWT and return its payload claims (does not validate signature)
  decodeToken(token?: string): any | null {
    try {
      const raw = token || this.getToken();
      if (!raw) return null;
      const clean = raw.startsWith('Bearer ') ? raw.slice(7) : raw;
      const parts = clean.split('.');
      if (parts.length < 2) return null;
      const payload = parts[1];
      const base64 = payload.replace(/-/g, '+').replace(/_/g, '/');
      const padded = base64 + '==='.slice((base64.length + 3) % 4);
      const json = decodeURIComponent(atob(padded).split('').map(function(c) {
        return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
      }).join(''));
      return JSON.parse(json);
    } catch (e) {
      console.warn('Failed to decode token', e);
      return null;
    }
  }

  // Convenience: decode current token and log claims for debugging
  logTokenClaims(): void {
    const claims = this.decodeToken();
    try {
      console.debug('[AuthService] token claims:', claims);
    } catch {}
  }

  setNameFromFullName(fullName: string | undefined | null): void {
    if (!fullName) return;
    console.log('[AuthService.setNameFromFullName] Setting name and storing in localStorage:', fullName);
    this.userData.setName(fullName);
    // Also store in localStorage as a fallback when token doesn't have fullName
    try {
      localStorage.setItem('scp_user_fullname', fullName);
    } catch {}
  }

  // Emit role changes so components can subscribe to role updates
  private notifyRoleChange(role: string | null): void {
    console.log('[AuthService.notifyRoleChange] Emitting role:', role);
    this.roleSubject.next(role);
  }

  // Store a token directly (useful when backend returns an updated token after actions)
  setToken(token: string, remember = true): void {
    if (!token) return;
    let clean = typeof token === 'string' ? token : String(token);
    if (clean.startsWith('Bearer ')) clean = clean.slice(7);
    try {
      if (remember) {
        localStorage.setItem(AUTH_TOKEN_KEY, clean);
      } else {
        sessionStorage.setItem(AUTH_TOKEN_KEY, clean);
      }
      const claims = this.decodeToken(clean);
      if (claims) {
        // Extract fullName from various possible claim paths
        const fullName = claims['fullName'] 
          || claims['name'] 
          || claims['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] 
          || claims['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname']
          || null;
        if (fullName) this.setNameFromFullName(fullName);
        
        // Extract and emit new role — check Microsoft path FIRST
        const newRole = claims['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']
          || claims['role'] 
          || claims['Role'] 
          || claims['roleName'] 
          || claims['RoleName'] 
          || null;
        console.log('[AuthService.setToken] Extracted role from new token:', newRole);
        if (newRole) {
          console.log('[AuthService.setToken] Emitting new role to subscribers:', newRole);
          this.notifyRoleChange(newRole);
        }
      }
      // Clear any temporary cached role — the token is authoritative now
      try { 
        localStorage.removeItem('scp_cached_role');
        console.log('[AuthService.setToken] Cleared cached role since new token was set');
      } catch {}
      console.log('[AuthService] setToken stored token and updated local user data');
    } catch (e) {
      console.warn('[AuthService] Failed to set token', e);
    }
  }

  // Temporary cached role to allow frontend to reflect role changes until a new JWT is issued
  setCachedRole(role: string | null): void {
    try {
      if (role) {
        localStorage.setItem('scp_cached_role', role);
        // Notify subscribers of role change via cache
        this.notifyRoleChange(role);
      } else {
        localStorage.removeItem('scp_cached_role');
      }
    } catch (e) {
      console.warn('[AuthService] Failed to set cached role', e);
    }
  }

  getCachedRole(): string | null {
    try {
      return localStorage.getItem('scp_cached_role');
    } catch (e) {
      return null;
    }
  }

  // Verify email with token (do not persist token client-side)
  verifyEmail(payload: { email: string; token: string }) {
    return this.http.post('http://localhost:5164/api/auth/verify-email', payload);
  }

  // Fetch current user's profile (useful to refresh role/state after actions)
  getProfile(): Observable<any> {
    return this.http.get<any>('http://localhost:5164/api/Auth/me');
  }

  /**
   * Refresh user profile from backend and update local state.
   * Useful after payment completion to ensure we have the latest role/permissions.
   * Updates local user data and emits role changes to subscribers.
   */
  refreshUserProfile(): Observable<any> {
    console.log('[AuthService.refreshUserProfile] Fetching latest user profile from backend');
    return this.getProfile().pipe(
      tap((profile) => {
        console.log('[AuthService.refreshUserProfile] Profile response:', profile);
        
        // Update user name if provided
        const fullName = profile?.fullName || profile?.data?.fullName || profile?.user?.fullName;
        if (fullName) {
          console.log('[AuthService.refreshUserProfile] Updating full name:', fullName);
          this.setNameFromFullName(fullName);
        }

        // Update role if provided
        const newRole = profile?.role || profile?.roleName || profile?.data?.role || profile?.data?.roleName || profile?.user?.role || null;
        if (newRole) {
          console.log('[AuthService.refreshUserProfile] Found role in profile:', newRole);
          // Emit the role change so UI components pick it up
          this.notifyRoleChange(newRole);
          // Cache it temporarily in case token isn't refreshed yet
          this.setCachedRole(newRole);
        }

        // If profile includes a new token with updated claims, store it
        const newToken = profile?.token || profile?.accessToken || profile?.data?.token;
        if (newToken) {
          console.log('[AuthService.refreshUserProfile] Profile included new token; storing it');
          this.setToken(newToken, true);
        }
      })
    );
  }

  // Get user's role from JWT token
  getUserRole(): string | null {
    const claims = this.decodeToken();
    if (!claims) {
      console.warn('[AuthService.getUserRole] No claims found in token');
      // If no token claims, fall back to cached role if present
      try {
        const cached = this.getCachedRole();
        if (cached) return cached;
      } catch {}
      return null;
    }
    // Try common role claim names — check Microsoft path FIRST since that's what backend uses
    const role = claims['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']
      || claims['role'] 
      || claims['Role'] 
      || claims['roleName'] 
      || claims['RoleName'] 
      || null;
    console.debug('[AuthService.getUserRole] Extracted role:', role, 'from claims:', claims);
    // If backend recently updated role but token not refreshed, prefer cached role if present
    try {
      const cached = this.getCachedRole();
      if (cached) {
        console.debug('[AuthService.getUserRole] Found cached role, returning:', cached);
        return cached;
      }
    } catch {}
    return role;
  }

  // Emit role change for observable subscribers
  notifyRoleChangeFromCache(role: string | null): void {
    this.notifyRoleChange(role);
  }
}
