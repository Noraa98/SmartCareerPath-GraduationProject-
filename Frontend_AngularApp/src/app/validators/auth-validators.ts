import { ValidatorFn, AbstractControl, ValidationErrors, FormGroup } from '@angular/forms';

export const passwordRegex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$/;

export const BACKEND_ERRORS = {
  emailRequired: 'Email is required',
  invalidEmail: 'Invalid email format',
  passwordRequired: 'Password is required',
  passwordMinLength: 'Password must be at least 8 characters',
  passwordComplexity: 'Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character',
  passwordConfirmationRequired: 'Password confirmation is required',
  passwordsDoNotMatch: 'Passwords do not match',
  fullNameRequired: 'Full name is required',
  fullNameMinLength: 'Full name must be at least 2 characters',
  invalidPhone: 'Invalid phone number format',
  currentPasswordRequired: 'Current password is required',
  tokenRequired: 'Token is required'
};

export function passwordComplexityValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    if (!control) return null;
    const val = control.value;
    if (!val) return null; // required handled separately
    const s = String(val);
    if (s.length < 8) return { minlength: { requiredLength: 8, actualLength: s.length } };
    return passwordRegex.test(s) ? null : { passwordComplexity: true };
  };
}

export function matchFieldValidator(matchTo: string, errorKey = 'mismatch'): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    if (!control || !control.parent) return null;
    const parent = control.parent as FormGroup;
    const matchControl = parent.get(matchTo);
    if (!matchControl) return null;
    return control.value === matchControl.value ? null : { [errorKey]: true };
  };
}

export function groupMatchValidator(controlKey: string, matchToKey: string, errorKey = 'mismatch'): ValidatorFn {
  return (group: AbstractControl): ValidationErrors | null => {
    if (!group) return null;
    const fg = group as FormGroup;
    const a = fg.get(controlKey);
    const b = fg.get(matchToKey);
    if (!a || !b) return null;
    return a.value === b.value ? null : { [errorKey]: true };
  };
}

// Basic phone validator: allows +, digits, spaces, dashes, parentheses; max length checked in form control
export function phoneValidator(): ValidatorFn {
  const phonePattern = /^\+?[0-9\s\-()]{6,20}$/;
  return (control: AbstractControl): ValidationErrors | null => {
    const val = control?.value;
    if (!val) return null;
    return phonePattern.test(String(val)) ? null : { invalidPhone: true };
  };
}
