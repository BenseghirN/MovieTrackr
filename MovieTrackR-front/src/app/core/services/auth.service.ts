import { computed, inject, Injectable, signal } from '@angular/core';
import { Observable, tap, catchError, map, of, switchMap } from 'rxjs';
import { AuthUser, MeClaims } from '../auth/models/auth-user.model';
import { HttpBackend, HttpClient } from '@angular/common/http';
import { ConfigService } from './config.service';

@Injectable({ providedIn: 'root' })
export class AuthService {
    private readonly http = new HttpClient(inject(HttpBackend));
    private readonly config = inject(ConfigService);

    private currentUserSignal = signal<AuthUser | null>(null);
    private isAuthenticatedSignal = signal<boolean>(false);

    readonly currentUser = this.currentUserSignal.asReadonly();
    readonly isAuthenticated = this.isAuthenticatedSignal.asReadonly();
    readonly isAdmin = computed(() => this.currentUserSignal()?.role === 'Admin');

    // constructor() {
    //     this.checkAuthStatus();
    // }
    init(): Observable<boolean> {
        return this.checkAuth();
    }


    login(returnUrl: string = '/'): void {
        const finalReturnUrl = !this.config.isProduction
            ? window.location.origin + returnUrl
            : returnUrl

        const loginUrl = `${this.config.apiUrl}/connect?returnUrl=${encodeURIComponent(finalReturnUrl)}`;
        window.location.href = loginUrl;
    }

    logout(returnUrl: string = '/'): void {
        const finalReturnUrl = !this.config.isProduction
            ? window.location.origin + returnUrl
            : returnUrl

        const logoutUrl = `${this.config.apiUrl}/logout?returnUrl=${encodeURIComponent(finalReturnUrl)}`;
        
        this.currentUserSignal.set(null);
        this.isAuthenticatedSignal.set(false);
        
        window.location.href = logoutUrl;
    }

    checkAuth(): Observable<boolean> {
        console.log('SERVICE CheckAuth');
        return this.http.get<MeClaims>(
            `${this.config.apiUrl}/me`,
            { withCredentials: true }
            ).pipe(
            map(() => {
                this.isAuthenticatedSignal.set(true);
                console.log('SERVICE isAuthenticatedSignal', this.isAuthenticatedSignal());
                return true;
            }),
            catchError((err) => {
                this.isAuthenticatedSignal.set(false);
                this.currentUserSignal.set(null);
                console.log('SERVICE CATCHERROR', this.isAuthenticatedSignal());
                return of(false);
            })
        );
    }

    getUserInfo(): Observable<AuthUser> {
        return this.http.get<AuthUser>(
            `${this.config.apiUrl}/user-info`,
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

    // private checkAuthStatus(): void {
    //     this.checkAuth().subscribe();
    //     this.getUserInfo().subscribe();
    // }
}