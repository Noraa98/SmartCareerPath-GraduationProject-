import { Component, OnInit, ElementRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-verify-email',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './verify-email.component.html',
  styleUrl: './verify-email.component.css'
})
export class VerifyEmailComponent implements OnInit {
  @ViewChild('banner') bannerRef?: ElementRef;

  form!: FormGroup;
  loading = false;
  successMessage = '';
  errorMessage = '';

  // token/email from query params when present
  emailFromQuery = '';
  tokenFromQuery = '';

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private auth: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      token: ['', [Validators.required]]
    });

    // Read query params once
    this.route.queryParams.subscribe(params => {
      const email = params['email'] || '';
      const token = params['token'] || '';
      this.emailFromQuery = email;
      this.tokenFromQuery = token;

      // If both present, auto-verify once
      if (email && token) {
        // populate form for accessibility but do not store token anywhere
        this.form.patchValue({ email, token });
        this.verify({ email, token });
      }
    });
  }

  get email() {
    return this.form.get('email');
  }

  get token() {
    return this.form.get('token');
  }

  verify(payload?: { email: string; token: string }) {
    const body = payload || this.form.value;

    if (!body?.email || !body?.token) {
      this.errorMessage = 'Email and token are required';
      this.focusBanner();
      return;
    }

    if (this.form.invalid && !payload) {
      // mark controls touched to show validation
      this.form.markAllAsTouched();
      const firstInvalid = this.form.invalid ? Object.keys(this.form.controls).find(k => this.form.get(k)?.invalid) : null;
      if (firstInvalid) {
        const el = document.getElementById(firstInvalid);
        el?.focus();
      }
      return;
    }

    this.loading = true;
    this.successMessage = '';
    this.errorMessage = '';

    this.auth.verifyEmail({ email: body.email, token: body.token }).subscribe({
      next: (res: any) => {
        this.loading = false;
        this.successMessage = 'Email verified successfully! Redirecting to login...';
        this.focusBanner();
        setTimeout(() => this.router.navigate(['/login']), 2000);
      },
      error: (err: any) => {
        this.loading = false;
        const msg = err?.error?.message || err?.error?.error || err?.message || 'Failed to verify email. Please try again.';
        if (typeof msg === 'string') {
          this.errorMessage = msg;
        } else {
          this.errorMessage = 'Failed to verify email. Please try again.';
        }
        this.focusBanner();
      }
    });
  }

  focusBanner() {
    // focus the banner for screen readers
    setTimeout(() => {
      try {
        this.bannerRef?.nativeElement?.focus();
      } catch {}
    }, 50);
  }
}
