import { inject } from "@angular/core";
import { CanActivateFn, Router } from "@angular/router";
import { AuthService } from "../services/auth.service";
import { of } from "rxjs";
import { map, catchError } from "rxjs/operators";

export const authGuard: CanActivateFn = () => {
    const authService = inject(AuthService);
    const router = inject(Router);

    return authService.checkAuth().pipe(
        map((isAuth: boolean) => (isAuth ? true : router.createUrlTree(['/forbidden']))),
        catchError(() => of(router.createUrlTree(['/forbidden'])))
    );
};