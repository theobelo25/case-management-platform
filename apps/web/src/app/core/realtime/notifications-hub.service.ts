import { inject, Injectable } from '@angular/core';
import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
} from '@microsoft/signalr';
import { API_BASE_URL } from '@app/core/api/api-base-url.token';
import { AuthService } from '@app/core/auth/auth.service';
import { InAppNotificationsService } from './in-app-notifications.service';

function joinApiPath(baseUrl: string, path: string): string {
  const base = baseUrl.replace(/\/+$/, '');
  const p = path.replace(/^\/+/, '');
  if (!base) {
    return `/${p}`;
  }
  return base.startsWith('/') ? `${base}/${p}` : `${base}/${p}`;
}

function hubBaseUrlFromApiBase(baseUrl: string): string {
  const trimmed = baseUrl.trim().replace(/\/+$/, '');
  if (!trimmed) {
    return '';
  }

  // API calls use `/api`; realtime hub is exposed at `/hubs/*`.
  if (trimmed === '/api') {
    return '';
  }
  if (trimmed.endsWith('/api')) {
    return trimmed.slice(0, -4);
  }
  return trimmed;
}

/**
 * SignalR client for {@link NotificationsHub} at `/hubs/notifications`.
 * Uses `accessTokenFactory` so the JWT is sent as `?access_token=...`, matching the API
 * `JwtBearerEvents.OnMessageReceived` for `/hubs` paths.
 */
@Injectable({ providedIn: 'root' })
export class NotificationsHubService {
  private readonly apiBaseUrl = inject(API_BASE_URL);
  private readonly auth = inject(AuthService);
  private readonly inAppNotifications = inject(InAppNotificationsService);

  private hubConnection: HubConnection | null = null;
  private handlersBound = false;

  get connection(): HubConnection | null {
    return this.hubConnection;
  }

  /**
   * Builds (once) and starts the hub when a token exists; stops when there is no token.
   */
  async ensureStarted(): Promise<void> {
    const token = this.auth.getAccessToken();
    if (!token) {
      await this.stop();
      return;
    }

    if (!this.hubConnection) {
      this.hubConnection = new HubConnectionBuilder()
        .withUrl(joinApiPath(hubBaseUrlFromApiBase(this.apiBaseUrl), 'hubs/notifications'), {
          accessTokenFactory: () => this.auth.getAccessToken() ?? '',
        })
        .withAutomaticReconnect()
        .build();
      this.handlersBound = false;
    }

    const hc = this.hubConnection;
    if (!this.handlersBound) {
      const onPayload = (payload: unknown) => {
        this.inAppNotifications.ingestHubPayload(payload);
      };
      // ASP.NET Core Hub<T> invokes client interface methods using C# names (PascalCase).
      hc.on('NotificationReceived', onPayload);
      hc.on('notificationReceived', onPayload);
      this.handlersBound = true;
    }
    if (hc.state === HubConnectionState.Disconnected) {
      await hc.start();
    }
  }

  async stop(): Promise<void> {
    if (!this.hubConnection) {
      return;
    }
    if (this.hubConnection.state !== HubConnectionState.Disconnected) {
      await this.hubConnection.stop();
    }
    this.hubConnection = null;
    this.handlersBound = false;
  }
}
