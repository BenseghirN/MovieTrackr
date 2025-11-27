import { computed, inject, Injectable, signal } from '@angular/core';
import { Observable, tap, catchError, map, of } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthUser, MeClaims } from './auth-user.model';
import { HttpBackend, HttpClient } from '@angular/common/http';

@Injectable({ providedIn: 'root' })
export class AuthService {
    // private readonly api = inject(ApiService);
    private readonly http = new HttpClient(inject(HttpBackend));
    private readonly baseUrl = environment.apiUrl;

    private currentUserSignal = signal<AuthUser | null>(null);
    private isAuthenticatedSignal = signal<boolean>(false);

    readonly currentUser = this.currentUserSignal.asReadonly();
    readonly isAuthenticated = this.isAuthenticatedSignal.asReadonly();
    readonly isAdmin = computed(() => this.currentUserSignal()?.role === 'Admin');

    constructor() {
        this.checkAuthStatus();
    }

    login(returnUrl: string = '/'): void {
        const loginUrl = `${this.baseUrl}/api/v1/connect?returnUrl=${encodeURIComponent(returnUrl)}`;
        window.location.href = loginUrl;
    }

    logout(returnUrl: string = '/'): void {
        const logoutUrl = `${this.baseUrl}/api/v1/logout?returnUrl=${encodeURIComponent(returnUrl)}`;
        
        this.currentUserSignal.set(null);
        this.isAuthenticatedSignal.set(false);
        
        window.location.href = logoutUrl;
    }

    checkAuth(): Observable<boolean> {
        console.log("1/ CHECKAUTH");
        return this.http.get<MeClaims>(
            `${this.baseUrl}/api/v1/me`,
            { withCredentials: true }
            ).pipe(
            map(() => {
                this.isAuthenticatedSignal.set(true);
                console.log("2/ CHECKAUTH - SUCCESS", true);
                return true;
            }),
            catchError((err) => {
                console.error('3/ CHECKAUTH - ERROR', err);
                this.isAuthenticatedSignal.set(false);
                this.currentUserSignal.set(null);
                return of(false);
            })
        );
    }

    getUserInfo(): Observable<AuthUser> {
        return this.http.get<AuthUser>(
            `${this.baseUrl}/api/v1/user-info`,
            { withCredentials: true }
        ).pipe(
            tap(user => {
                this.currentUserSignal.set(user);
                this.isAuthenticatedSignal.set(true);
            }),
            catchError(error => {
                console.error('Failed to get user info:', error);
                this.currentUserSignal.set(null);
                this.isAuthenticatedSignal.set(false);
                throw error;
            })
        );
    }

    private checkAuthStatus(): void {
        this.checkAuth().subscribe();
    }
}