import { TestBed } from '@angular/core/testing';
import { Router, ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree } from '@angular/router';
import { adminGuard } from './admin.guard';
import { AuthService } from '../services/auth.service';
import { of, throwError } from 'rxjs';
import { signal } from '@angular/core';
import { AuthUser } from '../auth/models/auth-user.model';

describe('adminGuard', () => {
  let mockAuthService: jasmine.SpyObj<AuthService>;
  let mockRouter: jasmine.SpyObj<Router>;
  let mockUrlTree: UrlTree;
  let mockRoute: ActivatedRouteSnapshot;
  let mockRouterState: RouterStateSnapshot;

  const mockAdminUser: AuthUser = {
    id: 'admin-1',
    externalId: 'external-admin',
    email: 'admin@example.com',
    pseudo: 'admin',
    givenName: 'Admin',
    surname: 'User',
    avatarUrl: '/avatar.jpg',
    role: 'Admin'
  };

  const mockRegularUser: AuthUser = {
    id: 'user-1',
    externalId: 'external-1',
    email: 'user@example.com',
    pseudo: 'user',
    givenName: 'Regular',
    surname: 'User',
    avatarUrl: '/avatar.jpg',
    role: 'User'
  };

  function setupTest(currentUser: AuthUser | null = null) {
    mockUrlTree = {} as UrlTree;
    mockRoute = {} as ActivatedRouteSnapshot;
    mockRouterState = { url: '/admin' } as RouterStateSnapshot;

    mockAuthService = jasmine.createSpyObj('AuthService', ['init', 'getUserInfo'], {
      currentUser: signal<AuthUser | null>(currentUser).asReadonly()
    });

    mockRouter = jasmine.createSpyObj('Router', ['createUrlTree']);
    mockRouter.createUrlTree.and.returnValue(mockUrlTree);

    TestBed.configureTestingModule({
      providers: [
        { provide: AuthService, useValue: mockAuthService },
        { provide: Router, useValue: mockRouter }
      ]
    });
  }

  afterEach(() => {
    TestBed.resetTestingModule();
  });

  describe('when user is not authenticated', () => {
    it('should redirect to forbidden', (done) => {
      setupTest();
      mockAuthService.init.and.returnValue(of(false));

      TestBed.runInInjectionContext(() => {
        const result = adminGuard(mockRoute, mockRouterState);

        (result as any).subscribe((canActivate: boolean | UrlTree) => {
          expect(canActivate).toBe(mockUrlTree);
          expect(mockRouter.createUrlTree).toHaveBeenCalledWith(['/forbidden']);
          done();
        });
      });
    });
  });

  describe('when user is authenticated with cached admin user', () => {
    it('should allow access without calling getUserInfo', (done) => {
      setupTest(mockAdminUser);
      mockAuthService.init.and.returnValue(of(true));

      TestBed.runInInjectionContext(() => {
        const result = adminGuard(mockRoute, mockRouterState);

        (result as any).subscribe((canActivate: boolean | UrlTree) => {
          expect(canActivate).toBe(true);
          expect(mockAuthService.getUserInfo).not.toHaveBeenCalled();
          done();
        });
      });
    });
  });

  describe('when user is authenticated with cached non-admin user', () => {
    it('should deny access without calling getUserInfo', (done) => {
      setupTest(mockRegularUser);
      mockAuthService.init.and.returnValue(of(true));

      TestBed.runInInjectionContext(() => {
        const result = adminGuard(mockRoute, mockRouterState);

        (result as any).subscribe((canActivate: boolean | UrlTree) => {
          expect(canActivate).toBe(mockUrlTree);
          expect(mockRouter.createUrlTree).toHaveBeenCalledWith(['/forbidden']);
          expect(mockAuthService.getUserInfo).not.toHaveBeenCalled();
          done();
        });
      });
    });
  });

  describe('when user is authenticated without cached user', () => {
    it('should fetch user info and allow access for Admin', (done) => {
      setupTest(null);
      mockAuthService.init.and.returnValue(of(true));
      mockAuthService.getUserInfo.and.returnValue(of(mockAdminUser));

      TestBed.runInInjectionContext(() => {
        const result = adminGuard(mockRoute, mockRouterState);

        (result as any).subscribe((canActivate: boolean | UrlTree) => {
          expect(canActivate).toBe(true);
          expect(mockAuthService.getUserInfo).toHaveBeenCalled();
          done();
        });
      });
    });

    it('should fetch user info and deny access for non-Admin', (done) => {
      setupTest(null);
      mockAuthService.init.and.returnValue(of(true));
      mockAuthService.getUserInfo.and.returnValue(of(mockRegularUser));

      TestBed.runInInjectionContext(() => {
        const result = adminGuard(mockRoute, mockRouterState);

        (result as any).subscribe((canActivate: boolean | UrlTree) => {
          expect(canActivate).toBe(mockUrlTree);
          expect(mockRouter.createUrlTree).toHaveBeenCalledWith(['/forbidden']);
          done();
        });
      });
    });

    it('should redirect to forbidden on getUserInfo error', (done) => {
      setupTest(null);
      mockAuthService.init.and.returnValue(of(true));
      mockAuthService.getUserInfo.and.returnValue(throwError(() => new Error('Network error')));

      TestBed.runInInjectionContext(() => {
        const result = adminGuard(mockRoute, mockRouterState);

        (result as any).subscribe((canActivate: boolean | UrlTree) => {
          expect(canActivate).toBe(mockUrlTree);
          expect(mockRouter.createUrlTree).toHaveBeenCalledWith(['/forbidden']);
          done();
        });
      });
    });
  });

  describe('error handling', () => {
    it('should handle init() error gracefully', (done) => {
      setupTest();
      mockAuthService.init.and.returnValue(throwError(() => new Error('Init failed')));

      TestBed.runInInjectionContext(() => {
        const result = adminGuard(mockRoute, mockRouterState);

        (result as any).subscribe({
          next: () => done.fail('Should not emit next'),
          error: () => done() // Guard propagates error from init
        });
      });
    });
  });
});