import { Injectable } from '@angular/core';
import { CanActivate, Router, UrlTree } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Injectable({ providedIn: 'root' })
export class AuthGuard implements CanActivate {
  constructor(private auth: AuthService, private router: Router) {}

  canActivate(): boolean | UrlTree {
    const logged = this.auth.isLoggedIn();
    console.debug('[AuthGuard] canActivate =>', { loggedIn: logged });
    if (logged) return true;
    console.debug('[AuthGuard] redirecting to /login');
    return this.router.parseUrl('/login');
  }
}
