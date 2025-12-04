import { Component, Input, OnInit, OnDestroy, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, NavigationEnd } from '@angular/router';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { AuthService } from '../../services/auth.service';
import { ButtonComponent } from '../ui/button/button.component';
import { SidebarService } from '../../services/sidebar.service';
import { icons } from '../../utils/icons';
import { filter, Subscription } from 'rxjs';

interface MenuItem {
  label: string;
  route: string;
  iconName: string;
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterModule, ButtonComponent],
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SidebarComponent implements OnInit, OnDestroy {
  @Input() showProfile: boolean = false;
  @Input() userName: string = '';
  @Input() showBack: boolean = true;

  isCollapsed: boolean = false;
  activeRoute: string = '';
  showDropdown: boolean = false;
  userRole: string | null = null;

  menuItems: MenuItem[] = [
    { label: 'Home', route: '/home', iconName: 'home' },
    { label: 'Features', route: '/features', iconName: 'sparkles' },
    { label: 'CV Builder', route: '/cv-builder', iconName: 'documentText' },
    { label: 'Job Parser', route: '/job-parser', iconName: 'magnifyingGlass' },
    { label: 'AI Interviewer', route: '/ai-interviewer', iconName: 'chatBubbleEllipses' },
    { label: 'Career Quiz ', route: '/interests', iconName: 'fileMark' },
    // { label: 'Recommendations', route: '/recommendations', iconName: 'lightbulb' },
    // { label: 'Profile', route: '/profile', iconName: 'user' }
  ];
  

  private subscriptions = new Subscription();

  constructor(
    private router: Router,
    private sidebarService: SidebarService,
    private sanitizer: DomSanitizer,
    private auth: AuthService,
    private cdr: ChangeDetectorRef
  ) {}

  // Routes that require authentication
  protectedRoutes: string[] = [
    '/features',
    '/cv-builder',
    '/cv-builder/preview',
    '/job-parser',
    '/ai-interviewer',
    '/ai-interviewer/report',
    '/recommendations',
    '/interests',
    '/quiz'
  ];

  getTargetRoute(route: string): string {
    if (this.protectedRoutes.includes(route) && !this.auth.isLoggedIn()) {
      return '/login';
    }
    return route;
  }

  getTitle(item: MenuItem): string {
    if (this.isCollapsed) return item.label;
    if (this.protectedRoutes.includes(item.route) && !this.auth.isLoggedIn()) return 'Login required';
    return '';
  }

  isPremiumFeature(route: string): boolean {
    return ['/job-parser', '/ai-interviewer', '/ai-interviewer/report'].includes(route);
  }

  isPremium(): boolean {
    console.log('[Sidebar.isPremium] Checking if premium. userRole:', this.userRole, 'Result:', this.userRole === 'Premium');
    return this.userRole === 'Premium';
  }

  isFeatureDisabled(route: string): boolean {
    return this.isPremiumFeature(route) && !this.isPremium();
  }

  onMenuItemClick(event: Event, item: MenuItem): void {
    if (this.isFeatureDisabled(item.route)) {
      event.preventDefault();
      return;
    }
  }

  logout(): void {
    this.auth.logout();
  }

  getIconHtml(iconName: string): SafeHtml {
    const iconKey = iconName as keyof typeof icons;
    const iconSvg = icons[iconKey] || '';
    return this.sanitizer.bypassSecurityTrustHtml(iconSvg);
  }

  ngOnInit(): void {
    // Set initial active route
    this.activeRoute = this.router.url.split('?')[0];
    
    // Subscribe to role changes from AuthService so premium features update reactively
    this.subscriptions.add(
      this.auth.role$.subscribe(role => {
        console.log('[Sidebar] Role changed to:', role);
        this.userRole = role;
        // Trigger change detection since we're using OnPush strategy
        this.cdr.markForCheck();
      })
    );
    
    // Set initial role from current token
    const initialRole = this.auth.getUserRole();
    console.log('[Sidebar.ngOnInit] Setting initial role:', initialRole);
    this.userRole = initialRole;
    this.cdr.markForCheck();

    // Subscribe to route changes
    this.subscriptions.add(
      this.router.events.pipe(
        filter(event => event instanceof NavigationEnd)
      ).subscribe((event: any) => {
        this.activeRoute = event.url.split('?')[0];
        this.cdr.markForCheck();
      })
    );

    // Subscribe to sidebar service for collapsed state
    this.subscriptions.add(
      this.sidebarService.isCollapsed$.subscribe(collapsed => {
        this.isCollapsed = collapsed;
        this.cdr.markForCheck();
      })
    );

    // Subscribe to sidebar visibility state
    this.subscriptions.add(
      this.sidebarService.isVisible$.subscribe(visible => {
        // When the sidebar becomes visible again, ensure it's expanded
        // so labels are shown. Parent may only toggle visibility, so
        // reset the collapsed state when shown to restore labels.
        if (visible) {
          this.sidebarService.expand();
        }
        this.cdr.markForCheck();
      })
    );
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  toggleSidebar(): void {
    this.sidebarService.toggle();
  }

  closeSidebar(): void {
    this.sidebarService.collapse(); 
  }


  toggleDropdown(): void {
    this.showDropdown = !this.showDropdown;
  }

  isActiveRoute(route: string): boolean {
    if (route === '/home') {
      return this.activeRoute === '/home' || this.activeRoute === '/';
    }
    return this.activeRoute.startsWith(route);
  }

  navigateToSubscription(): void {
    // Navigate to subscription/plans page
    this.router.navigate(['/plans']);
  }
}

