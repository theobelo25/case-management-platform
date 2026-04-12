import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

export interface PasswordsMatchValidatorOptions {
  passwordKey?: string;
  confirmKey?: string;
}

export function passwordsMatchValidator(options?: PasswordsMatchValidatorOptions): ValidatorFn {
  const passwordKey = options?.passwordKey ?? 'password';
  const confirmKey = options?.confirmKey ?? 'confirmPassword';

  return (control: AbstractControl): ValidationErrors | null => {
    const password = (control.get(passwordKey)?.value as string)?.trim() ?? '';
    const confirmPassword = (control.get(confirmKey)?.value as string)?.trim() ?? '';

    if (!password || !confirmPassword) {
      return null;
    }

    return password === confirmPassword ? null : { passwordsMismatch: true };
  };
}

export function updateProfilePasswordGroupValidator(options?: {
  oldKey?: string;
  newKey?: string;
  confirmKey?: string;
}): ValidatorFn {
  const oldKey = options?.oldKey ?? 'currentPassword';
  const newKey = options?.newKey ?? 'newPassword';
  const confirmKey = options?.confirmKey ?? 'confirmNewPassword';

  return (control: AbstractControl): ValidationErrors | null => {
    const oldV = (control.get(oldKey)?.value as string)?.trim() ?? '';
    const newV = (control.get(newKey)?.value as string)?.trim() ?? '';
    const confirmV = (control.get(confirmKey)?.value as string)?.trim() ?? '';
    const changing = newV.length > 0 || confirmV.length > 0;
    if (!changing) {
      return null;
    }

    return oldV.length === 0 ? { currentPasswordRequired: true } : null;
  };
}

export function optionalMinLength(min: number): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const v = ((control.value as string) ?? '').trim();
    if (v.length === 0) {
      return null;
    }

    return v.length < min ? { minlength: { requiredLength: min, actualLength: v.length } } : null;
  };
}
