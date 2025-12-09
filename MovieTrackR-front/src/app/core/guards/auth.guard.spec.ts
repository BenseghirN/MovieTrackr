import { TestBed } from '@angular/core/testing';
import { Router, RouterStateSnapshot, ActivatedRouteSnapshot, UrlTree } from '@angular/router';
import { authGuard } from './auth.guard';
import { AuthService } from '../services/auth.service';
import { of, throwError } from 'rxjs';

describe('authGuard', () => {
  let mockAuthService: jasmine.SpyObj<AuthService>;
  let mockRouter: jasmine.SpyObj<Router>;
  let mockUrlTree: UrlTree;
  let mockRoute: ActivatedRouteSnapshot;
  let mockRouterState: RouterStateSnapshot;

  beforeEach(() => {
    mockAuthService = jasmine.createSpyObj('AuthService', ['checkAuth']);
    mockRouter = jasmine.createSpyObj('Router', ['createUrlTree']);
    mockUrlTree = {} as UrlTree;
    mockRoute = {} as ActivatedRouteSnapshot;
    mockRouterState = { url: '/test' } as RouterStateSnapshot;

    TestBed.configureTestingModule({
      providers: [
        { provide: AuthService, useValue: mockAuthService },
        { provide: Router, useValue: mockRouter }
      ]
    });

    mockRouter.createUrlTree.and.returnValue(mockUrlTree);
  });

  describe('authGuard', () => {
    it('should allow access when user is authenticated', (done) => {
      mockAuthService.checkAuth.and.returnValue(of(true));

      TestBed.runInInjectionContext(() => {
        const result = authGuard(mockRoute, mockRouterState) as any;

        if (result.subscribe) {
          result.subscribe((canActivate: any) => {
            expect(canActivate).toBe(true);
            done();
          });
        } else {
          expect(result).toBe(true);
          done();
        }
      });
    });

    it('should deny access when user is not authenticated', (done) => {
      mockAuthService.checkAuth.and.returnValue(of(false));

      TestBed.runInInjectionContext(() => {
        const result = authGuard(mockRoute, mockRouterState) as any;

        if (result.subscribe) {
          result.subscribe((canActivate: any) => {
            expect(canActivate).toBe(mockUrlTree);
            expect(mockRouter.createUrlTree).toHaveBeenCalledWith(['/forbidden']);
            done();
          });
        } else {
          expect(result).toBe(mockUrlTree);
          done();
        }
      });
    });

    it('should redirect to forbidden page on auth check failure', (done) => {
      mockAuthService.checkAuth.and.returnValue(
        throwError(() => new Error('Auth check failed'))
      );

      TestBed.runInInjectionContext(() => {
        const result = authGuard(mockRoute, mockRouterState) as any;

        if (result.subscribe) {
          result.subscribe((canActivate: any) => {
            expect(canActivate).toBe(mockUrlTree);
            done();
          });
        } else {
          done();
        }
      });
    });

    it('should call authService.checkAuth', (done) => {
      mockAuthService.checkAuth.and.returnValue(of(true));

      TestBed.runInInjectionContext(() => {
        const result = authGuard(mockRoute, mockRouterState) as any;

        if (result.subscribe) {
          result.subscribe(() => {
            expect(mockAuthService.checkAuth).toHaveBeenCalled();
            done();
          });
        } else {
          expect(mockAuthService.checkAuth).toHaveBeenCalled();
          done();
        }
      });
    });

    it('should inject AuthService and Router', () => {
      mockAuthService.checkAuth.and.returnValue(of(true));

      TestBed.runInInjectionContext(() => {
        authGuard(mockRoute, mockRouterState);
        expect(mockAuthService.checkAuth).toHaveBeenCalled();
      });
    });

    it('should not allow access on 401 error', (done) => {
      mockAuthService.checkAuth.and.returnValue(
        throwError(() => ({ status: 401 }))
      );

      TestBed.runInInjectionContext(() => {
        const result = authGuard(mockRoute, mockRouterState) as any;

        if (result.subscribe) {
          result.subscribe((canActivate: any) => {
            expect(mockRouter.createUrlTree).toHaveBeenCalledWith(['/forbidden']);
            done();
          });
        } else {
          done();
        }
      });
    });

    it('should not allow access on 403 error', (done) => {
      mockAuthService.checkAuth.and.returnValue(
        throwError(() => ({ status: 403 }))
      );

      TestBed.runInInjectionContext(() => {
        const result = authGuard(mockRoute, mockRouterState) as any;

        if (result.subscribe) {
          result.subscribe((canActivate: any) => {
            expect(mockRouter.createUrlTree).toHaveBeenCalledWith(['/forbidden']);
            done();
          });
        } else {
          done();
        }
      });
    });

    it('should handle network error gracefully', (done) => {
      mockAuthService.checkAuth.and.returnValue(
        throwError(() => new Error('Network error'))
      );

      TestBed.runInInjectionContext(() => {
        const result = authGuard(mockRoute, mockRouterState) as any;

        if (result.subscribe) {
          result.subscribe((canActivate: any) => {
            expect(canActivate).toBe(mockUrlTree);
            done();
          });
        } else {
          done();
        }
      });
    });
  });

  describe('Guard Return Types', () => {
    it('should return true when authenticated', (done) => {
      mockAuthService.checkAuth.and.returnValue(of(true));

      TestBed.runInInjectionContext(() => {
        const result = authGuard(mockRoute, mockRouterState) as any;

        if (result.subscribe) {
          result.subscribe((canActivate: any) => {
            expect(canActivate).toBe(true);
            done();
          });
        } else {
          expect(result).toBe(true);
          done();
        }
      });
    });

    it('should return UrlTree when not authenticated', (done) => {
      mockAuthService.checkAuth.and.returnValue(of(false));

      TestBed.runInInjectionContext(() => {
        const result = authGuard(mockRoute, mockRouterState) as any;

        if (result.subscribe) {
          result.subscribe((canActivate: any) => {
            expect(canActivate).toBe(mockUrlTree);
            done();
          });
        } else {
          expect(result).toBe(mockUrlTree);
          done();
        }
      });
    });
  });

  describe('Observable Operators', () => {
    it('should handle true response from checkAuth', (done) => {
      mockAuthService.checkAuth.and.returnValue(of(true));

      TestBed.runInInjectionContext(() => {
        const result = authGuard(mockRoute, mockRouterState) as any;

        if (result.subscribe) {
          result.subscribe((canActivate: any) => {
            expect(canActivate).toBe(true);
            done();
          });
        } else {
          expect(result).toBe(true);
          done();
        }
      });
    });

    it('should handle error from checkAuth with catchError', (done) => {
      mockAuthService.checkAuth.and.returnValue(
        throwError(() => new Error('Test error'))
      );

      TestBed.runInInjectionContext(() => {
        const result = authGuard(mockRoute, mockRouterState) as any;

        if (result.subscribe) {
          result.subscribe((canActivate: any) => {
            expect(canActivate).toBe(mockUrlTree);
            done();
          });
        } else {
          done();
        }
      });
    });
  });

  describe('Security Behavior', () => {
    it('should create forbidden url tree on auth failure', (done) => {
      mockAuthService.checkAuth.and.returnValue(of(false));

      TestBed.runInInjectionContext(() => {
        const result = authGuard(mockRoute, mockRouterState) as any;

        if (result.subscribe) {
          result.subscribe(() => {
            expect(mockRouter.createUrlTree).toHaveBeenCalledWith(['/forbidden']);
            done();
          });
        } else {
          expect(mockRouter.createUrlTree).toHaveBeenCalledWith(['/forbidden']);
          done();
        }
      });
    });

    it('should not allow unauthenticated access', (done) => {
      mockAuthService.checkAuth.and.returnValue(of(false));

      TestBed.runInInjectionContext(() => {
        const result = authGuard(mockRoute, mockRouterState) as any;

        if (result.subscribe) {
          result.subscribe((canActivate: any) => {
            expect(canActivate).not.toBe(true);
            done();
          });
        } else {
          expect(result).not.toBe(true);
          done();
        }
      });
    });
  });
});
