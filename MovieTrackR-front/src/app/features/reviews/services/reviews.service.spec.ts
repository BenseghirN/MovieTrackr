import { TestBed } from '@angular/core/testing';
import { ReviewService } from './reviews.service';
import { ApiService } from '../../../core/services/api.service';
import { ConfigService } from '../../../core/services/config.service';
import { of, throwError } from 'rxjs';
import {
  PagedReviews,
  ReviewListItem,
  ReviewDetails,
  CreateReviewModel,
  UpdateReviewModel,
  MovieReviewsQueryParams,
  UserReviewsQueryParams
} from '../models/review.model';

describe('ReviewService', () => {
  let service: ReviewService;
  let mockApiService: jasmine.SpyObj<ApiService>;
  let mockConfigService: jasmine.SpyObj<ConfigService>;

  const mockConfigUrl = 'https://api.example.com';

  const mockReviewListItem: ReviewListItem = {
    id: 'review-1',
    movieId: 'movie-1',
    movieTitle: 'Test Movie',
    posterUrl: '/poster.jpg',
    userId: 'user-1',
    userName: 'John Doe',
    avatarUrl: '/avatar.jpg',
    rating: 4,
    content: 'Great movie with amazing cinematography',
    likesCount: 10,
    commentsCount: 3,
    createdAt: '2024-01-01T00:00:00Z',
    hasLiked: false
  };

  const mockReviewDetails: ReviewDetails = {
    id: 'review-1',
    movieId: 'movie-1',
    userId: 'user-1',
    rating: 4,
    content: 'Great movie with amazing cinematography',
    likesCount: 10,
    commentsCount: 3,
    createdAt: '2024-01-01T00:00:00Z'
  };

  const mockPagedReviews: PagedReviews = {
    items: [mockReviewListItem],
    totalCount: 50,
    page: 1,
    pageSize: 20,
    totalPages: 3
  };

  beforeEach(() => {
    mockApiService = jasmine.createSpyObj('ApiService', ['get', 'post', 'put', 'delete']);
    mockConfigService = jasmine.createSpyObj('ConfigService', [], {
      apiUrl: mockConfigUrl
    });

    TestBed.configureTestingModule({
      providers: [
        ReviewService,
        { provide: ApiService, useValue: mockApiService },
        { provide: ConfigService, useValue: mockConfigService }
      ]
    });

    service = TestBed.inject(ReviewService);
  });

  describe('Service Initialization', () => {
    it('should be created', () => {
      expect(service).toBeTruthy();
    });
  });

  describe('getMovieReviews', () => {
    it('should call api.get with correct endpoint', () => {
      mockApiService.get.and.returnValue(of(mockPagedReviews));

      const params: MovieReviewsQueryParams = {
        page: 1,
        pageSize: 20
      };

      service.getMovieReviews('movie-1', params).subscribe();

      expect(mockApiService.get).toHaveBeenCalledWith(
        `${mockConfigUrl}/reviews/by-movie/movie-1`,
        {
          params: { Page: 1, PageSize: 20 },
          withCredentials: false
        }
      );
    });

    it('should use default page when not provided', () => {
      mockApiService.get.and.returnValue(of(mockPagedReviews));

      const params: MovieReviewsQueryParams = {
        pageSize: 20
      };

      service.getMovieReviews('movie-1', params).subscribe();

      expect(mockApiService.get).toHaveBeenCalledWith(
        jasmine.any(String),
        jasmine.objectContaining({
          params: jasmine.objectContaining({ Page: 1 })
        })
      );
    });

    it('should use default pageSize when not provided', () => {
      mockApiService.get.and.returnValue(of(mockPagedReviews));

      const params: MovieReviewsQueryParams = {
        page: 1
      };

      service.getMovieReviews('movie-1', params).subscribe();

      expect(mockApiService.get).toHaveBeenCalledWith(
        jasmine.any(String),
        jasmine.objectContaining({
          params: jasmine.objectContaining({ PageSize: 20 })
        })
      );
    });

    it('should include sort param when provided', () => {
      mockApiService.get.and.returnValue(of(mockPagedReviews));

      const params: MovieReviewsQueryParams = {
        page: 1,
        pageSize: 20,
        sort: 'MostLiked'
      };

      service.getMovieReviews('movie-1', params).subscribe();

      expect(mockApiService.get).toHaveBeenCalledWith(
        jasmine.any(String),
        jasmine.objectContaining({
          params: jasmine.objectContaining({ Sort: 'MostLiked' })
        })
      );
    });

    it('should include ratingFilter when provided', () => {
      mockApiService.get.and.returnValue(of(mockPagedReviews));

      const params: MovieReviewsQueryParams = {
        page: 1,
        pageSize: 20,
        ratingFilter: 4
      };

      service.getMovieReviews('movie-1', params).subscribe();

      expect(mockApiService.get).toHaveBeenCalledWith(
        jasmine.any(String),
        jasmine.objectContaining({
          params: jasmine.objectContaining({ RatinFilter: 4 })
        })
      );
    });

    it('should not include ratingFilter when null', () => {
      mockApiService.get.and.returnValue(of(mockPagedReviews));

      const params: MovieReviewsQueryParams = {
        page: 1,
        pageSize: 20,
        ratingFilter: null
      };

      service.getMovieReviews('movie-1', params).subscribe();

      const callArgs = mockApiService.get.calls.mostRecent().args[1] as any;
      expect(callArgs.params['RatinFilter']).toBeUndefined();
    });

    it('should disable credentials in request', () => {
      mockApiService.get.and.returnValue(of(mockPagedReviews));

      const params: MovieReviewsQueryParams = { page: 1, pageSize: 20 };

      service.getMovieReviews('movie-1', params).subscribe();

      expect(mockApiService.get).toHaveBeenCalledWith(
        jasmine.any(String),
        jasmine.objectContaining({ withCredentials: false })
      );
    });

    it('should return paged reviews', (done) => {
      mockApiService.get.and.returnValue(of(mockPagedReviews));

      const params: MovieReviewsQueryParams = { page: 1, pageSize: 20 };

      service.getMovieReviews('movie-1', params).subscribe(result => {
        expect(result).toEqual(mockPagedReviews);
        expect(result.items.length).toBe(1);
        done();
      });
    });

    it('should handle API error', (done) => {
      const error = new Error('Failed to fetch reviews');
      mockApiService.get.and.returnValue(throwError(() => error));

      const params: MovieReviewsQueryParams = { page: 1, pageSize: 20 };

      service.getMovieReviews('movie-1', params).subscribe({
        error: (err) => {
          expect(err).toEqual(error);
          done();
        }
      });
    });
  });

  describe('getReviewById', () => {
    it('should call api.get with correct endpoint', () => {
      mockApiService.get.and.returnValue(of(mockReviewDetails));

      service.getReviewById('review-1').subscribe();

      expect(mockApiService.get).toHaveBeenCalledWith(
        `${mockConfigUrl}/reviews/review-1`,
        { withCredentials: false }
      );
    });

    it('should disable credentials in request', () => {
      mockApiService.get.and.returnValue(of(mockReviewDetails));

      service.getReviewById('review-1').subscribe();

      expect(mockApiService.get).toHaveBeenCalledWith(
        jasmine.any(String),
        jasmine.objectContaining({ withCredentials: false })
      );
    });

    it('should return review details', (done) => {
      mockApiService.get.and.returnValue(of(mockReviewDetails));

      service.getReviewById('review-1').subscribe(result => {
        expect(result).toEqual(mockReviewDetails);
        done();
      });
    });

    it('should handle API error', (done) => {
      const error = new Error('Review not found');
      mockApiService.get.and.returnValue(throwError(() => error));

      service.getReviewById('non-existent').subscribe({
        error: (err) => {
          expect(err).toEqual(error);
          done();
        }
      });
    });

    it('should construct correct URL with review ID', () => {
      mockApiService.get.and.returnValue(of(mockReviewDetails));

      service.getReviewById('review-123').subscribe();

      expect(mockApiService.get).toHaveBeenCalledWith(
        `${mockConfigUrl}/reviews/review-123`,
        jasmine.any(Object)
      );
    });
  });

  describe('getUserReviews', () => {
    it('should call api.get with correct endpoint', () => {
      mockApiService.get.and.returnValue(of(mockPagedReviews));

      const params: UserReviewsQueryParams = {
        page: 1,
        pageSize: 20
      };

      service.getUserReviews('user-1', params).subscribe();

      expect(mockApiService.get).toHaveBeenCalledWith(
        `${mockConfigUrl}/reviews/by-user/user-1`,
        {
          params: { Page: 1, PageSize: 20 },
          withCredentials: true
        }
      );
    });

    it('should use default page when not provided', () => {
      mockApiService.get.and.returnValue(of(mockPagedReviews));

      const params: UserReviewsQueryParams = { pageSize: 20 };

      service.getUserReviews('user-1', params).subscribe();

      expect(mockApiService.get).toHaveBeenCalledWith(
        jasmine.any(String),
        jasmine.objectContaining({
          params: jasmine.objectContaining({ Page: 1 })
        })
      );
    });

    it('should use default pageSize when not provided', () => {
      mockApiService.get.and.returnValue(of(mockPagedReviews));

      const params: UserReviewsQueryParams = { page: 1 };

      service.getUserReviews('user-1', params).subscribe();

      expect(mockApiService.get).toHaveBeenCalledWith(
        jasmine.any(String),
        jasmine.objectContaining({
          params: jasmine.objectContaining({ PageSize: 20 })
        })
      );
    });

    it('should include sort param when provided', () => {
      mockApiService.get.and.returnValue(of(mockPagedReviews));

      const params: UserReviewsQueryParams = {
        page: 1,
        pageSize: 20,
        sort: 'Newest'
      };

      service.getUserReviews('user-1', params).subscribe();

      expect(mockApiService.get).toHaveBeenCalledWith(
        jasmine.any(String),
        jasmine.objectContaining({
          params: jasmine.objectContaining({ Sort: 'Newest' })
        })
      );
    });

    it('should include ratingFilter when provided', () => {
      mockApiService.get.and.returnValue(of(mockPagedReviews));

      const params: UserReviewsQueryParams = {
        page: 1,
        pageSize: 20,
        ratingFilter: 5
      };

      service.getUserReviews('user-1', params).subscribe();

      expect(mockApiService.get).toHaveBeenCalledWith(
        jasmine.any(String),
        jasmine.objectContaining({
          params: jasmine.objectContaining({ RatingFilter: 5 })
        })
      );
    });

    it('should enable credentials in request', () => {
      mockApiService.get.and.returnValue(of(mockPagedReviews));

      const params: UserReviewsQueryParams = { page: 1, pageSize: 20 };

      service.getUserReviews('user-1', params).subscribe();

      expect(mockApiService.get).toHaveBeenCalledWith(
        jasmine.any(String),
        jasmine.objectContaining({ withCredentials: true })
      );
    });

    it('should return paged reviews', (done) => {
      mockApiService.get.and.returnValue(of(mockPagedReviews));

      const params: UserReviewsQueryParams = { page: 1, pageSize: 20 };

      service.getUserReviews('user-1', params).subscribe(result => {
        expect(result).toEqual(mockPagedReviews);
        done();
      });
    });

    it('should handle API error', (done) => {
      const error = new Error('Failed to fetch user reviews');
      mockApiService.get.and.returnValue(throwError(() => error));

      const params: UserReviewsQueryParams = { page: 1, pageSize: 20 };

      service.getUserReviews('user-1', params).subscribe({
        error: (err) => {
          expect(err).toEqual(error);
          done();
        }
      });
    });
  });

  describe('createReview', () => {
    it('should call api.post with correct endpoint and payload', () => {
      mockApiService.post.and.returnValue(of(void 0));

      const newReview: CreateReviewModel = {
        movieId: 'movie-1',
        rating: 5,
        content: 'Excellent movie!'
      };

      service.createReview(newReview).subscribe();

      expect(mockApiService.post).toHaveBeenCalledWith(
        `${mockConfigUrl}/reviews`,
        newReview,
        { withCredentials: true }
      );
    });

    it('should enable credentials in request', () => {
      mockApiService.post.and.returnValue(of(void 0));

      const newReview: CreateReviewModel = {
        movieId: 'movie-1',
        rating: 5,
        content: 'Test'
      };

      service.createReview(newReview).subscribe();

      expect(mockApiService.post).toHaveBeenCalledWith(
        jasmine.any(String),
        jasmine.any(Object),
        jasmine.objectContaining({ withCredentials: true })
      );
    });

    it('should pass review object as payload', () => {
      mockApiService.post.and.returnValue(of(void 0));

      const newReview: CreateReviewModel = {
        movieId: 'movie-123',
        rating: 4,
        content: 'Great content'
      };

      service.createReview(newReview).subscribe();

      expect(mockApiService.post).toHaveBeenCalledWith(
        jasmine.any(String),
        newReview,
        jasmine.any(Object)
      );
    });

    it('should return void on success', (done) => {
      mockApiService.post.and.returnValue(of(void 0));

      const newReview: CreateReviewModel = {
        movieId: 'movie-1',
        rating: 5,
        content: 'Test'
      };

      service.createReview(newReview).subscribe(result => {
        expect(result).toBeUndefined();
        done();
      });
    });

    it('should handle API error', (done) => {
      const error = new Error('Failed to create review');
      mockApiService.post.and.returnValue(throwError(() => error));

      const newReview: CreateReviewModel = {
        movieId: 'movie-1',
        rating: 5,
        content: 'Test'
      };

      service.createReview(newReview).subscribe({
        error: (err) => {
          expect(err).toEqual(error);
          done();
        }
      });
    });

    it('should handle validation error from API', (done) => {
      const error = { status: 400, errors: { content: ['Too short'] } };
      mockApiService.post.and.returnValue(throwError(() => error));

      const newReview: CreateReviewModel = {
        movieId: 'movie-1',
        rating: 5,
        content: 'Bad'
      };

      service.createReview(newReview).subscribe({
        error: (err) => {
          expect(err.errors.content).toBeDefined();
          done();
        }
      });
    });
  });

  describe('updateReview', () => {
    it('should call api.put with correct endpoint and payload', () => {
      mockApiService.put.and.returnValue(of(void 0));

      const updatedReview: UpdateReviewModel = {
        rating: 5,
        content: 'Updated content'
      };

      service.updateReview('review-1', updatedReview).subscribe();

      expect(mockApiService.put).toHaveBeenCalledWith(
        `${mockConfigUrl}/reviews/review-1`,
        updatedReview,
        { withCredentials: true }
      );
    });

    it('should enable credentials in request', () => {
      mockApiService.put.and.returnValue(of(void 0));

      const updatedReview: UpdateReviewModel = {
        rating: 5,
        content: 'Test'
      };

      service.updateReview('review-1', updatedReview).subscribe();

      expect(mockApiService.put).toHaveBeenCalledWith(
        jasmine.any(String),
        jasmine.any(Object),
        jasmine.objectContaining({ withCredentials: true })
      );
    });

    it('should include review ID in URL', () => {
      mockApiService.put.and.returnValue(of(void 0));

      const updatedReview: UpdateReviewModel = {
        rating: 5,
        content: 'Updated'
      };

      service.updateReview('review-456', updatedReview).subscribe();

      expect(mockApiService.put).toHaveBeenCalledWith(
        `${mockConfigUrl}/reviews/review-456`,
        jasmine.any(Object),
        jasmine.any(Object)
      );
    });

    it('should return void on success', (done) => {
      mockApiService.put.and.returnValue(of(void 0));

      const updatedReview: UpdateReviewModel = {
        rating: 5,
        content: 'Updated'
      };

      service.updateReview('review-1', updatedReview).subscribe(result => {
        expect(result).toBeUndefined();
        done();
      });
    });

    it('should handle API error', (done) => {
      const error = new Error('Failed to update review');
      mockApiService.put.and.returnValue(throwError(() => error));

      const updatedReview: UpdateReviewModel = {
        rating: 5,
        content: 'Updated'
      };

      service.updateReview('review-1', updatedReview).subscribe({
        error: (err) => {
          expect(err).toEqual(error);
          done();
        }
      });
    });

    it('should handle 404 when review not found', (done) => {
      const error = { status: 404, message: 'Review not found' };
      mockApiService.put.and.returnValue(throwError(() => error));

      const updatedReview: UpdateReviewModel = {
        rating: 5,
        content: 'Updated'
      };

      service.updateReview('non-existent', updatedReview).subscribe({
        error: (err) => {
          expect(err.status).toBe(404);
          done();
        }
      });
    });
  });

  describe('deleteReview', () => {
    it('should call api.delete with correct endpoint', () => {
      mockApiService.delete.and.returnValue(of(void 0));

      service.deleteReview('review-1').subscribe();

      expect(mockApiService.delete).toHaveBeenCalledWith(
        `${mockConfigUrl}/reviews/review-1`,
        { withCredentials: true }
      );
    });

    it('should enable credentials in request', () => {
      mockApiService.delete.and.returnValue(of(void 0));

      service.deleteReview('review-1').subscribe();

      expect(mockApiService.delete).toHaveBeenCalledWith(
        jasmine.any(String),
        jasmine.objectContaining({ withCredentials: true })
      );
    });

    it('should include review ID in URL', () => {
      mockApiService.delete.and.returnValue(of(void 0));

      service.deleteReview('review-789').subscribe();

      expect(mockApiService.delete).toHaveBeenCalledWith(
        `${mockConfigUrl}/reviews/review-789`,
        jasmine.any(Object)
      );
    });

    it('should return void on success', (done) => {
      mockApiService.delete.and.returnValue(of(void 0));

      service.deleteReview('review-1').subscribe(result => {
        expect(result).toBeUndefined();
        done();
      });
    });

    it('should handle API error', (done) => {
      const error = new Error('Failed to delete review');
      mockApiService.delete.and.returnValue(throwError(() => error));

      service.deleteReview('review-1').subscribe({
        error: (err) => {
          expect(err).toEqual(error);
          done();
        }
      });
    });

    it('should handle 404 when review not found', (done) => {
      const error = { status: 404, message: 'Review not found' };
      mockApiService.delete.and.returnValue(throwError(() => error));

      service.deleteReview('non-existent').subscribe({
        error: (err) => {
          expect(err.status).toBe(404);
          done();
        }
      });
    });

    it('should handle 403 when user not authorized', (done) => {
      const error = { status: 403, message: 'Forbidden' };
      mockApiService.delete.and.returnValue(throwError(() => error));

      service.deleteReview('review-1').subscribe({
        error: (err) => {
          expect(err.status).toBe(403);
          done();
        }
      });
    });
  });

  describe('API URL Construction', () => {
    it('should use config.apiUrl from ConfigService', () => {
      mockApiService.get.and.returnValue(of(mockPagedReviews));

      const params: MovieReviewsQueryParams = { page: 1, pageSize: 20 };
      service.getMovieReviews('movie-1', params).subscribe();

      const callArgs = mockApiService.get.calls.mostRecent().args;
      expect(callArgs[0]).toContain(mockConfigUrl);
    });

    it('should construct correct endpoint URLs', () => {
      mockApiService.get.and.returnValue(of(mockPagedReviews));
      mockApiService.post.and.returnValue(of(void 0));
      mockApiService.put.and.returnValue(of(void 0));
      mockApiService.delete.and.returnValue(of(void 0));

      const params: MovieReviewsQueryParams = { page: 1, pageSize: 20 };

      // Movie reviews
      service.getMovieReviews('movie-1', params).subscribe();
      expect(mockApiService.get).toHaveBeenCalledWith(
        jasmine.stringContaining('/reviews/by-movie/'),
        jasmine.any(Object)
      );

      // User reviews
      service.getUserReviews('user-1', params as UserReviewsQueryParams).subscribe();
      expect(mockApiService.get).toHaveBeenCalledWith(
        jasmine.stringContaining('/reviews/by-user/'),
        jasmine.any(Object)
      );

      // Create review
      service.createReview({
        movieId: 'movie-1',
        rating: 5,
        content: 'Test'
      }).subscribe();
      expect(mockApiService.post).toHaveBeenCalledWith(
        `${mockConfigUrl}/reviews`,
        jasmine.any(Object),
        jasmine.any(Object)
      );
    });
  });

  describe('Query Parameters', () => {
    it('should handle all sort options for movie reviews', () => {
      mockApiService.get.and.returnValue(of(mockPagedReviews));

      const sortOptions = ['Newest', 'Oldest', 'HighestRating', 'LowestRating', 'MostLiked', 'MostCommented'] as const;

      sortOptions.forEach(sort => {
        const params: MovieReviewsQueryParams = { page: 1, pageSize: 20, sort };
        service.getMovieReviews('movie-1', params).subscribe();

        expect(mockApiService.get).toHaveBeenCalledWith(
          jasmine.any(String),
          jasmine.objectContaining({
            params: jasmine.objectContaining({ Sort: sort })
          })
        );
      });
    });

    it('should handle all sort options for user reviews', () => {
      mockApiService.get.and.returnValue(of(mockPagedReviews));

      const sortOptions = ['Newest', 'Oldest', 'HighestRating', 'LowestRating'] as const;

      sortOptions.forEach(sort => {
        const params: UserReviewsQueryParams = { page: 1, pageSize: 20, sort };
        service.getUserReviews('user-1', params).subscribe();

        expect(mockApiService.get).toHaveBeenCalledWith(
          jasmine.any(String),
          jasmine.objectContaining({
            params: jasmine.objectContaining({ Sort: sort })
          })
        );
      });
    });

    it('should handle different page numbers', () => {
      mockApiService.get.and.returnValue(of(mockPagedReviews));

      for (let page = 1; page <= 5; page++) {
        const params: MovieReviewsQueryParams = { page, pageSize: 20 };
        service.getMovieReviews('movie-1', params).subscribe();

        expect(mockApiService.get).toHaveBeenCalledWith(
          jasmine.any(String),
          jasmine.objectContaining({
            params: jasmine.objectContaining({ Page: page })
          })
        );
      }
    });

    it('should handle different page sizes', () => {
      mockApiService.get.and.returnValue(of(mockPagedReviews));

      const pageSizes = [10, 20, 50, 100];

      pageSizes.forEach(pageSize => {
        const params: MovieReviewsQueryParams = { page: 1, pageSize };
        service.getMovieReviews('movie-1', params).subscribe();

        expect(mockApiService.get).toHaveBeenCalledWith(
          jasmine.any(String),
          jasmine.objectContaining({
            params: jasmine.objectContaining({ PageSize: pageSize })
          })
        );
      });
    });
  });

  describe('Credentials Handling', () => {
    it('should disable credentials for movie reviews', () => {
      mockApiService.get.and.returnValue(of(mockPagedReviews));

      const params: MovieReviewsQueryParams = { page: 1, pageSize: 20 };
      service.getMovieReviews('movie-1', params).subscribe();

      expect(mockApiService.get).toHaveBeenCalledWith(
        jasmine.any(String),
        jasmine.objectContaining({ withCredentials: false })
      );
    });

    it('should enable credentials for user reviews', () => {
      mockApiService.get.and.returnValue(of(mockPagedReviews));

      const params: UserReviewsQueryParams = { page: 1, pageSize: 20 };
      service.getUserReviews('user-1', params).subscribe();

      expect(mockApiService.get).toHaveBeenCalledWith(
        jasmine.any(String),
        jasmine.objectContaining({ withCredentials: true })
      );
    });

    it('should enable credentials for creating reviews', () => {
      mockApiService.post.and.returnValue(of(void 0));

      const newReview: CreateReviewModel = {
        movieId: 'movie-1',
        rating: 5,
        content: 'Test'
      };

      service.createReview(newReview).subscribe();

      expect(mockApiService.post).toHaveBeenCalledWith(
        jasmine.any(String),
        jasmine.any(Object),
        jasmine.objectContaining({ withCredentials: true })
      );
    });

    it('should enable credentials for updating reviews', () => {
      mockApiService.put.and.returnValue(of(void 0));

      const updatedReview: UpdateReviewModel = {
        rating: 5,
        content: 'Updated'
      };

      service.updateReview('review-1', updatedReview).subscribe();

      expect(mockApiService.put).toHaveBeenCalledWith(
        jasmine.any(String),
        jasmine.any(Object),
        jasmine.objectContaining({ withCredentials: true })
      );
    });

    it('should enable credentials for deleting reviews', () => {
      mockApiService.delete.and.returnValue(of(void 0));

      service.deleteReview('review-1').subscribe();

      expect(mockApiService.delete).toHaveBeenCalledWith(
        jasmine.any(String),
        jasmine.objectContaining({ withCredentials: true })
      );
    });
  });

  describe('Observable Behavior', () => {
    it('should be subscribable multiple times', (done) => {
      mockApiService.get.and.returnValue(of(mockPagedReviews));

      const params: MovieReviewsQueryParams = { page: 1, pageSize: 20 };
      let callCount = 0;

      service.getMovieReviews('movie-1', params).subscribe(() => callCount++);
      service.getMovieReviews('movie-1', params).subscribe(() => callCount++);

      setTimeout(() => {
        expect(callCount).toBe(2);
        done();
      }, 10);
    });

    it('should handle concurrent requests', (done) => {
      mockApiService.get.and.returnValue(of(mockPagedReviews));
      mockApiService.post.and.returnValue(of(void 0));

      const params: MovieReviewsQueryParams = { page: 1, pageSize: 20 };
      let getCallCount = 0;
      let postCallCount = 0;

      service.getMovieReviews('movie-1', params).subscribe(() => getCallCount++);
      service.createReview({
        movieId: 'movie-1',
        rating: 5,
        content: 'Test'
      }).subscribe(() => postCallCount++);

      setTimeout(() => {
        expect(getCallCount).toBe(1);
        expect(postCallCount).toBe(1);
        done();
      }, 10);
    });
  });
});
