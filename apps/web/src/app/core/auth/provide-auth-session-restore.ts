import { EnvironmentProviders, inject, provideAppInitializer } from '@angular/core';
import { AuthService } from './auth.service';

export function provideAuthSessionRestore(): EnvironmentProviders {
  return provideAppInitializer(() => {
    inject(AuthService).startSessionRestore();
  });
}
