import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { trigger, style, transition, animate } from '@angular/animations';
import { ButtonComponent } from '../ui/button/button.component';
import { InputComponent } from '../ui/input/input.component';
import { LabelComponent } from '../ui/label/label.component';
import { CardComponent, CardContentComponent } from '../ui/card/card.component';
import { UserDataService } from '../../services/user-data.service';
import { AuthService } from '../../services/auth.service';
import { IconService } from '../../services/icon.service';
import { BACKEND_ERRORS, passwordComplexityValidator, phoneValidator, groupMatchValidator } from '../../validators/auth-validators';

@Component({
  selector: 'app-registration-page',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    RouterModule,
    ButtonComponent,
    InputComponent,
    LabelComponent,
    CardComponent,
    CardContentComponent
  ],
  templateUrl: './registration-page.component.html',
  styleUrl: './registration-page.component.css',
  animations: [
    trigger('fadeInUp', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(20px)' }),
        animate('600ms ease-out', style({ opacity: 1, transform: 'translateY(0)' }))
      ])
    ]),
    trigger('scaleIn', [
      transition(':enter', [
        style({ opacity: 0, transform: 'scale(0)' }),
        animate('600ms 200ms ease-out', style({ opacity: 1, transform: 'scale(1)' }))
      ])
    ])
  ]
})
export class RegistrationPageComponent {
  form!: FormGroup;
  backend = BACKEND_ERRORS;
  
  // Template binding properties
  name: string = '';
  phone: string = '';
  email: string = '';
  password: string = '';
  confirmPassword: string = '';

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private userDataService: UserDataService,
    private auth: AuthService,
    public iconService: IconService
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(8), passwordComplexityValidator()]],
      confirmPassword: ['', [Validators.required]],
      fullName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(150)]],
      phone: ['', [Validators.maxLength(20), phoneValidator()]],
      roleName: ['user', [Validators.maxLength(64)]]
    }, { validators: groupMatchValidator('password', 'confirmPassword', 'mismatch') });
  }

  onSubmit(): void {
    // Validate individual properties from template bindings
    if (!this.name || !this.email || !this.password || !this.confirmPassword) {
      return;
    }

    if (this.password !== this.confirmPassword) {
      return;
    }

    const payload: any = {
      email: this.email,
      password: this.password,
      confirmPassword: this.confirmPassword,
      fullName: this.name,
    };
    if (this.phone) payload.phone = this.phone;

    this.auth.register(payload).subscribe({
      next: () => this.router.navigate(['/login']),
      error: (err) => {
        console.error('Registration failed', err);
        // show backend error via alert or UI if desired; keep routing behavior
        this.router.navigate(['/login']);
      }
    });
  }
}


// with phone number
// export class RegistrationPageComponent {

//   // ========= Form Fields =========
//   name: string = '';
//   phone: string = '';   // تمت إضافته
//   email: string = '';
//   password: string = '';

//   constructor(
//     private router: Router,
//     private userDataService: UserDataService,
//     public iconService: IconService
//   ) {}

//   onSubmit(): void {
//     // ========= Validation =========
//     if (this.name && this.phone && this.email && this.password) {

//       // احفظ البيانات لو هتستخدميها بعدين
//       this.userDataService.setName(this.name);
//       // this.userDataService.setPhone(this.phone);  ← لو عايزة تحطيه

//       // انقلي المستخدم للخطوة التالية
//       this.router.navigate(['/interests']);
//     }
//   }
// }
