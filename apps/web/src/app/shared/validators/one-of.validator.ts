import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

export function oneOfValidator<const TAllowed extends readonly (string | number)[]>(
  allowedValues: TAllowed,
  errorKey = 'invalidValue',
): ValidatorFn {
  const allowedSet = new Set<string | number>(allowedValues as readonly (string | number)[]);

  return (control: AbstractControl): ValidationErrors | null => {
    const value = control.value as unknown;

    if (value == null || value == '') return null;

    if (typeof value !== 'string' && typeof value !== 'number') {
      return { [errorKey]: { value, allowedValues } };
    }

    return allowedSet.has(value) ? null : { [errorKey]: { value, allowedValues } };
  };
}
