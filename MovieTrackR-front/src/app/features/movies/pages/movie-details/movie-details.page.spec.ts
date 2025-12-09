import { ComponentFixture, TestBed, fakeAsync, tick, flush } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, ParamMap, Router } from '@angular/router';
import { Location } from '@angular/common';
import { of, throwError, BehaviorSubject, Subject, NEVER } from 'rxjs';
import { MovieDetailsPage } from './movie-details.page';
import { MovieService } from '../../services/movie.service';
import { TmdbImageService } from '../../../../core/services/tmdb-image.service';
import { MovieDetails } from '../../models/movie-details.model';
import { MovieStreamingOffers } from '../../models/streaming-offers.model';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { DialogService } from 'primeng/dynamicdialog';

describe('MovieDetailsPage', () => {
  let component: MovieDetailsPage;
  let fixture: ComponentFixture<MovieDetailsPage>;
  let mockMovieService: jasmine.SpyObj<MovieService>;
  let mockTmdbImageService: jasmine.SpyObj<TmdbImageService>;
  let mockRouter: jasmine.SpyObj<Router>;
  let mockLocation: jasmine.SpyObj<Location>;
  let paramMapSubject: BehaviorSubject<ParamMap>;

  const mockMovieDetails: MovieDetails = {
    id: '1',
    tmdbId: 123,
    title: 'Test Movie',
    originalTitle: 'Test Movie Original',
    overview: 'A great test movie',
    posterUrl: '/poster.jpg',
    backdropPath: '/backdrop.jpg',
    releaseDate: '2024-01-15',
    duration: 120,
    voteAverage: 8.5,
    averageRating: 8.5,
    reviewCount: 50,
    genres: [
      { id: 'genre-1', name: 'Action', tmdbId: 28 },
      { id: 'genre-2', name: 'Adventure', tmdbId: 12 }
    ],
    cast: [
      {
        personId: 'person-1',
        name: 'John Doe',
        character: 'Main Character',
        profilePath: '/actor1.jpg',
        order: 0
      }
    ],
    crew: [
      {
        personId: 'director-1',
        name: 'Jane Director',
        job: 'Director',
        department: 'Directing',
        profilePath: '/director.jpg'
      },
      {
        personId: 'writer-1',
        name: 'Bob Writer',
        job: 'Writer',
        department: 'Writing',
        profilePath: '/writer.jpg'
      }
    ],
    year: 2024,
    trailerUrl: 'https://www.youtube.com/watch?v=test',
    tagline: 'Test Tagline',
    createdAt: '2024-01-01T00:00:00Z'
  };

  const mockStreamingOffers: MovieStreamingOffers = {
    country: 'BE',
    link: 'https://example.com',
    flatrate: [
      { providerId: 1, providerName: 'Provider1', logoPath: '/logo1.png' }
    ],
    free: [
      { providerId: 2, providerName: 'Provider2', logoPath: '/logo2.png' }
    ]
  };

  function createComponent(
    movieResponse: any = of(mockMovieDetails),
    streamingResponse: any = of(mockStreamingOffers),
    initialParamMap: ParamMap = convertToParamMap({ id: '123' })
  ) {
    paramMapSubject = new BehaviorSubject<ParamMap>(initialParamMap);

    mockMovieService = jasmine.createSpyObj('MovieService', [
      'getMovieByRouteId',
      'getStreamingOffers'
    ]);
    mockTmdbImageService = jasmine.createSpyObj('TmdbImageService', [
      'getPosterUrl',
      'getBackdropUrl',
      'getProfileUrl'
    ]);
    mockRouter = jasmine.createSpyObj('Router', ['navigate']);
    mockLocation = jasmine.createSpyObj('Location', ['back']);

    mockMovieService.getMovieByRouteId.and.returnValue(movieResponse);
    mockMovieService.getStreamingOffers.and.returnValue(streamingResponse);

    TestBed.configureTestingModule({
      imports: [MovieDetailsPage],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: MovieService, useValue: mockMovieService },
        { provide: TmdbImageService, useValue: mockTmdbImageService },
        { provide: Router, useValue: mockRouter },
        { provide: Location, useValue: mockLocation },
        {
          provide: ActivatedRoute,
          useValue: {
            paramMap: paramMapSubject
          }
        },
        DialogService
      ]
    });

    fixture = TestBed.createComponent(MovieDetailsPage);
    component = fixture.componentInstance;
  }

  afterEach(() => {
    TestBed.resetTestingModule();
  });

  describe('Component Initialization', () => {
    it('should create', () => {
      createComponent();
      expect(component).toBeTruthy();
    });

    it('should initialize with loading state true', fakeAsync(() => {
      const pendingSubject = new Subject();
      createComponent(pendingSubject.asObservable());
      
      expect(component.loading()).toBe(true);
      
      fixture.detectChanges();
      expect(component.loading()).toBe(true);
      
      pendingSubject.complete();
      flush();
    }));

    it('should set initial error to null', () => {
      createComponent();
      fixture.detectChanges();
      expect(component.error()).toBeNull();
    });

    it('should have initial poster flip state as false', () => {
      createComponent();
      fixture.detectChanges();
      expect(component.posterFlipped()).toBe(false);
    });

    it('should have trailer dialog initially hidden', () => {
      createComponent();
      fixture.detectChanges();
      expect(component.trailerDialogVisible()).toBe(false);
    });

    it('should have initial empty current trailer URL', () => {
      createComponent();
      fixture.detectChanges();
      expect(component.currentTrailerUrl()).toBe('');
    });
  });

  describe('Movie Loading', () => {
    it('should load movie by id from route params', fakeAsync(() => {
      createComponent();
      fixture.detectChanges();
      tick(100);

      expect(mockMovieService.getMovieByRouteId).toHaveBeenCalledWith('123');
      expect(component.movie()).toEqual(mockMovieDetails);
    }));

    it('should set error when movie id is missing', fakeAsync(() => {
      createComponent(of(mockMovieDetails), of(mockStreamingOffers), convertToParamMap({}));
      fixture.detectChanges();
      tick(100);

      expect(component.error()).toBe('ID du film manquant');
      expect(component.loading()).toBe(false);
    }));

    it('should handle movie loading error', fakeAsync(() => {
      createComponent(throwError(() => new Error('Network error')));
      fixture.detectChanges();
      tick(100);

      expect(component.error()).toBe('Impossible de charger les informations du film');
      expect(component.loading()).toBe(false);
      expect(component.movie()).toBeNull();
    }));

    it('should clear previous error when loading new movie', fakeAsync(() => {
      createComponent();
      fixture.detectChanges();
      tick(100);
      
      component.error.set('Previous error');
      paramMapSubject.next(convertToParamMap({ id: '456' }));
      tick(100);

      expect(component.error()).toBeNull();
    }));
  });

  describe('Streaming Offers Loading', () => {
    it('should load streaming offers when movie is loaded', fakeAsync(() => {
      createComponent();
      fixture.detectChanges();
      tick(100);

      expect(mockMovieService.getStreamingOffers).toHaveBeenCalledWith(123, 'BE');
      expect(component.streamingOffers()).toEqual(mockStreamingOffers);
      expect(component.streamingLoading()).toBe(false);
    }));

    it('should set streaming loading state during load', fakeAsync(() => {
      const streamingSubject = new Subject<MovieStreamingOffers>();
      createComponent(of(mockMovieDetails), streamingSubject.asObservable());
      fixture.detectChanges();
      tick(100);

      expect(component.streamingLoading()).toBe(true);

      streamingSubject.next(mockStreamingOffers);
      streamingSubject.complete();
      tick(100);
      
      expect(component.streamingLoading()).toBe(false);
    }));

    it('should handle streaming offers loading error', fakeAsync(() => {
      createComponent(of(mockMovieDetails), throwError(() => new Error('Streaming error')));
      fixture.detectChanges();
      tick(100);

      expect(component.streamingError()).toBe('Impossible de charger les offres de streaming.');
      expect(component.streamingLoading()).toBe(false);
      expect(component.streamingOffers()).toBeNull();
    }));

    it('should not load streaming offers when movie is null', fakeAsync(() => {
      createComponent(of(null as any));
      fixture.detectChanges();
      tick(100);

      expect(component.streamingOffers()).toBeNull();
    }));

    it('should not load streaming offers when movie has no tmdbId', fakeAsync(() => {
      const movieWithoutTmdbId = { ...mockMovieDetails, tmdbId: 0 };
      createComponent(of(movieWithoutTmdbId));
      fixture.detectChanges();
      tick(100);

      expect(component.streamingOffers()).toBeNull();
    }));
  });

  describe('Computed Properties', () => {
    it('should compute hasCast as true when movie has cast', fakeAsync(() => {
      createComponent();
      fixture.detectChanges();
      tick(100);

      expect(component.hasCast()).toBe(true);
    }));

    it('should compute hasCast as false when movie has no cast', fakeAsync(() => {
      const movieWithoutCast = { ...mockMovieDetails, cast: [] };
      createComponent(of(movieWithoutCast));
      fixture.detectChanges();
      tick(100);

      expect(component.hasCast()).toBe(false);
    }));

    it('should compute hasCrew as true when movie has crew', fakeAsync(() => {
      createComponent();
      fixture.detectChanges();
      tick(100);

      expect(component.hasCrew()).toBe(true);
    }));

    it('should compute hasCrew as false when movie has no crew', fakeAsync(() => {
      const movieWithoutCrew = { ...mockMovieDetails, crew: [] };
      createComponent(of(movieWithoutCrew));
      fixture.detectChanges();
      tick(100);

      expect(component.hasCrew()).toBe(false);
    }));

    it('should find director from crew', fakeAsync(() => {
      createComponent();
      fixture.detectChanges();
      tick(100);

      const director = component.director();
      expect(director).toBeTruthy();
      expect(director?.job).toBe('Director');
      expect(director?.name).toBe('Jane Director');
    }));

    it('should return null for director when not found', fakeAsync(() => {
      const movieWithoutDirector = {
        ...mockMovieDetails,
        crew: [
          {
            personId: 'writer-1',
            name: 'Bob Writer',
            job: 'Writer',
            department: 'Writing',
            profilePath: '/writer.jpg'
          }
        ]
      };
      createComponent(of(movieWithoutDirector));
      fixture.detectChanges();
      tick(100);

      expect(component.director()).toBeNull();
    }));
  });

  describe('Crew by Department', () => {
    it('should group crew by department', fakeAsync(() => {
      createComponent();
      fixture.detectChanges();
      tick(100);

      const grouped = component.crewByDepartment();
      expect(Object.keys(grouped)).toContain('Réalisation');
      expect(Object.keys(grouped)).toContain('Scénario');
    }));

    it('should exclude unknown departments', fakeAsync(() => {
      const movieWithUnknownDept = {
        ...mockMovieDetails,
        crew: [
          {
            personId: 'crew-1',
            name: 'Unknown Crew',
            job: 'Unknown Job',
            department: 'Unknown Department',
            profilePath: '/unknown.jpg'
          }
        ]
      };
      createComponent(of(movieWithUnknownDept));
      fixture.detectChanges();
      tick(100);

      const grouped = component.crewByDepartment();
      expect(Object.keys(grouped).length).toBe(0);
    }));

    it('should avoid crew duplicates in same department', fakeAsync(() => {
      const movieWithDuplicates = {
        ...mockMovieDetails,
        crew: [
          {
            personId: 'director-1',
            name: 'Jane Director',
            job: 'Director',
            department: 'Directing',
            profilePath: '/director.jpg'
          },
          {
            personId: 'director-1',
            name: 'Jane Director',
            job: 'Executive Producer',
            department: 'Directing',
            profilePath: '/director.jpg'
          }
        ]
      };
      createComponent(of(movieWithDuplicates));
      fixture.detectChanges();
      tick(100);

      const grouped = component.crewByDepartment();
      expect(grouped['Réalisation']?.length).toBe(1);
    }));
  });

  describe('getDepartmentKeys', () => {
    it('should return department keys', fakeAsync(() => {
      createComponent();
      fixture.detectChanges();
      tick(100);

      const keys = component.getDepartmentKeys();
      expect(Array.isArray(keys)).toBe(true);
      expect(keys.length).toBeGreaterThan(0);
    }));

    it('should return empty array when no crew', fakeAsync(() => {
      const movieWithNoCrew = { ...mockMovieDetails, crew: [] };
      createComponent(of(movieWithNoCrew));
      fixture.detectChanges();
      tick(100);

      const keys = component.getDepartmentKeys();
      expect(keys.length).toBe(0);
    }));
  });

  describe('Carousel Configuration', () => {
    it('should have responsive carousel options', () => {
      createComponent();
      fixture.detectChanges();
      expect(component.carouselResponsiveOptions.length).toBe(5);
      expect(component.carouselResponsiveOptions[0].breakpoint).toBe('1400px');
      expect(component.carouselResponsiveOptions[0].numVisible).toBe(6);
    });
  });

  describe('Format Duration', () => {
    beforeEach(() => {
      createComponent();
      fixture.detectChanges();
    });

    it('should format duration correctly', () => {
      const result = component.formatDuration(125);
      expect(result).toBe('2h 5min');
    });

    it('should handle single digit minutes', () => {
      const result = component.formatDuration(61);
      expect(result).toBe('1h 1min');
    });

    it('should return N/A for null duration', () => {
      const result = component.formatDuration(null);
      expect(result).toBe('N/A');
    });

    it('should handle zero duration', () => {
      const result = component.formatDuration(0);
      expect(result).toBe('N/A');
    });

    it('should handle hours only', () => {
      const result = component.formatDuration(120);
      expect(result).toBe('2h 0min');
    });
  });

  describe('Format Date', () => {
    beforeEach(() => {
      createComponent();
      fixture.detectChanges();
    });

    it('should format date in French locale', () => {
      const result = component.formatDate('2024-01-15');
      expect(result).toContain('15');
      expect(result).toContain('janvier');
      expect(result).toContain('2024');
    });

    it('should return empty string for null date', () => {
      const result = component.formatDate(null);
      expect(result).toBe('');
    });

    it('should return empty string for undefined date', () => {
      const result = component.formatDate(undefined);
      expect(result).toBe('');
    });

    it('should handle empty string date', () => {
      const result = component.formatDate('');
      expect(result).toBe('');
    });
  });

  describe('Trailer Management', () => {
    beforeEach(() => {
      createComponent();
      fixture.detectChanges();
    });

    it('should open trailer and show dialog', () => {
      const youtubeUrl = 'https://www.youtube.com/watch?v=dQw4w9WgXcQ';
      component.openTrailer(youtubeUrl);

      expect(component.trailerDialogVisible()).toBe(true);
      expect(component.currentTrailerUrl()).toContain('embed');
    });

    it('should build embed URL from youtube URL', () => {
      const youtubeUrl = 'https://www.youtube.com/watch?v=dQw4w9WgXcQ';
      component.openTrailer(youtubeUrl);

      expect(component.currentTrailerUrl()).toBe(
        'https://www.youtube.com/embed/dQw4w9WgXcQ'
      );
    });

    it('should use embed URL as is if already in embed format', () => {
      const embedUrl = 'https://www.youtube.com/embed/dQw4w9WgXcQ';
      component.openTrailer(embedUrl);

      expect(component.currentTrailerUrl()).toBe(embedUrl);
    });

    it('should handle invalid URLs gracefully', () => {
      const invalidUrl = 'not a valid url';
      component.openTrailer(invalidUrl);

      expect(component.currentTrailerUrl()).toBe(invalidUrl);
    });

    it('should hide trailer dialog', () => {
      component.trailerDialogVisible.set(true);
      component.currentTrailerUrl.set('https://example.com/video');

      component.onTrailerDialogVisibleChange(false);

      expect(component.trailerDialogVisible()).toBe(false);
      expect(component.currentTrailerUrl()).toBe('');
    });

    it('should keep dialog hidden when called with false', () => {
      component.onTrailerDialogVisibleChange(false);

      expect(component.trailerDialogVisible()).toBe(false);
    });
  });

  describe('Poster Flip', () => {
    beforeEach(() => {
      createComponent();
      fixture.detectChanges();
    });

    it('should toggle poster flip state', () => {
      expect(component.posterFlipped()).toBe(false);

      component.togglePosterFlip();
      expect(component.posterFlipped()).toBe(true);

      component.togglePosterFlip();
      expect(component.posterFlipped()).toBe(false);
    });

    it('should reset poster flip when movie changes to null', fakeAsync(() => {
      tick(100);
      
      component.posterFlipped.set(true);
      expect(component.posterFlipped()).toBe(true);
      
      mockMovieService.getMovieByRouteId.and.returnValue(of(null as any));
      paramMapSubject.next(convertToParamMap({ id: '456' }));
      fixture.detectChanges();
      tick(100);
      fixture.detectChanges();

      expect(component.posterFlipped()).toBe(false);
    }));
  });

  describe('Streaming Link Opening', () => {
    beforeEach(() => {
      createComponent();
      fixture.detectChanges();
      spyOn(window, 'open');
    });

    it('should open streaming link in new tab', () => {
      const event = new MouseEvent('click');
      spyOn(event, 'stopPropagation');

      component.openStreamingLink('https://example.com', event);

      expect(event.stopPropagation).toHaveBeenCalled();
      expect(window.open).toHaveBeenCalledWith('https://example.com', '_blank');
    });

    it('should not open link when URL is null', () => {
      const event = new MouseEvent('click');
      component.openStreamingLink(null, event);

      expect(window.open).not.toHaveBeenCalled();
    });

    it('should prevent event propagation', () => {
      const event = new MouseEvent('click');
      spyOn(event, 'stopPropagation');

      component.openStreamingLink('https://example.com', event);

      expect(event.stopPropagation).toHaveBeenCalled();
    });
  });

  describe('Navigation', () => {
    beforeEach(() => {
      createComponent();
      fixture.detectChanges();
    });

    it('should navigate to person details', () => {
      component.onPersonClick('person-1');

      expect(mockRouter.navigate).toHaveBeenCalledWith(['/people', 'person-1']);
    });

    it('should not navigate when person id is empty', () => {
      component.onPersonClick('');

      expect(mockRouter.navigate).not.toHaveBeenCalled();
    });

    it('should go back in navigation history', () => {
      component.goBack();

      expect(mockLocation.back).toHaveBeenCalled();
    });
  });

  describe('Poster Flip Reset on Movie Change', () => {
    it('should reset poster flip when changing to movie without tmdbId', fakeAsync(() => {
      createComponent();
      fixture.detectChanges();
      tick(100);
      
      component.posterFlipped.set(true);
      expect(component.posterFlipped()).toBe(true);

      const movieWithoutTmdbId = { ...mockMovieDetails, tmdbId: 0 };
      mockMovieService.getMovieByRouteId.and.returnValue(of(movieWithoutTmdbId));
      paramMapSubject.next(convertToParamMap({ id: 'different-id' }));
      fixture.detectChanges();
      tick(100);
      fixture.detectChanges();

      expect(component.posterFlipped()).toBe(false);
    }));
  });

  describe('Streaming Offers Clearing', () => {
    it('should clear streaming offers when movie id is missing', fakeAsync(() => {
      createComponent();
      fixture.detectChanges();
      tick(100);
      
      expect(component.streamingOffers()).toEqual(mockStreamingOffers);
      
      mockMovieService.getMovieByRouteId.and.returnValue(of(null as any));
      paramMapSubject.next(convertToParamMap({}));
      fixture.detectChanges(); 
      tick(100);
      fixture.detectChanges(); 

      expect(component.streamingOffers()).toBeNull();
    }));

    it('should clear streaming offers when movie has no tmdbId', fakeAsync(() => {
      createComponent();
      fixture.detectChanges();
      tick(100);
      
      expect(component.streamingOffers()).toEqual(mockStreamingOffers);

      const movieWithoutTmdbId = { ...mockMovieDetails, tmdbId: 0 };
      mockMovieService.getMovieByRouteId.and.returnValue(of(movieWithoutTmdbId));
      paramMapSubject.next(convertToParamMap({ id: '456' }));
      fixture.detectChanges();
      tick(100);
      fixture.detectChanges();

      expect(component.streamingOffers()).toBeNull();
    }));
  });
});