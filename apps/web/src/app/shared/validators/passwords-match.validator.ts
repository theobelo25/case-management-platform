import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

/** Cross-field validator: `password` and `confirmPassword` must match when both are set. */
export function passwordsMatchValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const password = control.get('password')?.value;
    const confirmPassword = control.get('confirmPassword')?.value;

    if (!password || !confirmPassword) {
      return null;
    }

    return password === confirmPassword ? null : { passwordsMismatch: true };
  };
}
