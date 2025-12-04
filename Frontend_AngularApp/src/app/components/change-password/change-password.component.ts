import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../services/auth.service';
import { ButtonComponent } from '../ui/button/button.component';
import { BACKEND_ERRORS, passwordComplexityValidator, groupMatchValidator } from '../../validators/auth-validators';

@Component({
  selector: 'app-change-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, ButtonComponent],
  templateUrl: './change-password.component.html',
  styleUrl: './change-password.component.css'
})
export class ChangePasswordComponent implements OnInit {
  form!: FormGroup;
  loading = false;
  successMessage = '';
  errorMessage = '';
  showPassword = false;
  showNewPassword = false;
  showConfirmPassword = false;
  backend = BACKEND_ERRORS;

  constructor(
    private fb: FormBuilder,
    private http: HttpClient,
    private router: Router,
    private auth: AuthService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
  }

  initializeForm(): void {
    this.form = this.fb.group({
      currentPassword: ['', [Validators.required]],
      newPassword: ['', [Validators.required, Validators.minLength(8), passwordComplexityValidator()]],
      confirmNewPassword: ['', [Validators.required]]
    }, { validators: groupMatchValidator('newPassword', 'confirmNewPassword', 'mismatch') });
  }

  onSubmit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid) {
      return;
    }

    this.loading = true;
    this.errorMessage = '';
    this.successMessage = '';

    const payload = {
      currentPassword: this.form.get('currentPassword')?.value,
      newPassword: this.form.get('newPassword')?.value,
      confirmNewPassword: this.form.get('confirmNewPassword')?.value
    };

    console.log('[ChangePassword] Submitting change password request');
    
    this.http.post('http://localhost:5164/api/Auth/change-password', payload).subscribe({
      next: (response: any) => {
        console.log('[ChangePassword] Password changed successfully');
        this.successMessage = 'Password changed successfully! Redirecting to home...';
        this.loading = false;
        
        // Redirect to home after 2 seconds
        setTimeout(() => {
          this.router.navigate(['/home']);
        }, 2000);
      },
      error: (err) => {
        console.error('[ChangePassword] Error:', err);
        this.loading = false;
        
        const errorMsg = err?.error?.message || err?.error?.error || 'Failed to change password';
        this.errorMessage = errorMsg;
      }
    });
  }

  togglePasswordVisibility(field: string): void {
    if (field === 'current') this.showPassword = !this.showPassword;
    if (field === 'new') this.showNewPassword = !this.showNewPassword;
    if (field === 'confirm') this.showConfirmPassword = !this.showConfirmPassword;
  }

  goBack(): void {
    this.router.navigate(['/home']);
  }
}
