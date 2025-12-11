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
    mockAuthService = jasmine.createSpyObj('AuthService', ['init']);
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

  afterEach(() => {
    TestBed.resetTestingModule();
  });

  it('should allow access when user is authenticated', (done) => {
    mockAuthService.init.and.returnValue(of(true));

    TestBed.runInInjectionContext(() => {
      const result = authGuard(mockRoute, mockRouterState);

      (result as any).subscribe((canActivate: boolean | UrlTree) => {
        expect(canActivate).toBe(true);
        done();
      });
    });
  });

  it('should deny access when user is not authenticated', (done) => {
    mockAuthService.init.and.returnValue(of(false));

    TestBed.runInInjectionContext(() => {
      const result = authGuard(mockRoute, mockRouterState);

      (result as any).subscribe((canActivate: boolean | UrlTree) => {
        expect(canActivate).toBe(mockUrlTree);
        expect(mockRouter.createUrlTree).toHaveBeenCalledWith(['/forbidden']);
        done();
      });
    });
  });

  it('should redirect to forbidden on init error', (done) => {
    mockAuthService.init.and.returnValue(throwError(() => new Error('Auth failed')));

    TestBed.runInInjectionContext(() => {
      const result = authGuard(mockRoute, mockRouterState);

      (result as any).subscribe((canActivate: boolean | UrlTree) => {
        expect(canActivate).toBe(mockUrlTree);
        expect(mockRouter.createUrlTree).toHaveBeenCalledWith(['/forbidden']);
        done();
      });
    });
  });

  it('should call authService.init()', (done) => {
    mockAuthService.init.and.returnValue(of(true));

    TestBed.runInInjectionContext(() => {
      const result = authGuard(mockRoute, mockRouterState);

      (result as any).subscribe(() => {
        expect(mockAuthService.init).toHaveBeenCalled();
        done();
      });
    });
  });

  it('should handle 401 error gracefully', (done) => {
    mockAuthService.init.and.returnValue(throwError(() => ({ status: 401 })));

    TestBed.runInInjectionContext(() => {
      const result = authGuard(mockRoute, mockRouterState);

      (result as any).subscribe((canActivate: boolean | UrlTree) => {
        expect(canActivate).toBe(mockUrlTree);
        expect(mockRouter.createUrlTree).toHaveBeenCalledWith(['/forbidden']);
        done();
      });
    });
  });

  it('should handle network error gracefully', (done) => {
    mockAuthService.init.and.returnValue(throwError(() => new Error('Network error')));

    TestBed.runInInjectionContext(() => {
      const result = authGuard(mockRoute, mockRouterState);

      (result as any).subscribe((canActivate: boolean | UrlTree) => {
        expect(canActivate).toBe(mockUrlTree);
        done();
      });
    });
  });
});