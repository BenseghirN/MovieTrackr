import { HttpInterceptorFn, HttpErrorResponse } from "@angular/common/http";
import { inject } from "@angular/core";
import { Router } from "@angular/router";
import { catchError, throwError } from "rxjs";
import { AuthService } from "../services/auth.service";
import { ApiError } from "../models/api-error.model";
import { ProblemDetails } from "../models/api-error.model";
import { NotificationService } from "../services/notification.service";

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const notificationService = inject(NotificationService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      const problemDetails = error.error as ProblemDetails;
      
      const apiError: ApiError = {
        status: error.status,
        message: problemDetails?.detail || problemDetails?.title || error.message,
        errors: problemDetails?.errors,
        traceId: problemDetails?.traceId
      };

      switch (error.status) {
        case 0: // Network error
          notificationService.error('Impossible de contacter le serveur');
          break;

        case 400: // Bad Request / Validation
          console.error('Validation error:', apiError);
          break;

        case 401: // Unauthorized
          notificationService.warning('Vous devez être connecté');
          const returnUrl = window.location.pathname;
          authService.login(returnUrl);
          break;

        case 403: // Forbidden
          notificationService.error('Accès refusé');
          router.navigate(['/forbidden']);
          break;

        case 404: // Not Found
          notificationService.error('Ressource non trouvée');
          router.navigate(['/not-found']);
          break;

        case 409: // Conflict
          notificationService.warning(apiError.message);
          break;

        case 500: // Server Error
        case 502: // Server Error
        case 503: // Server Error
          notificationService.error('Erreur serveur. Veuillez réessayer plus tard.');
          break;

        default:
          notificationService.error('Une erreur est survenue');
      }

      console.error(`[API Error ${apiError.status}]`, apiError);
      
      return throwError(() => apiError);
    })
  );
};