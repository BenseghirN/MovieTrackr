import { inject } from "@angular/core";
import { CanActivateFn } from "@angular/router";
import { AuthService } from "../auth/auth-service";

export const authGuard: CanActivateFn = () => {
    const authService = inject(AuthService);

    if (authService.isAuthenticated()) {
        return true;
    }

    const returnUrl = window.location.pathname;
    authService.login(returnUrl);
    return false;
};