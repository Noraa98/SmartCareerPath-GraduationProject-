import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { trigger, state, style, transition, animate } from '@angular/animations';
import { ButtonComponent } from '../ui/button/button.component';
import { InputComponent } from '../ui/input/input.component';
import { LabelComponent } from '../ui/label/label.component';
import { CardComponent, CardContentComponent } from '../ui/card/card.component';
import { AuthService } from '../../services/auth.service';
import { UserDataService } from '../../services/user-data.service';
import { IconService } from '../../services/icon.service';
import { BACKEND_ERRORS } from '../../validators/auth-validators';

@Component({
  selector: 'app-login-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, RouterModule, ButtonComponent, InputComponent, LabelComponent, CardComponent, CardContentComponent],
  templateUrl: './login-page.component.html',
  styleUrl: './login-page.component.css',
  animations: [
    trigger('fadeInUp', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(20px)' }),
        animate('600ms ease-out', style({ opacity: 1, transform: 'translateY(0)' }))
      ])
    ]),
    trigger('slideIn', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateX(-20px)' }),
        animate('400ms ease-out', style({ opacity: 1, transform: 'translateX(0)' }))
      ])
    ])
  ]
})
export class LoginPageComponent {
  form!: FormGroup;
  error: string = '';
  isLoading: boolean = false;
  backend = BACKEND_ERRORS;
  
  // Template binding properties
  email: string = '';
  password: string = '';
  rememberMe: boolean = false;
  emailError: string = '';
  passwordError: string = '';

  constructor(
    private auth: AuthService, 
    private router: Router,
    private userData: UserDataService,
    public iconService: IconService,
    private fb: FormBuilder
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required]],
      rememberMe: [false]
    });
  }

  validateEmail(): void {
    this.emailError = '';
    if (!this.email) {
      this.emailError = 'Email is required';
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(this.email)) {
      this.emailError = 'Please enter a valid email address';
    }
  }

  validatePassword(): void {
    this.passwordError = '';
    if (!this.password) {
      this.passwordError = 'Password is required';
    }
  }

  get emailFormControl() { return this.form.get('email'); }
  get passwordFormControl() { return this.form.get('password'); }

  onSubmit(): void {
    this.error = '';
    this.validateEmail();
    this.validatePassword();
    
    if (this.emailError || this.passwordError || !this.email || !this.password) {
      return;
    }

    this.isLoading = true;

    this.auth.login({ email: String(this.email).trim(), password: this.password, rememberMe: Boolean(this.rememberMe) })
      .subscribe({
        next: success => {
          this.isLoading = false;
          if (success) {
            // AuthService has already set user's full name (if returned by backend)
            if (this.rememberMe) {
              localStorage.setItem('rememberMe', 'true');
              localStorage.setItem('email', String(this.email || ''));
            }
            this.router.navigate(['/home']);
          } else {
            this.error = 'Invalid email or password. Please try again.';
          }
        },
        error: err => {
          this.isLoading = false;
          // Extract backend error message if available
          const backendMessage = err?.error?.message || err?.error?.error || err?.statusText;
          const statusCode = err?.status || '';
          console.error('[LoginPage] Login error:', { status: statusCode, message: backendMessage, fullError: err });
          
          if (statusCode === 401) {
            this.error = 'Invalid email or password.';
          } else if (statusCode === 400) {
            this.error = backendMessage || 'Invalid input. Please check your credentials.';
          } else {
            this.error = backendMessage || 'Login failed. Please try again.';
          }
        }
      });
  }

  onSocialLogin(provider: string): void {
    this.isLoading = true;
    // Simulate social login
    setTimeout(() => {
      this.isLoading = false;
      console.log(`Logging in with ${provider}`);
      // In a real app, this would redirect to OAuth provider
      this.router.navigate(['/home']);
    }, 1000);
  }
}
