import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter, withInMemoryScrolling } from '@angular/router';

import { routes } from './app.routes';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { auth401Interceptor } from '@app/core/auth/auth-401.interceptor';
import { authInterceptor } from '@app/core/auth/auth.interceptor';
import { withCredentialsInterceptor } from '@app/core/auth/refresh.interceptor';
import { provideAuthSessionRestore } from '@app/core/auth/provide-auth-session-restore';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideHttpClient(
      withInterceptors([withCredentialsInterceptor, authInterceptor, auth401Interceptor]),
    ),
    provideAuthSessionRestore(),
    provideRouter(
      routes,
      withInMemoryScrolling({
        anchorScrolling: 'enabled',
        scrollPositionRestoration: 'enabled',
      }),
    ),
  ],
};
