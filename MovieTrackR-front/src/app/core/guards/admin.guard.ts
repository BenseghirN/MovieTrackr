import { inject } from "@angular/core";
import { CanActivateFn, Router } from "@angular/router";
import { AuthService } from "../services/auth.service";
import { map, catchError, of, switchMap } from "rxjs";

export const adminGuard: CanActivateFn = () => {
    const authService = inject(AuthService);
    const router = inject(Router);

    return authService.init().pipe(
        switchMap(isAuth => {
            if (!isAuth) return of(router.createUrlTree(['/forbidden']));

            const cached = authService.currentUser();
            if (cached)
                return of(cached.role === 'Admin'
                    ? true
                    : router.createUrlTree(['/forbidden']));

            return authService.getUserInfo().pipe(
                map(user =>
                    user.role === 'Admin'
                        ? true
                        : router.createUrlTree(['/forbidden'])
                ),
                catchError(() => of(router.createUrlTree(['/forbidden'])))
            );
        })
    );
};