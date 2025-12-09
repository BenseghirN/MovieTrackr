import { TestBed } from '@angular/core/testing';
import { Router, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { adminGuard } from './admin.guard';
import { AuthService } from '../services/auth.service';

describe('adminGuard', () => {
  let mockAuthService: jasmine.SpyObj<AuthService>;
  let mockRouter: jasmine.SpyObj<Router>;
  let mockRoute: ActivatedRouteSnapshot;
  let mockRouterState: RouterStateSnapshot;

  beforeEach(() => {
    mockAuthService = jasmine.createSpyObj('AuthService', ['isAdmin']);
    mockRouter = jasmine.createSpyObj('Router', ['navigate']);
    mockRoute = {} as ActivatedRouteSnapshot;
    mockRouterState = { url: '/admin' } as RouterStateSnapshot;

    TestBed.configureTestingModule({
      providers: [
        { provide: AuthService, useValue: mockAuthService },
        { provide: Router, useValue: mockRouter }
      ]
    });
  });

  it('should allow access when user is admin', () => {
    mockAuthService.isAdmin.and.returnValue(true);

    TestBed.runInInjectionContext(() => {
      const result = adminGuard(mockRoute, mockRouterState);
      expect(result).toBe(true);
    });
  });

  it('should deny access and return false when user is not admin', () => {
    mockAuthService.isAdmin.and.returnValue(false);

    TestBed.runInInjectionContext(() => {
      const result = adminGuard(mockRoute, mockRouterState);
      expect(result).toBe(false);
    });
  });

  it('should call authService.isAdmin', () => {
    mockAuthService.isAdmin.and.returnValue(true);

    TestBed.runInInjectionContext(() => {
      adminGuard(mockRoute, mockRouterState);
      expect(mockAuthService.isAdmin).toHaveBeenCalled();
    });
  });

  it('should navigate to forbidden when user is not admin', () => {
    mockAuthService.isAdmin.and.returnValue(false);

    TestBed.runInInjectionContext(() => {
      adminGuard(mockRoute, mockRouterState);
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/forbidden']);
    });
  });

  it('should not navigate when user is admin', () => {
    mockAuthService.isAdmin.and.returnValue(true);

    TestBed.runInInjectionContext(() => {
      adminGuard(mockRoute, mockRouterState);
      expect(mockRouter.navigate).not.toHaveBeenCalled();
    });
  });

  it('should handle role changes between calls', () => {
    mockAuthService.isAdmin.and.returnValue(false);

    TestBed.runInInjectionContext(() => {
      const result1 = adminGuard(mockRoute, mockRouterState);
      expect(result1).toBe(false);

      mockAuthService.isAdmin.and.returnValue(true);
      const result2 = adminGuard(mockRoute, mockRouterState);
      expect(result2).toBe(true);
    });
  });

  it('should check admin status on each navigation', () => {
    mockAuthService.isAdmin.and.returnValue(true);

    TestBed.runInInjectionContext(() => {
      adminGuard(mockRoute, mockRouterState);
      adminGuard(mockRoute, mockRouterState);
      adminGuard(mockRoute, mockRouterState);

      expect(mockAuthService.isAdmin).toHaveBeenCalledTimes(3);
    });
  });

  it('should handle null isAdmin return as non-admin', () => {
    mockAuthService.isAdmin.and.returnValue(null as any);

    TestBed.runInInjectionContext(() => {
      const result = adminGuard(mockRoute, mockRouterState);
      expect(result).toBe(false);
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/forbidden']);
    });
  });

  it('should handle undefined isAdmin return as non-admin', () => {
    mockAuthService.isAdmin.and.returnValue(undefined as any);

    TestBed.runInInjectionContext(() => {
      const result = adminGuard(mockRoute, mockRouterState);
      expect(result).toBe(false);
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/forbidden']);
    });
  });
});