import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReorderMoviesPopoverComponent } from './reorder-movies-popover.component';
import { UserListService } from '../../services/user-list.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { TmdbImageService } from '../../../../core/services/tmdb-image.service';
import { UserListMovie } from '../../models/user-list.model';
import { of, Subject, throwError } from 'rxjs';

describe('ReorderMoviesPopoverComponent', () => {
  let component: ReorderMoviesPopoverComponent;
  let fixture: ComponentFixture<ReorderMoviesPopoverComponent>;
  let mockListService: jasmine.SpyObj<UserListService>;
  let mockNotificationService: jasmine.SpyObj<NotificationService>;
  let mockImageService: jasmine.SpyObj<TmdbImageService>;

  const mockMovies: UserListMovie[] = [
    {
      movieId: '1',
      position: 10,
      movie: { id: '1', title: 'Movie 1', year: 2020, posterUrl: '/poster1.jpg' }
    },
    {
      movieId: '2',
      position: 20,
      movie: { id: '2', title: 'Movie 2', year: 2021, posterUrl: '/poster2.jpg' }
    },
    {
      movieId: '3',
      position: 30,
      movie: { id: '3', title: 'Movie 3', year: 2022, posterUrl: '/poster3.jpg' }
    }
  ];

  function setupTest(movies: UserListMovie[] = mockMovies, listId: string = 'list-1') {
    mockListService = jasmine.createSpyObj('UserListService', ['reorderMovieInList']);
    mockNotificationService = jasmine.createSpyObj('NotificationService', ['success', 'error']);
    mockImageService = jasmine.createSpyObj('TmdbImageService', ['getImageUrl', 'getPosterUrl']);
    mockImageService.getPosterUrl.and.returnValue('/mock-poster.jpg');
    // mockImageService.getImageUrl.and.returnValue('/mock-image.jpg');

    TestBed.configureTestingModule({
      imports: [ReorderMoviesPopoverComponent],
      providers: [
        { provide: UserListService, useValue: mockListService },
        { provide: NotificationService, useValue: mockNotificationService },
        { provide: TmdbImageService, useValue: mockImageService }
      ]
    });

    fixture = TestBed.createComponent(ReorderMoviesPopoverComponent);
    component = fixture.componentInstance;

    fixture.componentRef.setInput('listId', listId);
    fixture.componentRef.setInput('movies', movies);
  }

  afterEach(() => {
    TestBed.resetTestingModule();
  });

  describe('Component Initialization', () => {
    it('should create', () => {
      setupTest();
      expect(component).toBeTruthy();
    });

    it('should have required inputs', () => {
      setupTest();
      expect(component.listId).toBeDefined();
      expect(component.movies).toBeDefined();
    });

    it('should have reordered output', () => {
      setupTest();
      expect(component.reordered).toBeDefined();
    });

    it('should initialize signals correctly', () => {
      setupTest();
      expect(component.localMovies()).toEqual([]);
      expect(component.saving()).toBe(false);
    });

    it('should inject services', () => {
      setupTest();
      expect(component['listService']).toBeTruthy();
      expect(component['notificationService']).toBeTruthy();
      expect(component.imageService).toBeTruthy();
    });
  });

  describe('toggle()', () => {
    it('should copy movies to localMovies signal', () => {
      setupTest();
      fixture.detectChanges();

      const mockPopover = { toggle: jasmine.createSpy('toggle') } as any;
      const event = new Event('click');

      component.toggle(event, mockPopover);

      expect(component.localMovies()).toEqual(mockMovies);
    });

    it('should call popover.toggle with event', () => {
      setupTest();
      fixture.detectChanges();

      const mockPopover = { toggle: jasmine.createSpy('toggle') } as any;
      const event = new Event('click');

      component.toggle(event, mockPopover);

      expect(mockPopover.toggle).toHaveBeenCalledWith(event);
    });

    it('should create a copy not reference', () => {
      setupTest();
      fixture.detectChanges();

      const mockPopover = { toggle: jasmine.createSpy('toggle') } as any;
      const event = new Event('click');

      component.toggle(event, mockPopover);

      expect(component.localMovies()).not.toBe(mockMovies);
    });

    it('should handle empty movies', () => {
      setupTest([]);
      fixture.detectChanges();

      const mockPopover = { toggle: jasmine.createSpy('toggle') } as any;
      const event = new Event('click');

      expect(() => component.toggle(event, mockPopover)).not.toThrow();
    });
  });

  describe('onReorder()', () => {
    it('should create a copy of localMovies', () => {
      setupTest();
      component.localMovies.set(mockMovies);
      const original = component.localMovies();

      component.onReorder({});

      expect(component.localMovies()).not.toBe(original);
    });

    it('should preserve movie order', () => {
      setupTest();
      component.localMovies.set(mockMovies);

      component.onReorder({});

      expect(component.localMovies().length).toBe(mockMovies.length);
      expect(component.localMovies()[0].movieId).toBe('1');
      expect(component.localMovies()[1].movieId).toBe('2');
      expect(component.localMovies()[2].movieId).toBe('3');
    });

    it('should work with reordered array', () => {
      setupTest();
      const reordered = [mockMovies[2], mockMovies[0], mockMovies[1]];
      component.localMovies.set(reordered);

      component.onReorder({});

      expect(component.localMovies()[0].movieId).toBe('3');
      expect(component.localMovies()[1].movieId).toBe('1');
      expect(component.localMovies()[2].movieId).toBe('2');
    });
  });

  describe('hasChanges computed', () => {
    it('should return false when no changes', () => {
      setupTest();
      // Set localMovies with same content as input movies
      component.localMovies.set([...mockMovies]);
      // No detectChanges() to avoid template rendering issues

      expect(component.hasChanges()).toBe(false);
    });

    it('should return true when order changed', () => {
      setupTest();
      const reordered = [mockMovies[1], mockMovies[0], mockMovies[2]];
      component.localMovies.set(reordered);

      expect(component.hasChanges()).toBe(true);
    });

    it('should return true when movie removed', () => {
      setupTest();
      const removed = [mockMovies[0], mockMovies[2]];
      component.localMovies.set(removed);

      expect(component.hasChanges()).toBe(true);
    });

    it('should return true when movie added', () => {
      setupTest();
      const added = [...mockMovies, mockMovies[0]];
      component.localMovies.set(added);

      expect(component.hasChanges()).toBe(true);
    });

    it('should handle empty original movies', () => {
      setupTest([]);
      component.localMovies.set(mockMovies);

      expect(component.hasChanges()).toBe(true);
    });

    it('should handle empty local movies', () => {
      setupTest();
      component.localMovies.set([]);

      expect(component.hasChanges()).toBe(true);
    });
  });

  describe('onSave()', () => {
    it('should hide popover when no changes', () => {
      setupTest();
      component.localMovies.set([...mockMovies]);

      const mockPopover = { hide: jasmine.createSpy('hide') } as any;

      component.onSave(mockPopover);

      expect(mockPopover.hide).toHaveBeenCalled();
      expect(mockListService.reorderMovieInList).not.toHaveBeenCalled();
    });

    it('should start saving when changes exist', () => {
      setupTest();
      const reordered = [mockMovies[1], mockMovies[0], mockMovies[2]];
      component.localMovies.set(reordered);
      
      const subject = new Subject<void>();
      mockListService.reorderMovieInList.and.returnValue(subject.asObservable());
      
      const mockPopover = { hide: jasmine.createSpy('hide') } as any;
      
      component.onSave(mockPopover);
      
      expect(component.saving()).toBeTrue();
      
      subject.next();
      subject.complete();
    });

    it('should create correct position updates', (done) => {
      setupTest();
      const reordered = [mockMovies[1], mockMovies[0], mockMovies[2]];
      component.localMovies.set(reordered);
      mockListService.reorderMovieInList.and.returnValue(of(void 0));

      const mockPopover = { hide: jasmine.createSpy('hide') } as any;

      component.onSave(mockPopover);

      setTimeout(() => {
        expect(mockListService.reorderMovieInList).toHaveBeenCalledWith('list-1', {movieId: '2', newPosition: 10});
        done();
      }, 100);
    });

    it('should call reorderMovieInList for each movie', (done) => {
      setupTest();
      const reordered = [mockMovies[1], mockMovies[0], mockMovies[2]];
      component.localMovies.set(reordered);
      mockListService.reorderMovieInList.and.returnValue(of(void 0));

      const mockPopover = { hide: jasmine.createSpy('hide') } as any;

      component.onSave(mockPopover);

      setTimeout(() => {
        expect(mockListService.reorderMovieInList).toHaveBeenCalledTimes(3);
        done();
      }, 150);
    });
  });

  describe('updatePosition() private method', () => {
    it('should show success notification on completion', (done) => {
      setupTest();
      const reordered = [mockMovies[1], mockMovies[0], mockMovies[2]];
      component.localMovies.set(reordered);
      mockListService.reorderMovieInList.and.returnValue(of(void 0));

      const mockPopover = { hide: jasmine.createSpy('hide') } as any;

      component.onSave(mockPopover);

      setTimeout(() => {
        expect(mockNotificationService.success).toHaveBeenCalledWith('Ordre mis à jour.');
        expect(component.saving()).toBe(false);
        done();
      }, 150);
    });

    it('should emit reordered output on success', (done) => {
      setupTest();
      const reordered = [mockMovies[1], mockMovies[0], mockMovies[2]];
      component.localMovies.set(reordered);
      mockListService.reorderMovieInList.and.returnValue(of(void 0));
      spyOn(component.reordered, 'emit');

      const mockPopover = { hide: jasmine.createSpy('hide') } as any;

      component.onSave(mockPopover);

      setTimeout(() => {
        expect(component.reordered.emit).toHaveBeenCalled();
        done();
      }, 150);
    });

    it('should show error notification on api error', (done) => {
      setupTest();
      const reordered = [mockMovies[1], mockMovies[0], mockMovies[2]];
      component.localMovies.set(reordered);
      mockListService.reorderMovieInList.and.returnValue(
        throwError(() => new Error('API error'))
      );

      const mockPopover = { hide: jasmine.createSpy('hide') } as any;

      component.onSave(mockPopover);

      setTimeout(() => {
        expect(mockNotificationService.error).toHaveBeenCalledWith('Erreur lors de la mise à jour');
        expect(component.saving()).toBe(false);
        done();
      }, 100);
    });

    it('should stop on first error', (done) => {
      setupTest();
      const reordered = [mockMovies[1], mockMovies[0], mockMovies[2]];
      component.localMovies.set(reordered);
      mockListService.reorderMovieInList.and.returnValue(
        throwError(() => new Error('First error'))
      );

      const mockPopover = { hide: jasmine.createSpy('hide') } as any;

      component.onSave(mockPopover);

      setTimeout(() => {
        expect(mockListService.reorderMovieInList).toHaveBeenCalledTimes(1);
        done();
      }, 100);
    });

    it('should continue on success for multiple updates', (done) => {
      setupTest();
      const reordered = [mockMovies[1], mockMovies[0], mockMovies[2]];
      component.localMovies.set(reordered);
      mockListService.reorderMovieInList.and.returnValue(of(void 0));

      const mockPopover = { hide: jasmine.createSpy('hide') } as any;

      component.onSave(mockPopover);

      setTimeout(() => {
        expect(mockListService.reorderMovieInList).toHaveBeenCalledTimes(3);
        done();
      }, 150);
    });

    it('should stop recursion when all updates processed', (done) => {
      setupTest();
      const reordered = [mockMovies[1], mockMovies[0], mockMovies[2]];
      component.localMovies.set(reordered);
      mockListService.reorderMovieInList.and.returnValue(of(void 0));

      const mockPopover = { hide: jasmine.createSpy('hide') } as any;

      component.onSave(mockPopover);

      setTimeout(() => {
        expect(mockListService.reorderMovieInList).toHaveBeenCalledTimes(3);
        expect(mockNotificationService.success).toHaveBeenCalledTimes(1);
        done();
      }, 200);
    });
  });

  describe('onCancel()', () => {
    it('should hide popover', () => {
      setupTest();

      const mockPopover = { hide: jasmine.createSpy('hide') } as any;

      component.onCancel(mockPopover);

      expect(mockPopover.hide).toHaveBeenCalled();
    });
  });

  describe('Integration Tests', () => {
    it('should handle complete reorder flow', (done) => {
      setupTest();
      fixture.detectChanges();

      const mockPopover = { toggle: jasmine.createSpy('toggle'), hide: jasmine.createSpy('hide') } as any;
      const event = new Event('click');

      component.toggle(event, mockPopover);
      expect(component.localMovies()).toEqual(mockMovies);

      const reordered = [mockMovies[1], mockMovies[0], mockMovies[2]];
      component.localMovies.set(reordered);
      expect(component.hasChanges()).toBe(true);

      mockListService.reorderMovieInList.and.returnValue(of(void 0));
      component.onSave(mockPopover);

      setTimeout(() => {
        expect(mockNotificationService.success).toHaveBeenCalled();
        expect(component.saving()).toBe(false);
        done();
      }, 150);
    });

    it('should handle cancel after reorder', () => {
      setupTest();
      fixture.detectChanges();

      const mockPopover = { toggle: jasmine.createSpy('toggle'), hide: jasmine.createSpy('hide') } as any;

      component.toggle(new Event('click'), mockPopover);
      const reordered = [mockMovies[1], mockMovies[0], mockMovies[2]];
      component.localMovies.set(reordered);
      expect(component.hasChanges()).toBe(true);

      component.onCancel(mockPopover);

      expect(mockPopover.hide).toHaveBeenCalled();
    });
  });

  describe('Edge Cases', () => {
    it('should handle single movie', () => {
      const singleMovie = [mockMovies[0]];
      setupTest(singleMovie);
      component.localMovies.set(singleMovie);

      expect(component.hasChanges()).toBe(false);
    });

    it('should handle empty movies list', () => {
      setupTest([]);
      component.localMovies.set([]);

      expect(component.hasChanges()).toBe(false);
    });

    it('should handle large movies list', (done) => {
      const largeList = Array.from({ length: 100 }, (_, i) => ({
        movieId: `${i}`,
        position: (i + 1) * 10,
        movie: { id: `${i}`, title: `Movie ${i}`, year: 2020, posterUrl: `/poster${i}.jpg` }
      }));

      setupTest(largeList);
      const reordered = [...largeList].reverse();
      component.localMovies.set(reordered);
      mockListService.reorderMovieInList.and.returnValue(of(void 0));

      const mockPopover = { hide: jasmine.createSpy('hide') } as any;

      component.onSave(mockPopover);

      setTimeout(() => {
        expect(mockListService.reorderMovieInList).toHaveBeenCalledTimes(100);
        done();
      }, 500);
    });

    it('should handle consecutive saves', (done) => {
      setupTest();
      mockListService.reorderMovieInList.and.returnValue(of(void 0));

      const mockPopover = { hide: jasmine.createSpy('hide') } as any;

      const reordered1 = [mockMovies[1], mockMovies[0], mockMovies[2]];
      component.localMovies.set(reordered1);
      component.onSave(mockPopover);

      setTimeout(() => {
        const reordered2 = [mockMovies[2], mockMovies[1], mockMovies[0]];
        component.localMovies.set(reordered2);
        component.onSave(mockPopover);

        setTimeout(() => {
          expect(mockListService.reorderMovieInList).toHaveBeenCalledTimes(6);
          done();
        }, 150);
      }, 150);
    });
  });

  describe('Input/Output Bindings', () => {
    it('should accept listId input', () => {
      setupTest(mockMovies, 'test-list-id');

      expect(component.listId()).toBe('test-list-id');
    });

    it('should accept movies input', () => {
      setupTest();

      expect(component.movies()).toEqual(mockMovies);
    });

    it('should emit reordered output', () => {
      setupTest();
      spyOn(component.reordered, 'emit');

      component.reordered.emit();

      expect(component.reordered.emit).toHaveBeenCalled();
    });
  });

  describe('Service Integration', () => {
    it('should use UserListService', () => {
      setupTest();
      expect(component['listService']).toBe(mockListService);
    });

    it('should use NotificationService', () => {
      setupTest();
      expect(component['notificationService']).toBe(mockNotificationService);
    });

    it('should use TmdbImageService', () => {
      setupTest();
      expect(component.imageService).toBe(mockImageService);
    });
  });
});