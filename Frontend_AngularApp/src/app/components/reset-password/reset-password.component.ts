import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute, Router } from '@angular/router';
import { BACKEND_ERRORS, passwordComplexityValidator, groupMatchValidator } from '../../validators/auth-validators';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './reset-password.component.html',
  styleUrl: './reset-password.component.css'
})
export class ResetPasswordComponent implements OnInit {
  form!: FormGroup;
  loading = false;
  successMessage = '';
  errorMessage = '';
  email = '';
  token = '';
  showNewPassword = false;
  showConfirmPassword = false;
  backend = BACKEND_ERRORS;

  // Password strength requirements - kept for UI display
  passwordStrengthRequirements = {
    minLength: false,
    hasUpperCase: false,
    hasLowerCase: false,
    hasDigit: false,
    hasSpecialChar: false
  };

  constructor(
    private fb: FormBuilder,
    private http: HttpClient,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    // Extract email and token from URL parameters
    this.route.queryParams.subscribe(params => {
      this.email = params['email'] || '';
      this.token = params['token'] || '';

      if (!this.email || !this.token) {
        this.errorMessage = 'Invalid reset link. Please request a new password reset.';
      }
    });

    this.form = this.fb.group({
      newPassword: ['', [Validators.required, Validators.minLength(8), passwordComplexityValidator()]],
      confirmNewPassword: ['', [Validators.required]]
    }, { validators: groupMatchValidator('newPassword', 'confirmNewPassword', 'mismatch') });

    // Listen to password changes for strength indicator
    this.form.get('newPassword')?.valueChanges.subscribe(() => {
      this.updatePasswordStrength();
    });
  }

  passwordStrengthValidator(control: AbstractControl): ValidationErrors | null {
    const value = control.value;
    if (!value) return null;

    const hasMinLength = value.length >= 8;
    const hasUpperCase = /[A-Z]/.test(value);
    const hasLowerCase = /[a-z]/.test(value);
    const hasDigit = /\d/.test(value);
    const hasSpecialChar = /[@$!%*?&]/.test(value);

    const isValid = hasMinLength && hasUpperCase && hasLowerCase && hasDigit && hasSpecialChar;

    return isValid ? null : { weakPassword: true };
  }

  passwordMatchValidator(group: AbstractControl): ValidationErrors | null {
    const newPassword = group.get('newPassword')?.value;
    const confirmPassword = group.get('confirmNewPassword')?.value;

    return newPassword === confirmPassword ? null : { passwordMismatch: true };
  }

  updatePasswordStrength(): void {
    const password = this.form.get('newPassword')?.value || '';

    this.passwordStrengthRequirements.minLength = password.length >= 8;
    this.passwordStrengthRequirements.hasUpperCase = /[A-Z]/.test(password);
    this.passwordStrengthRequirements.hasLowerCase = /[a-z]/.test(password);
    this.passwordStrengthRequirements.hasDigit = /\d/.test(password);
    this.passwordStrengthRequirements.hasSpecialChar = /[!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?]/.test(password);
  }

  get newPassword() {
    return this.form.get('newPassword');
  }

  get confirmNewPassword() {
    return this.form.get('confirmNewPassword');
  }

  get isPasswordStrong(): boolean {
    return Object.values(this.passwordStrengthRequirements).every(v => v);
  }

  togglePasswordVisibility(field: string): void {
    if (field === 'newPassword') {
      this.showNewPassword = !this.showNewPassword;
    } else if (field === 'confirmNewPassword') {
      this.showConfirmPassword = !this.showConfirmPassword;
    }
  }

  onSubmit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid || !this.email || !this.token) {
      return;
    }

    this.loading = true;
    this.successMessage = '';
    this.errorMessage = '';

    const { newPassword, confirmNewPassword } = this.form.value;

    this.http.post('http://localhost:5164/api/auth/reset-password', {
      email: this.email,
      token: this.token,
      newPassword,
      confirmNewPassword
    }).subscribe({
      next: (response: any) => {
        this.loading = false;
        this.successMessage = 'Password reset successfully! Redirecting to login...';
        setTimeout(() => {
          this.router.navigate(['/login']);
        }, 2000);
      },
      error: (err) => {
        this.loading = false;
        const errorMsg = err?.error?.message || err?.error?.error || '';

        if (errorMsg.toLowerCase().includes('invalid') || errorMsg.toLowerCase().includes('expired')) {
          this.errorMessage = 'Reset link expired. Please request a new password reset.';
        } else if (errorMsg.toLowerCase().includes('do not match')) {
          this.errorMessage = 'Passwords do not match. Please try again.';
        } else if (errorMsg.toLowerCase().includes('strong')) {
          this.errorMessage = 'Password is not strong enough. Must contain uppercase, lowercase, digit, and special character.';
        } else {
          this.errorMessage = errorMsg || 'Failed to reset password. Please try again.';
        }
      }
    });
  }

  goToLogin(): void {
    this.router.navigate(['/login']);
  }

  goToForgotPassword(): void {
    this.router.navigate(['/forgot-password']);
  }
}
