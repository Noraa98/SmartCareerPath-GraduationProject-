import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router, NavigationEnd, RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';
import { UserDataService, Step } from './services/user-data.service';
import { AuthService } from './services/auth.service';
import { SidebarComponent } from './components/sidebar/sidebar.component';
import { SidebarService } from './services/sidebar.service';
import { FooterComponent } from './components/footer/footer.component';
import { ButtonComponent } from './components/ui/button/button.component';
import { filter, Subscription } from 'rxjs';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, SidebarComponent, FooterComponent, ButtonComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit, OnDestroy {
  currentStep: Step = 'registration';
  userName: string = '';
  showNavigation: boolean = false;
  showFooter: boolean = false;
  sidebarWidth: string = '16rem';
  sidebarVisible: boolean = false;
  private subscriptions = new Subscription();
  // routes that require authentication
  // routes that require authentication
  protectedRoutes: string[] = ['/features', '/cv-builder', '/cv-builder/preview', '/job-parser', '/ai-interviewer', '/ai-interviewer/report', '/recommendations', '/interests', '/quiz'];

  constructor(
    private router: Router,
    private userDataService: UserDataService,
    private sidebarService: SidebarService,
    private auth: AuthService
  ) {}

  ngOnInit(): void {
    // Restore name from stored JWT (if present) so UI shows user's name after full-page reloads
    try {
      const claims = this.auth.decodeToken();
      const fullName = claims?.fullName 
        || claims?.name 
        || claims?.['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name']
        || claims?.['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname']
        || null;
      console.log('[AppComponent.ngOnInit] Decoded claims, fullName from token:', fullName);
      if (fullName) {
        console.log('[AppComponent] Setting name from token:', fullName);
        this.userDataService.setName(fullName);
      } else {
        console.log('[AppComponent] No fullName in token, trying localStorage');
        // Try to restore from localStorage as fallback
        try {
          const storedName = localStorage.getItem('scp_user_fullname');
          if (storedName) {
            console.log('[AppComponent] Restoring name from localStorage:', storedName);
            this.userDataService.setName(storedName);
          }
        } catch {}
      }
    } catch (e) {
      console.warn('[AppComponent] Error restoring name from token:', e);
    }
    // Subscribe to route changes
    this.subscriptions.add(
      this.router.events.pipe(
        filter(event => event instanceof NavigationEnd)
      ).subscribe((event: any) => {
        this.updateStepFromRoute(event.url);
        // If the user navigated to a protected route but is not logged in, redirect to /login
        const cleanUrl = event.url.split('?')[0];
        if (this.protectedRoutes.includes(cleanUrl) && !this.auth.isLoggedIn()) {
          console.debug('[AppComponent] blocking navigation to protected route', cleanUrl);
          this.router.navigate(['/login']);
        }
      })
    );

    // Subscribe to user data changes
    this.subscriptions.add(
      this.userDataService.userData$.subscribe(userData => {
        this.userName = userData.name;
      })
    );

    // Subscribe to sidebar state changes
    this.subscriptions.add(
      this.sidebarService.isCollapsed$.subscribe(collapsed => {
        this.sidebarWidth = collapsed ? '4rem' : '16rem';
      })
    );

    // Subscribe to sidebar visibility changes
    this.subscriptions.add(
      this.sidebarService.isVisible$.subscribe(visible => {
        this.sidebarVisible = visible;
      })
    );

    // Global error guard: suppress benign SVG parse errors so runtime doesn't stop
    try {
      if (typeof window !== 'undefined' && window.addEventListener) {
        window.addEventListener('error', (ev: any) => {
          try {
            const msg = (ev && ev.message) ? String(ev.message) : '';
            if (/parsing the .*d\' attribute|d attribute/i.test(msg) || /An error occurred while parsing the 'd' attribute/i.test(msg)) {
              console.warn('[AppComponent] Suppressed SVG parse error:', msg);
              ev.preventDefault && ev.preventDefault();
              ev.stopImmediatePropagation && ev.stopImmediatePropagation();
            }
          } catch (e) {}
        }, { capture: true });

        window.addEventListener('unhandledrejection', (ev: any) => {
          try {
            const reason = ev && ev.reason ? String(ev.reason) : '';
            if (/SVG|d attribute/i.test(reason)) {
              console.warn('[AppComponent] Suppressed unhandledrejection due to SVG parse issue:', reason);
              ev.preventDefault && ev.preventDefault();
            }
          } catch (e) {}
        });
      }
    } catch (e) {}

    // Initial route check
    this.updateStepFromRoute(this.router.url);
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  private updateStepFromRoute(url: string): void {
    // Handle empty route or root route
    const cleanUrl = url.split('?')[0];
    const route = cleanUrl === '/' || cleanUrl === '' ? 'registration' : cleanUrl.substring(1);
    this.currentStep = (route as Step) || 'registration';
    
    // Hide sidebar on registration and login pages (initial pages when website first opens)
    // Also hide on empty/root route
    const hideSidebarRoutes = ['registration', 'login', ''];
    this.showNavigation = !hideSidebarRoutes.includes(route);
    
    // Update sidebar visibility based on route
    if (this.showNavigation) {
      this.sidebarService.show();
    } else {
      this.sidebarService.hide();
    }
    
    this.showFooter = ['recommendations', 'features', 'cv-builder', 'job-parser', 'ai-interviewer'].includes(this.currentStep);
  }

toggleSidebar() : void {
  this.sidebarVisible = !this.sidebarVisible;

  if (this.sidebarVisible) {
    // When opening via the toggle button, mark sidebar visible and
    // restore expanded state so labels are shown.
    this.sidebarService.show();
    this.sidebarService.expand();
  } else {
    // When closing, hide the sidebar and collapse to icons.
    this.sidebarService.hide();
    this.sidebarService.collapse();
  }
}


  onBack(): void {
    if (this.currentStep === 'features') {
      this.router.navigate(['/recommendations']);
    } else if (['cv-builder', 'job-parser', 'ai-interviewer'].includes(this.currentStep)) {
      this.router.navigate(['/features']);
    }
  }
}

