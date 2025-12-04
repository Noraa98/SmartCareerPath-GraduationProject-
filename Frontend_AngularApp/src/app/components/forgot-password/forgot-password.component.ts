import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { BACKEND_ERRORS } from '../../validators/auth-validators';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './forgot-password.component.html',
  styleUrl: './forgot-password.component.css'
})
export class ForgotPasswordComponent implements OnInit {
  form!: FormGroup;
  loading = false;
  successMessage = '';
  errorMessage = '';
  backend = BACKEND_ERRORS;

  constructor(
    private fb: FormBuilder,
    private http: HttpClient
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      email: ['', [Validators.required, Validators.email]]
    });
  }

  get email() {
    return this.form.get('email');
  }

  onSubmit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid) return;

    this.loading = true;
    this.successMessage = '';
    this.errorMessage = '';

    const { email } = this.form.value;

    this.http.post('http://localhost:5164/api/auth/forgot-password', { email }).subscribe({
      next: (response: any) => {
        this.loading = false;
        this.successMessage = 'Check your email for password reset instructions';
        this.form.reset();
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = err?.error?.message || err?.error?.error || 'Failed to send reset email. Please try again.';
      }
    });
  }

  goToLogin(): void {
    window.location.href = '/login';
  }
}
