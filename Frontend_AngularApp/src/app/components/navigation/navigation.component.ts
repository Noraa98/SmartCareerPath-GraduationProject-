import { Component, Input, Output, EventEmitter } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ButtonComponent } from '../ui/button/button.component';

@Component({
  selector: 'app-navigation',
  standalone: true,
  imports: [CommonModule, RouterModule, ButtonComponent],
  templateUrl: './navigation.component.html',
  styleUrl: './navigation.component.css'
})
export class NavigationComponent {
  @Input() showProfile: boolean = false;
  @Input() userName: string = '';
  @Input() showBack: boolean = true;
  @Output() backClick = new EventEmitter<void>();

  showDropdown: boolean = false;

  // protected routes that require authentication
  protectedRoutes: string[] = ['/features', '/recommendations', '/interests', '/quiz'];

  constructor(private auth: AuthService) {}

  getTargetRoute(route: string): string {
    // Delay importing AuthService here to avoid circular deps in tests; use window storage check
    const token = localStorage.getItem('scp_auth_token') || sessionStorage.getItem('scp_auth_token');
    if (this.protectedRoutes.includes(route) && !token) return '/login';
    return route;
  }

  logout(): void {
    this.auth.logout();
  }

  handleBack(): void {
    this.backClick.emit();
  }

  toggleDropdown(): void {
    this.showDropdown = !this.showDropdown;
  }
}

