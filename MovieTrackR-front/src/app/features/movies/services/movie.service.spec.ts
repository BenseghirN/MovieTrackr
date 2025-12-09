import { TestBed } from '@angular/core/testing';
import { MovieService } from './movie.service';
import { ApiService } from '../../../core/services/api.service';
import { ConfigService } from '../../../core/services/config.service';
import { of, throwError } from 'rxjs';
import { SearchMovieParams, SearchMovieResponse, SearchMovieResult } from '../models/movie.model';
import { MovieDetails } from '../models/movie-details.model';
import { MovieStreamingOffers } from '../models/streaming-offers.model';

describe('MovieService', () => {
  let service: MovieService;
  let mockApiService: jasmine.SpyObj<ApiService>;
  let mockConfigService: jasmine.SpyObj<ConfigService>;

  const mockConfigUrl = 'https://api.example.com';

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
      }
    ],
    year: 2024,
    trailerUrl: 'https://www.youtube.com/watch?v=test',
    tagline: 'Test Tagline',
    createdAt: '2024-01-01T00:00:00Z'
  };

  const mockSearchResult: SearchMovieResult = {
    localId: '1',
    tmdbId: 123,
    title: 'Test Movie',
    year: 2024,
    originalTitle: 'Test Movie Original',
    posterPath: '/poster.jpg',
    isLocal: true,
    voteAverage: 8.5,
    popularity: 150.5,
    overview: 'A test movie overview'
  };

  const mockSearchResponse: SearchMovieResponse = {
    items: [mockSearchResult],
    meta: {
      page: 1,
      pageSize: 20,
      totalLocal: 100,
      totalTmdb: 200,
      totalResults: 300,
      totalTmdbPages: 10,
      hasMore: true
    }
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

  beforeEach(() => {
    mockApiService = jasmine.createSpyObj('ApiService', ['get']);
    mockConfigService = jasmine.createSpyObj('ConfigService', [], {
      apiUrl: mockConfigUrl
    });

    TestBed.configureTestingModule({
      providers: [
        MovieService,
        { provide: ApiService, useValue: mockApiService },
        { provide: ConfigService, useValue: mockConfigService }
      ]
    });

    service = TestBed.inject(MovieService);
  });

  describe('Service Initialization', () => {
    it('should be created', () => {
      expect(service).toBeTruthy();
    });
  });

  describe('search', () => {
    it('should call api.get with correct search endpoint and params', () => {
      mockApiService.get.and.returnValue(of(mockSearchResponse));

      const params: SearchMovieParams = {
        query: 'Test',
        page: 1,
        pageSize: 20
      };

      service.search(params).subscribe();

      expect(mockApiService.get).toHaveBeenCalledWith(
        `${mockConfigUrl}/movies/search`,
        {
          params: { Query: 'Test', Page: 1, PageSize: 20 },
          withCredentials: false
        }
      );
    });

    it('should use default page (1) when not provided', () => {
      mockApiService.get.and.returnValue(of(mockSearchResponse));

      const params: SearchMovieParams = {
        query: 'Test',
        pageSize: 20
      };

      service.search(params).subscribe();

      expect(mockApiService.get).toHaveBeenCalledWith(
        `${mockConfigUrl}/movies/search`,
        jasmine.objectContaining({
          params: jasmine.objectContaining({ Page: 1 })
        })
      );
    });

    it('should use default pageSize (20) when not provided', () => {
      mockApiService.get.and.returnValue(of(mockSearchResponse));

      const params: SearchMovieParams = {
        query: 'Test',
        page: 1
      };

      service.search(params).subscribe();

      expect(mockApiService.get).toHaveBeenCalledWith(
        `${mockConfigUrl}/movies/search`,
        jasmine.objectContaining({
          params: jasmine.objectContaining({ PageSize: 20 })
        })
      );
    });

    it('should not include Query param when query is not provided', () => {
      mockApiService.get.and.returnValue(of(mockSearchResponse));

      const params: SearchMovieParams = {
        page: 1,
        pageSize: 20
      };

      service.search(params).subscribe();

      const callArgs = mockApiService.get.calls.mostRecent().args[1] as any;
      expect(callArgs.params['Query']).toBeUndefined();
    });

    it('should not include Year param when year is not provided', () => {
      mockApiService.get.and.returnValue(of(mockSearchResponse));

      const params: SearchMovieParams = {
        query: 'Test',
        page: 1,
        pageSize: 20
      };

      service.search(params).subscribe();

      const callArgs = mockApiService.get.calls.mostRecent().args[1] as any;
      expect(callArgs.params['Year']).toBeUndefined();
    });

    it('should include Year param when provided', () => {
      mockApiService.get.and.returnValue(of(mockSearchResponse));

      const params: SearchMovieParams = {
        query: 'Test',
        year: 2024,
        page: 1,
        pageSize: 20
      };

      service.search(params).subscribe();

      expect(mockApiService.get).toHaveBeenCalledWith(
        `${mockConfigUrl}/movies/search`,
        jasmine.objectContaining({
          params: jasmine.objectContaining({ Year: 2024 })
        })
      );
    });

    it('should return search response', (done) => {
      mockApiService.get.and.returnValue(of(mockSearchResponse));

      const params: SearchMovieParams = {
        query: 'Test',
        page: 1,
        pageSize: 20
      };

      service.search(params).subscribe(result => {
        expect(result).toEqual(mockSearchResponse);
        done();
      });
    });
  });

  describe('getMovieByRouteId', () => {
    it('should return error when rawId is empty', (done) => {
      service.getMovieByRouteId('').subscribe({
        error: (err) => {
          expect(err.message).toBe('ID du film manquant');
          done();
        }
      });
    });

    it('should call getLocalMovie when rawId is a GUID (contains dashes)', () => {
      mockApiService.get.and.returnValue(of(mockMovieDetails));

      const localId = '550e8400-e29b-41d4-a716-446655440000';
      service.getMovieByRouteId(localId).subscribe();

      expect(mockApiService.get).toHaveBeenCalledWith(
        `${mockConfigUrl}/movies/${localId}`,
        { withCredentials: false }
      );
    });

    it('should call getTmdbMovie when rawId is numeric (no dashes)', () => {
      mockApiService.get.and.returnValue(of(mockMovieDetails));

      service.getMovieByRouteId('123').subscribe();

      expect(mockApiService.get).toHaveBeenCalledWith(
        `${mockConfigUrl}/movies/tmdb/123`,
        { withCredentials: false }
      );
    });

    it('should return movie details from local API', (done) => {
      mockApiService.get.and.returnValue(of(mockMovieDetails));

      const localId = '550e8400-e29b-41d4-a716-446655440000';
      service.getMovieByRouteId(localId).subscribe(result => {
        expect(result).toEqual(mockMovieDetails);
        done();
      });
    });

    it('should return movie details from TMDB API', (done) => {
      mockApiService.get.and.returnValue(of(mockMovieDetails));

      service.getMovieByRouteId('123').subscribe(result => {
        expect(result).toEqual(mockMovieDetails);
        done();
      });
    });

    it('should handle API error', (done) => {
      const error = new Error('Network error');
      mockApiService.get.and.returnValue(throwError(() => error));

      service.getMovieByRouteId('123').subscribe({
        error: (err) => {
          expect(err).toEqual(error);
          done();
        }
      });
    });
  });

  describe('getMovieDetails', () => {
    it('should get local movie when result is local with localId', () => {
      mockApiService.get.and.returnValue(of(mockMovieDetails));

      const localResult: SearchMovieResult = {
        ...mockSearchResult,
        isLocal: true,
        localId: 'local-123'
      };

      service.getMovieDetails(localResult).subscribe();

      expect(mockApiService.get).toHaveBeenCalledWith(
        `${mockConfigUrl}/movies/local-123`,
        { withCredentials: false }
      );
    });

    it('should get TMDB movie when result has tmdbId', () => {
      mockApiService.get.and.returnValue(of(mockMovieDetails));

      const tmdbResult: SearchMovieResult = {
        ...mockSearchResult,
        isLocal: false,
        tmdbId: 123
      };

      service.getMovieDetails(tmdbResult).subscribe();

      expect(mockApiService.get).toHaveBeenCalledWith(
        `${mockConfigUrl}/movies/tmdb/123`,
        { withCredentials: false }
      );
    });

    it('should throw error when result has no valid IDs', () => {
      const invalidResult: SearchMovieResult = {
        localId: null,
        tmdbId: 0,
        title: 'Invalid',
        year: null,
        originalTitle: null,
        posterPath: null,
        isLocal: false,
        voteAverage: 0,
        popularity: 0,
        overview: null
      };

      expect(() => {
        service.getMovieDetails(invalidResult);
      }).toThrowError('Film sans ID valide');
    });

    it('should return movie details', (done) => {
      mockApiService.get.and.returnValue(of(mockMovieDetails));

      const localResult: SearchMovieResult = {
        ...mockSearchResult,
        isLocal: true,
        localId: 'local-123'
      };

      service.getMovieDetails(localResult).subscribe(result => {
        expect(result).toEqual(mockMovieDetails);
        done();
      });
    });

    it('should handle API error in getMovieDetails', (done) => {
      const error = new Error('API error');
      mockApiService.get.and.returnValue(throwError(() => error));

      const localResult: SearchMovieResult = {
        ...mockSearchResult,
        isLocal: true,
        localId: 'local-123'
      };

      service.getMovieDetails(localResult).subscribe({
        error: (err) => {
          expect(err).toEqual(error);
          done();
        }
      });
    });
  });

  describe('getStreamingOffers', () => {
    it('should return error when tmdbId is missing', (done) => {
      service.getStreamingOffers(0, 'BE').subscribe({
        error: (err) => {
          expect(err.message).toBe('ID du film manquant');
          done();
        }
      });
    });

    it('should call api.get with correct streaming endpoint', () => {
      mockApiService.get.and.returnValue(of(mockStreamingOffers));

      service.getStreamingOffers(123, 'BE').subscribe();

      expect(mockApiService.get).toHaveBeenCalledWith(
        `${mockConfigUrl}/movies/123/streaming`,
        { params: { country: 'BE' }, withCredentials: false }
      );
    });

    it('should accept different country codes', () => {
      mockApiService.get.and.returnValue(of(mockStreamingOffers));

      service.getStreamingOffers(123, 'US').subscribe();

      expect(mockApiService.get).toHaveBeenCalledWith(
        `${mockConfigUrl}/movies/123/streaming`,
        { params: { country: 'US' }, withCredentials: false }
      );
    });

    it('should return streaming offers', (done) => {
      mockApiService.get.and.returnValue(of(mockStreamingOffers));

      service.getStreamingOffers(123, 'BE').subscribe(result => {
        expect(result).toEqual(mockStreamingOffers);
        done();
      });
    });

    it('should handle API error', (done) => {
      const error = new Error('Streaming error');
      mockApiService.get.and.returnValue(throwError(() => error));

      service.getStreamingOffers(123, 'BE').subscribe({
        error: (err) => {
          expect(err).toEqual(error);
          done();
        }
      });
    });
  });

  describe('getPopularMovies', () => {
    it('should call api.get with correct popular endpoint', () => {
      mockApiService.get.and.returnValue(of(mockSearchResponse));

      const params: SearchMovieParams = {
        page: 1,
        pageSize: 20
      };

      service.getPopularMovies(params).subscribe();

      expect(mockApiService.get).toHaveBeenCalledWith(
        `${mockConfigUrl}/movies/popular`,
        { params: { Page: 1, PageSize: 20 }, withCredentials: false }
      );
    });

    it('should use default page when not provided', () => {
      mockApiService.get.and.returnValue(of(mockSearchResponse));

      const params: SearchMovieParams = {
        pageSize: 20
      };

      service.getPopularMovies(params).subscribe();

      expect(mockApiService.get).toHaveBeenCalledWith(
        `${mockConfigUrl}/movies/popular`,
        jasmine.objectContaining({
          params: jasmine.objectContaining({ Page: 1 })
        })
      );
    });

    it('should use default pageSize when not provided', () => {
      mockApiService.get.and.returnValue(of(mockSearchResponse));

      const params: SearchMovieParams = {
        page: 1
      };

      service.getPopularMovies(params).subscribe();

      expect(mockApiService.get).toHaveBeenCalledWith(
        `${mockConfigUrl}/movies/popular`,
        jasmine.objectContaining({
          params: jasmine.objectContaining({ PageSize: 20 })
        })
      );
    });

    it('should return search response', (done) => {
      mockApiService.get.and.returnValue(of(mockSearchResponse));

      const params: SearchMovieParams = {
        page: 2,
        pageSize: 10
      };

      service.getPopularMovies(params).subscribe(result => {
        expect(result).toEqual(mockSearchResponse);
        done();
      });
    });

    it('should handle API error', (done) => {
      const error = new Error('Popular movies error');
      mockApiService.get.and.returnValue(throwError(() => error));

      service.getPopularMovies({ page: 1, pageSize: 20 }).subscribe({
        error: (err) => {
          expect(err).toEqual(error);
          done();
        }
      });
    });
  });

  describe('getTrendingMovies', () => {
    it('should call api.get with correct trending endpoint', () => {
      mockApiService.get.and.returnValue(of([mockSearchResult]));

      service.getTrendingMovies().subscribe();

      expect(mockApiService.get).toHaveBeenCalledWith(
        `${mockConfigUrl}/movies/trending`,
        { withCredentials: false }
      );
    });

    it('should return array of search results', (done) => {
      const trendingResults = [mockSearchResult, mockSearchResult];
      mockApiService.get.and.returnValue(of(trendingResults));

      service.getTrendingMovies().subscribe(result => {
        expect(result).toEqual(trendingResults);
        expect(Array.isArray(result)).toBe(true);
        done();
      });
    });

    it('should return empty array when no trending movies', (done) => {
      mockApiService.get.and.returnValue(of([]));

      service.getTrendingMovies().subscribe(result => {
        expect(result).toEqual([]);
        expect(result.length).toBe(0);
        done();
      });
    });

    it('should handle API error', (done) => {
      const error = new Error('Trending movies error');
      mockApiService.get.and.returnValue(throwError(() => error));

      service.getTrendingMovies().subscribe({
        error: (err) => {
          expect(err).toEqual(error);
          done();
        }
      });
    });

    it('should disable credentials in request', () => {
      mockApiService.get.and.returnValue(of([]));

      service.getTrendingMovies().subscribe();

      expect(mockApiService.get).toHaveBeenCalledWith(
        `${mockConfigUrl}/movies/trending`,
        { withCredentials: false }
      );
    });
  });

  describe('API URL Construction', () => {
    it('should use config.apiUrl from ConfigService', () => {
      mockApiService.get.and.returnValue(of(mockSearchResponse));

      service.search({ page: 1, pageSize: 20 }).subscribe();

      const callArgs = mockApiService.get.calls.mostRecent().args;
      expect(callArgs[0]).toContain(mockConfigUrl);
    });

    it('should construct correct URL for search endpoint', () => {
      mockApiService.get.and.returnValue(of(mockSearchResponse));

      service.search({ query: 'test', page: 1, pageSize: 20 }).subscribe();

      expect(mockApiService.get).toHaveBeenCalledWith(
        'https://api.example.com/movies/search',
        jasmine.any(Object)
      );
    });

    it('should construct correct URL for popular movies endpoint', () => {
      mockApiService.get.and.returnValue(of(mockSearchResponse));

      service.getPopularMovies({ page: 1, pageSize: 20 }).subscribe();

      expect(mockApiService.get).toHaveBeenCalledWith(
        'https://api.example.com/movies/popular',
        jasmine.any(Object)
      );
    });

    it('should construct correct URL for trending movies endpoint', () => {
      mockApiService.get.and.returnValue(of([]));

      service.getTrendingMovies().subscribe();

      expect(mockApiService.get).toHaveBeenCalledWith(
        'https://api.example.com/movies/trending',
        jasmine.any(Object)
      );
    });

    it('should construct correct URL for streaming offers endpoint', () => {
      mockApiService.get.and.returnValue(of(mockStreamingOffers));

      service.getStreamingOffers(123, 'BE').subscribe();

      expect(mockApiService.get).toHaveBeenCalledWith(
        'https://api.example.com/movies/123/streaming',
        jasmine.any(Object)
      );
    });
  });

  describe('Credentials Handling', () => {
    it('should disable credentials in all API calls', () => {
      mockApiService.get.and.returnValue(of(mockSearchResponse));

      // Test search
      service.search({ page: 1, pageSize: 20 }).subscribe();
      expect(mockApiService.get).toHaveBeenCalledWith(
        jasmine.any(String),
        jasmine.objectContaining({ withCredentials: false })
      );

      mockApiService.get.calls.reset();

      // Test getPopularMovies
      service.getPopularMovies({ page: 1, pageSize: 20 }).subscribe();
      expect(mockApiService.get).toHaveBeenCalledWith(
        jasmine.any(String),
        jasmine.objectContaining({ withCredentials: false })
      );

      mockApiService.get.calls.reset();

      // Test getStreamingOffers
      mockApiService.get.and.returnValue(of(mockStreamingOffers));
      service.getStreamingOffers(123, 'BE').subscribe();
      expect(mockApiService.get).toHaveBeenCalledWith(
        jasmine.any(String),
        jasmine.objectContaining({ withCredentials: false })
      );
    });
  });

  describe('Error Handling', () => {
    it('should propagate API errors from search', (done) => {
      const error = new Error('Search failed');
      mockApiService.get.and.returnValue(throwError(() => error));

      service.search({ page: 1, pageSize: 20 }).subscribe({
        error: (err) => {
          expect(err).toEqual(error);
          done();
        }
      });
    });

    it('should propagate API errors from getPopularMovies', (done) => {
      const error = new Error('Popular failed');
      mockApiService.get.and.returnValue(throwError(() => error));

      service.getPopularMovies({ page: 1, pageSize: 20 }).subscribe({
        error: (err) => {
          expect(err).toEqual(error);
          done();
        }
      });
    });

    it('should propagate API errors from getTrendingMovies', (done) => {
      const error = new Error('Trending failed');
      mockApiService.get.and.returnValue(throwError(() => error));

      service.getTrendingMovies().subscribe({
        error: (err) => {
          expect(err).toEqual(error);
          done();
        }
      });
    });
  });

  describe('Null and Undefined Handling', () => {
    it('should handle null tmdbId in getStreamingOffers', (done) => {
      service.getStreamingOffers(null as any, 'BE').subscribe({
        error: (err) => {
          expect(err.message).toBe('ID du film manquant');
          done();
        }
      });
    });

    it('should handle undefined tmdbId in getStreamingOffers', (done) => {
      service.getStreamingOffers(undefined as any, 'BE').subscribe({
        error: (err) => {
          expect(err.message).toBe('ID du film manquant');
          done();
        }
      });
    });

    it('should handle empty string rawId in getMovieByRouteId', (done) => {
      service.getMovieByRouteId('').subscribe({
        error: (err) => {
          expect(err.message).toBe('ID du film manquant');
          done();
        }
      });
    });

    it('should handle null rawId in getMovieByRouteId', (done) => {
      service.getMovieByRouteId(null as any).subscribe({
        error: (err) => {
          expect(err.message).toBe('ID du film manquant');
          done();
        }
      });
    });
  });

  describe('Observable Behavior', () => {
    it('should return hot observable from search', () => {
      mockApiService.get.and.returnValue(of(mockSearchResponse));

      const result1 = service.search({ page: 1, pageSize: 20 });
      const result2 = service.search({ page: 1, pageSize: 20 });

      expect(result1).toBeDefined();
      expect(result2).toBeDefined();
    });

    it('should be subscribable multiple times', (done) => {
      mockApiService.get.and.returnValue(of(mockMovieDetails));

      let callCount = 0;

      service.getMovieByRouteId('123').subscribe(() => callCount++);
      service.getMovieByRouteId('123').subscribe(() => callCount++);

      setTimeout(() => {
        expect(callCount).toBe(2);
        done();
      }, 10);
    });
  });
});
