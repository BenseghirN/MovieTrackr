import { TestBed } from '@angular/core/testing';
import { AuthService } from './auth.service';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ConfigService } from './config.service';
import { AuthUser } from '../auth/models/auth-user.model';
import { provideHttpClient } from '@angular/common/http';

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;
  let mockConfigService: jasmine.SpyObj<ConfigService>;

  const mockConfigUrl = 'https://api.example.com';

  const mockAuthUser: AuthUser = {
    id: 'user-1',
    externalId: 'external-1',
    email: 'test@example.com',
    pseudo: 'testuser',
    givenName: 'John',
    surname: 'Doe',
    avatarUrl: '/avatar.jpg',
    role: 'User'
  };

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

  beforeEach(() => {
    mockConfigService = jasmine.createSpyObj('ConfigService', [], {
      apiUrl: mockConfigUrl,
      isProduction: false
    });

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: ConfigService, useValue: mockConfigService }
      ]
    });

    httpMock = TestBed.inject(HttpTestingController);
    service = TestBed.inject(AuthService);

    const initialReq = httpMock.match(`${mockConfigUrl}/me`);
    initialReq.forEach(req => req.flush(null, { status: 401, statusText: 'Unauthorized' }));
  });

  afterEach(() => {
    httpMock.verify();
  });

  describe('Service Initialization', () => {
    it('should be created', () => {
      expect(service).toBeTruthy();
    });

    it('should initialize with null current user', () => {
      expect(service.currentUser()).toBeNull();
    });

    it('should initialize with isAuthenticated as false', () => {
      expect(service.isAuthenticated()).toBe(false);
    });

    it('should initialize with isAdmin as false', () => {
      expect(service.isAdmin()).toBe(false);
    });
  });

  describe('checkAuth', () => {
    it('should return true and set isAuthenticated on success', () => {
      service.checkAuth().subscribe(result => {
        expect(result).toBe(true);
        expect(service.isAuthenticated()).toBe(true);
      });

      const req = httpMock.expectOne(`${mockConfigUrl}/me`);
      expect(req.request.withCredentials).toBe(true);
      req.flush({ claims: [] });
    });

    it('should return false on error', () => {
      service.checkAuth().subscribe(result => {
        expect(result).toBe(false);
        expect(service.isAuthenticated()).toBe(false);
      });

      const req = httpMock.expectOne(`${mockConfigUrl}/me`);
      req.flush(null, { status: 401, statusText: 'Unauthorized' });
    });

    it('should clear currentUser on error', () => {
      service['currentUserSignal'].set(mockAuthUser);

      service.checkAuth().subscribe();

      const req = httpMock.expectOne(`${mockConfigUrl}/me`);
      req.flush(null, { status: 401, statusText: 'Unauthorized' });

      expect(service.currentUser()).toBeNull();
    });
  });

  describe('getUserInfo', () => {
    it('should return user data and set currentUser on success', () => {
      service.getUserInfo().subscribe(user => {
        expect(user).toEqual(mockAuthUser);
        expect(service.currentUser()).toEqual(mockAuthUser);
        expect(service.isAuthenticated()).toBe(true);
      });

      const req = httpMock.expectOne(`${mockConfigUrl}/user-info`);
      expect(req.request.withCredentials).toBe(true);
      req.flush(mockAuthUser);
    });

    it('should clear state and throw error on failure', () => {
      service['currentUserSignal'].set(mockAuthUser);
      spyOn(console, 'error');

      service.getUserInfo().subscribe({
        error: () => {
          expect(service.currentUser()).toBeNull();
          expect(service.isAuthenticated()).toBe(false);
          expect(console.error).toHaveBeenCalled();
        }
      });

      const req = httpMock.expectOne(`${mockConfigUrl}/user-info`);
      req.flush(null, { status: 500, statusText: 'Server Error' });
    });
  });

  describe('isAdmin computed signal', () => {
    it('should return true when user role is Admin', () => {
      service['currentUserSignal'].set(mockAdminUser);
      expect(service.isAdmin()).toBe(true);
    });

    it('should return false when user role is User', () => {
      service['currentUserSignal'].set(mockAuthUser);
      expect(service.isAdmin()).toBe(false);
    });

    it('should return false when currentUser is null', () => {
      service['currentUserSignal'].set(null);
      expect(service.isAdmin()).toBe(false);
    });

    it('should update when currentUser changes', () => {
      service['currentUserSignal'].set(mockAuthUser);
      expect(service.isAdmin()).toBe(false);

      service['currentUserSignal'].set(mockAdminUser);
      expect(service.isAdmin()).toBe(true);
    });
  });

  describe('Readonly signals', () => {
    it('should expose currentUser as readonly', () => {
      service['currentUserSignal'].set(mockAuthUser);
      expect(service.currentUser()).toEqual(mockAuthUser);
    });

    it('should expose isAuthenticated as readonly', () => {
      service['isAuthenticatedSignal'].set(true);
      expect(service.isAuthenticated()).toBe(true);
    });

    it('should not allow direct modification of currentUser signal', () => {
      expect(() => {
        (service.currentUser as any).set(mockAuthUser);
      }).toThrow();
    });
  });
});