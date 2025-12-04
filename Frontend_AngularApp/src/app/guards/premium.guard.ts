import { Injectable } from '@angular/core';
import { CanActivate, Router, UrlTree } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Injectable({ providedIn: 'root' })
export class PremiumGuard implements CanActivate {
  constructor(private auth: AuthService, private router: Router) {}

  canActivate(): boolean | UrlTree {
    const loggedIn = this.auth.isLoggedIn();
    console.debug('[PremiumGuard] canActivate =>', { loggedIn });

    if (!loggedIn) {
      console.debug('[PremiumGuard] Not logged in, redirecting to /login');
      return this.router.parseUrl('/login');
    }

    const role = this.auth.getUserRole();
    console.debug('[PremiumGuard] User role:', role);

    if (role === 'Premium') {
      console.debug('[PremiumGuard] User is Premium, allowing access');
      return true;
    }

    console.debug('[PremiumGuard] User is not Premium, redirecting to /plans');
    return this.router.parseUrl('/plans');
  }
}
