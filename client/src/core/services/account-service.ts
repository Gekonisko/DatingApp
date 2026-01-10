import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { LoginCreds, RegisterCreds, User } from '../../types/user';
import { tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { LikesService } from './likes-service';
import { PresenceService } from './presence-service';
import { HubConnectionState } from '@microsoft/signalr';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  private http = inject(HttpClient, { optional: true } as any);
  private likesService = inject(LikesService, { optional: true } as any);
  private presenceService = inject(PresenceService, { optional: true } as any);
  currentUser = signal<User | null>(null);
  private baseUrl = environment.apiUrl;

  register(creds: RegisterCreds) {
    return this.http.post<User>(this.baseUrl + 'account/register', creds,
      { withCredentials: true }).pipe(
        tap(user => {
          if (user) {
            this.setCurrentUser(user);
            this.startTokenRefreshInterval();
          }
        })
      )
  }

  login(creds: LoginCreds) {
    return this.http.post<User>(this.baseUrl + 'account/login', creds,
      { withCredentials: true }).pipe(
        tap(user => {
          if (user) {
            this.setCurrentUser(user);
            this.startTokenRefreshInterval();
          }
        })
      )
  }

  refreshToken() {
    return this.http.post<User>(this.baseUrl + 'account/refresh-token', {},
      { withCredentials: true })
  }

  startTokenRefreshInterval() {
    setInterval(() => {
      this.http.post<User>(this.baseUrl + 'account/refresh-token', {},
        { withCredentials: true }).subscribe({
          next: user => {
            this.setCurrentUser(user)
          },
          error: () => {
            this.logout()
          }
        })
    }, 14 * 24 * 60 * 60 * 1000) // 14 days
  }

  setCurrentUser(user: User) {
    user.roles = this.getRolesFromToken(user);
    this.currentUser.set(user);
    // persist current user for other parts of the app/tests
    try {
      localStorage.setItem('user', JSON.stringify(user));
    } catch {
      // ignore storage errors in tests
    }
    this.likesService.getLikeIds();
    if (this.presenceService.hubConnection?.state !== HubConnectionState.Connected) {
      this.presenceService.createHubConnection(user)
    }
  }

  logout() {
    this.http.post(this.baseUrl + 'account/logout', {}, { withCredentials: true }).subscribe({
      next: () => {
        localStorage.removeItem('filters');
        this.likesService.clearLikeIds();
        this.currentUser.set(null);
        this.presenceService.stopHubConnection();
      }
    })

  }

  private getRolesFromToken(user: User): string[] {
    try {
      if (!user?.token) return [];
      const parts = user.token.split('.');
      if (parts.length < 2) return [];
      const payload = parts[1];
      const decoded = atob(payload);
      const jsonPayload = JSON.parse(decoded);
      const role = jsonPayload.role ?? jsonPayload.roles;
      if (!role) return [];
      return Array.isArray(role) ? role : [role];
    } catch {
      return [];
    }
  }
}