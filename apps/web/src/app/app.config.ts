import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { auth401Interceptor } from './core/auth/auth-401.interceptor';
import { authInterceptor } from './core/auth/auth.interceptor';
import { provideAuthSessionRestore } from './core/auth/provide-auth-session-restore';
import { withCredentialsInterceptor } from './core/auth/refresh.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(
      withInterceptors([withCredentialsInterceptor, authInterceptor, auth401Interceptor]),
    ),
    provideAuthSessionRestore(),
  ],
};
