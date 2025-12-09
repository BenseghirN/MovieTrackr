import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ListDetailsPage } from './list-details.page';
import { UserListService } from '../../services/user-list.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { AuthService } from '../../../../core/services/auth.service';
import { ActivatedRoute, Router } from '@angular/router';
import { DialogService } from 'primeng/dynamicdialog';
import { UserListDetails, UserListType } from '../../models/user-list.model';
import { of, throwError } from 'rxjs';
import { signal } from '@angular/core';

describe('ListDetailsPage', () => {
  let component: ListDetailsPage;
  let fixture: ComponentFixture<ListDetailsPage>;
  let mockListService: jasmine.SpyObj<UserListService>;
  let mockNotificationService: jasmine.SpyObj<NotificationService>;
  let mockAuthService: jasmine.SpyObj<AuthService>;
  let mockRouter: jasmine.SpyObj<Router>;
  let mockDialogService: jasmine.SpyObj<DialogService>;
  let mockActivatedRoute: any;

  const mockListDetails: UserListDetails = {
    id: 'list-1',
    title: 'My Favorite Movies',
    description: 'A collection of my favorites',
    type: UserListType.Custom,
    isSystemList: false,
    createdAt: '2025-01-01',
    movies: [
      {
        movieId: '1',
        position: 10,
        movie: { id: '1', title: 'Movie 1', year: 2020, posterUrl: '/poster1.jpg' }
      },
      {
        movieId: '2',
        position: 20,
        movie: { id: '2', title: 'Movie 2', year: 2021, posterUrl: '/poster2.jpg' }
      }
    ]
  };

  function setupTest(
    listResponse: any = of(mockListDetails),
    paramMapValue: Map<string, string> = new Map([['id', 'list-1']])
  ) {
    mockListService = jasmine.createSpyObj('UserListService', [
      'getListDetails',
      'removeMovieFromList',
      'deleteList'
    ]);
    mockNotificationService = jasmine.createSpyObj('NotificationService', ['success', 'error']);
    mockAuthService = jasmine.createSpyObj('AuthService', [], {
      isAuthenticated: signal(true).asReadonly()
    });
    mockRouter = jasmine.createSpyObj('Router', ['navigate']);
    mockDialogService = jasmine.createSpyObj('DialogService', ['open']);

    mockActivatedRoute = {
      paramMap: of(paramMapValue)
    };

    mockListService.getListDetails.and.returnValue(listResponse);

    TestBed.configureTestingModule({
      imports: [ListDetailsPage],
      providers: [
        { provide: UserListService, useValue: mockListService },
        { provide: NotificationService, useValue: mockNotificationService },
        { provide: AuthService, useValue: mockAuthService },
        { provide: Router, useValue: mockRouter },
        { provide: DialogService, useValue: mockDialogService },
        { provide: ActivatedRoute, useValue: mockActivatedRoute }
      ]
    });

    fixture = TestBed.createComponent(ListDetailsPage);
    component = fixture.componentInstance;
  }

  afterEach(() => {
    TestBed.resetTestingModule();
  });

  describe('Component Initialization', () => {
    it('should create', () => {
      setupTest();
      expect(component).toBeTruthy();
    });

    it('should initialize signals with default values', () => {
      setupTest();
      expect(component.loading()).toBe(false);
      expect(component.error()).toBeNull();
      expect(component.reloadKey()).toBe(0);
      expect(component.listDetails()).toBeNull();
    });

    it('should load list on init', (done) => {
      setupTest();
      fixture.detectChanges();

      setTimeout(() => {
        expect(mockListService.getListDetails).toHaveBeenCalledWith('list-1');
        expect(component.listDetails()).toEqual(mockListDetails);
        done();
      }, 100);
    });

    it('should set error when list ID is missing', (done) => {
      setupTest(of(mockListDetails), new Map());
      fixture.detectChanges();

      setTimeout(() => {
        expect(component.error()).toBe('ID de liste manquant');
        done();
      }, 100);
    });
  });

  describe('loadList()', () => {
    it('should load list details on success', (done) => {
      setupTest();
      fixture.detectChanges();

      setTimeout(() => {
        expect(component.listDetails()).toEqual(mockListDetails);
        expect(component.loading()).toBe(false);
        done();
      }, 100);
    });

    it('should clear error on successful load', (done) => {
      setupTest();
      component.error.set('Previous error');
      fixture.detectChanges();

      setTimeout(() => {
        expect(component.error()).toBeNull();
        done();
      }, 100);
    });

    it('should handle load error', (done) => {
      setupTest(throwError(() => new Error('Load failed')));
      fixture.detectChanges();

      setTimeout(() => {
        expect(component.loading()).toBe(false);
        expect(mockNotificationService.error).toHaveBeenCalledWith('Impossible de charger les détails de la liste.');
        done();
      }, 100);
    });

    it('should call api with correct list id', (done) => {
      setupTest();
      fixture.detectChanges();

      setTimeout(() => {
        expect(mockListService.getListDetails).toHaveBeenCalledWith('list-1');
        done();
      }, 100);
    });
  });

  describe('hasMovies computed', () => {
    it('should return true when list has movies', (done) => {
      setupTest();
      fixture.detectChanges();

      setTimeout(() => {
        expect(component.hasMovies()).toBe(true);
        done();
      }, 100);
    });

    it('should return false when list is empty', (done) => {
      const emptyList = { ...mockListDetails, movies: [] };
      setupTest(of(emptyList));
      fixture.detectChanges();

      setTimeout(() => {
        expect(component.hasMovies()).toBe(false);
        done();
      }, 100);
    });

    it('should return false when list is null', () => {
      setupTest();
      expect(component.hasMovies()).toBe(false);
    });
  });

  describe('onRemoveMovie()', () => {
    it('should confirm before removal', (done) => {
      setupTest();
      mockListService.removeMovieFromList.and.returnValue(of(void 0));
      spyOn(window, 'confirm').and.returnValue(true);
      fixture.detectChanges();

      setTimeout(() => {
        component.onRemoveMovie('1');
        expect(window.confirm).toHaveBeenCalled();
        done();
      }, 100);
    });

    it('should not remove if user cancels', (done) => {
      setupTest();
      spyOn(window, 'confirm').and.returnValue(false);
      fixture.detectChanges();

      setTimeout(() => {
        component.onRemoveMovie('1');
        expect(mockListService.removeMovieFromList).not.toHaveBeenCalled();
        done();
      }, 100);
    });

    it('should remove movie on confirm', (done) => {
      setupTest();
      mockListService.removeMovieFromList.and.returnValue(of(void 0));
      spyOn(window, 'confirm').and.returnValue(true);
      fixture.detectChanges();

      setTimeout(() => {
        component.onRemoveMovie('1');

        setTimeout(() => {
          expect(mockListService.removeMovieFromList).toHaveBeenCalledWith('list-1', '1');
          done();
        }, 50);
      }, 100);
    });

    it('should show success notification after removal', (done) => {
      setupTest();
      mockListService.removeMovieFromList.and.returnValue(of(void 0));
      spyOn(window, 'confirm').and.returnValue(true);
      fixture.detectChanges();

      setTimeout(() => {
        component.onRemoveMovie('1');

        setTimeout(() => {
          expect(mockNotificationService.success).toHaveBeenCalledWith('Film retiré de la liste');
          done();
        }, 50);
      }, 100);
    });

    it('should handle removal error', (done) => {
      setupTest();
      mockListService.removeMovieFromList.and.returnValue(throwError(() => new Error('Remove failed')));
      spyOn(window, 'confirm').and.returnValue(true);
      fixture.detectChanges();

      setTimeout(() => {
        component.onRemoveMovie('1');

        setTimeout(() => {
          expect(mockNotificationService.error).toHaveBeenCalled();
          done();
        }, 50);
      }, 100);
    });
  });

  describe('onEditList()', () => {
    it('should open edit dialog', (done) => {
      setupTest();
      const mockDialogRef = { onClose: of(false) };
      mockDialogService.open.and.returnValue(mockDialogRef as any);
      fixture.detectChanges();

      setTimeout(() => {
        component.onEditList();
        expect(mockDialogService.open).toHaveBeenCalled();
        done();
      }, 100);
    });

    it('should reload list when dialog closes with true', (done) => {
      setupTest();
      const mockDialogRef = { onClose: of(true) };
      mockDialogService.open.and.returnValue(mockDialogRef as any);
      fixture.detectChanges();

      setTimeout(() => {
        const initialKey = component.reloadKey();
        component.onEditList();

        setTimeout(() => {
          expect(component.reloadKey()).toBeGreaterThan(initialKey);
          done();
        }, 50);
      }, 100);
    });
  });

  describe('onDeleteList()', () => {
    it('should confirm before deletion', (done) => {
      setupTest();
      mockListService.deleteList.and.returnValue(of(void 0));
      spyOn(window, 'confirm').and.returnValue(true);
      fixture.detectChanges();

      setTimeout(() => {
        component.onDeleteList();
        expect(window.confirm).toHaveBeenCalled();
        done();
      }, 100);
    });

    it('should not delete if user cancels', (done) => {
      setupTest();
      spyOn(window, 'confirm').and.returnValue(false);
      fixture.detectChanges();

      setTimeout(() => {
        component.onDeleteList();
        expect(mockListService.deleteList).not.toHaveBeenCalled();
        done();
      }, 100);
    });

    it('should navigate to my-lists after deletion', (done) => {
      setupTest();
      mockListService.deleteList.and.returnValue(of(void 0));
      spyOn(window, 'confirm').and.returnValue(true);
      fixture.detectChanges();

      setTimeout(() => {
        component.onDeleteList();

        setTimeout(() => {
          expect(mockRouter.navigate).toHaveBeenCalledWith(['/my-lists']);
          done();
        }, 50);
      }, 100);
    });
  });

  describe('onViewMovie()', () => {
    it('should navigate to movie details', (done) => {
      setupTest();
      fixture.detectChanges();

      setTimeout(() => {
        component.onViewMovie('movie-1');
        expect(mockRouter.navigate).toHaveBeenCalledWith(['/movies', 'movie-1']);
        done();
      }, 100);
    });
  });

  describe('onBack()', () => {
    it('should navigate to my-lists', (done) => {
      setupTest();
      fixture.detectChanges();

      setTimeout(() => {
        component.onBack();
        expect(mockRouter.navigate).toHaveBeenCalledWith(['/my-lists']);
        done();
      }, 100);
    });
  });

  describe('onReload()', () => {
    it('should increment reload key', (done) => {
      setupTest();
      fixture.detectChanges();

      setTimeout(() => {
        const initialKey = component.reloadKey();
        component.onReload();
        expect(component.reloadKey()).toBe(initialKey + 1);
        done();
      }, 100);
    });
  });

  describe('onReordered()', () => {
    it('should increment reload key', (done) => {
      setupTest();
      fixture.detectChanges();

      setTimeout(() => {
        const initialKey = component.reloadKey();
        component.onReordered();
        expect(component.reloadKey()).toBeGreaterThan(initialKey);
        done();
      }, 100);
    });
  });

  describe('goToMovies()', () => {
    it('should navigate to movies page', (done) => {
      setupTest();
      fixture.detectChanges();

      setTimeout(() => {
        component.goToMovies();
        expect(mockRouter.navigate).toHaveBeenCalledWith(['/movies']);
        done();
      }, 100);
    });
  });

  describe('isAuthenticated', () => {
    it('should expose auth service isAuthenticated signal', (done) => {
      setupTest();
      fixture.detectChanges();

      setTimeout(() => {
        expect(component.isAuthenticated).toBeDefined();
        expect(component.isAuthenticated()).toBe(true);
        done();
      }, 100);
    });
  });

  describe('Edge Cases', () => {
    it('should handle list with no movies', (done) => {
      const emptyList = { ...mockListDetails, movies: [] };
      setupTest(of(emptyList));
      fixture.detectChanges();

      setTimeout(() => {
        expect(component.hasMovies()).toBe(false);
        expect(component.listDetails()?.movies.length).toBe(0);
        done();
      }, 100);
    });

    it('should handle list with many movies', (done) => {
      const manyMovies = Array.from({ length: 100 }, (_, i) => ({
        movieId: `${i}`,
        position: (i + 1) * 10,
        movie: { id: `${i}`, title: `Movie ${i}`, year: 2020, posterUrl: `/poster${i}.jpg` }
      }));

      const largeList = { ...mockListDetails, movies: manyMovies };
      setupTest(of(largeList));
      fixture.detectChanges();

      setTimeout(() => {
        expect(component.hasMovies()).toBe(true);
        expect(component.listDetails()?.movies.length).toBe(100);
        done();
      }, 100);
    });

    it('should handle null list details gracefully for onRemoveMovie', () => {
      setupTest();
      expect(() => component.onRemoveMovie('1')).not.toThrow();
    });

    it('should handle null list details gracefully for onEditList', () => {
      setupTest();
      expect(() => component.onEditList()).not.toThrow();
    });

    it('should handle null list details gracefully for onDeleteList', () => {
      setupTest();
      expect(() => component.onDeleteList()).not.toThrow();
    });
  });
});