import { TestBed } from '@angular/core/testing';
import { ReviewLikesService } from './review-likes.service';
import { ApiService } from '../../../core/services/api.service';
import { ConfigService } from '../../../core/services/config.service';
import { of, throwError } from 'rxjs';

describe('ReviewLikesService', () => {
  let service: ReviewLikesService;
  let mockApiService: jasmine.SpyObj<ApiService>;
  let mockConfigService: jasmine.SpyObj<ConfigService>;

  const mockConfigUrl = 'https://api.example.com';

  beforeEach(() => {
    mockApiService = jasmine.createSpyObj('ApiService', ['post', 'delete']);
    mockConfigService = jasmine.createSpyObj('ConfigService', [], {
      apiUrl: mockConfigUrl
    });

    TestBed.configureTestingModule({
      providers: [
        ReviewLikesService,
        { provide: ApiService, useValue: mockApiService },
        { provide: ConfigService, useValue: mockConfigService }
      ]
    });

    service = TestBed.inject(ReviewLikesService);
  });

  describe('Service Initialization', () => {
    it('should be created', () => {
      expect(service).toBeTruthy();
    });
  });

  describe('likeReview', () => {
    it('should call api.post with correct endpoint', () => {
      mockApiService.post.and.returnValue(of(void 0));

      service.likeReview('review-1').subscribe();

      expect(mockApiService.post).toHaveBeenCalledWith(
        `${mockConfigUrl}/reviews/review-1/likes`,
        null,
        { withCredentials: true }
      );
    });

    it('should pass null as payload', () => {
      mockApiService.post.and.returnValue(of(void 0));

      service.likeReview('review-1').subscribe();

      expect(mockApiService.post).toHaveBeenCalledWith(
        jasmine.any(String),
        null,
        jasmine.any(Object)
      );
    });

    it('should enable credentials in request', () => {
      mockApiService.post.and.returnValue(of(void 0));

      service.likeReview('review-1').subscribe();

      expect(mockApiService.post).toHaveBeenCalledWith(
        jasmine.any(String),
        null,
        jasmine.objectContaining({ withCredentials: true })
      );
    });

    it('should construct correct URL with review ID', () => {
      mockApiService.post.and.returnValue(of(void 0));

      service.likeReview('review-123').subscribe();

      expect(mockApiService.post).toHaveBeenCalledWith(
        `${mockConfigUrl}/reviews/review-123/likes`,
        null,
        jasmine.any(Object)
      );
    });

    it('should return void on success', (done) => {
      mockApiService.post.and.returnValue(of(void 0));

      service.likeReview('review-1').subscribe(result => {
        expect(result).toBeUndefined();
        done();
      });
    });

    it('should handle API error', (done) => {
      const error = new Error('Failed to like review');
      mockApiService.post.and.returnValue(throwError(() => error));

      service.likeReview('review-1').subscribe({
        error: (err) => {
          expect(err).toEqual(error);
          done();
        }
      });
    });

    it('should handle 409 conflict when already liked', (done) => {
      const error = { status: 409, message: 'Already liked' };
      mockApiService.post.and.returnValue(throwError(() => error));

      service.likeReview('review-1').subscribe({
        error: (err) => {
          expect(err.status).toBe(409);
          done();
        }
      });
    });

    it('should handle 404 when review not found', (done) => {
      const error = { status: 404, message: 'Review not found' };
      mockApiService.post.and.returnValue(throwError(() => error));

      service.likeReview('non-existent').subscribe({
        error: (err) => {
          expect(err.status).toBe(404);
          done();
        }
      });
    });
  });

  describe('unlikeReview', () => {
    it('should call api.delete with correct endpoint', () => {
      mockApiService.delete.and.returnValue(of(void 0));

      service.unlikeReview('review-1').subscribe();

      expect(mockApiService.delete).toHaveBeenCalledWith(
        `${mockConfigUrl}/reviews/review-1/likes`,
        { withCredentials: true }
      );
    });

    it('should enable credentials in request', () => {
      mockApiService.delete.and.returnValue(of(void 0));

      service.unlikeReview('review-1').subscribe();

      expect(mockApiService.delete).toHaveBeenCalledWith(
        jasmine.any(String),
        jasmine.objectContaining({ withCredentials: true })
      );
    });

    it('should construct correct URL with review ID', () => {
      mockApiService.delete.and.returnValue(of(void 0));

      service.unlikeReview('review-456').subscribe();

      expect(mockApiService.delete).toHaveBeenCalledWith(
        `${mockConfigUrl}/reviews/review-456/likes`,
        jasmine.any(Object)
      );
    });

    it('should return void on success', (done) => {
      mockApiService.delete.and.returnValue(of(void 0));

      service.unlikeReview('review-1').subscribe(result => {
        expect(result).toBeUndefined();
        done();
      });
    });

    it('should handle API error', (done) => {
      const error = new Error('Failed to unlike review');
      mockApiService.delete.and.returnValue(throwError(() => error));

      service.unlikeReview('review-1').subscribe({
        error: (err) => {
          expect(err).toEqual(error);
          done();
        }
      });
    });

    it('should handle 404 when like not found', (done) => {
      const error = { status: 404, message: 'Like not found' };
      mockApiService.delete.and.returnValue(throwError(() => error));

      service.unlikeReview('review-1').subscribe({
        error: (err) => {
          expect(err.status).toBe(404);
          done();
        }
      });
    });

    it('should handle 404 when review not found', (done) => {
      const error = { status: 404, message: 'Review not found' };
      mockApiService.delete.and.returnValue(throwError(() => error));

      service.unlikeReview('non-existent').subscribe({
        error: (err) => {
          expect(err.status).toBe(404);
          done();
        }
      });
    });
  });

  describe('API URL Construction', () => {
    it('should use config.apiUrl from ConfigService', () => {
      mockApiService.post.and.returnValue(of(void 0));

      service.likeReview('review-1').subscribe();

      const callArgs = mockApiService.post.calls.mostRecent().args;
      expect(callArgs[0]).toContain(mockConfigUrl);
    });

    it('should construct correct likes endpoint URLs', () => {
      mockApiService.post.and.returnValue(of(void 0));
      mockApiService.delete.and.returnValue(of(void 0));

      service.likeReview('review-1').subscribe();
      expect(mockApiService.post).toHaveBeenCalledWith(
        jasmine.stringContaining('/reviews/review-1/likes'),
        null,
        jasmine.any(Object)
      );

      service.unlikeReview('review-1').subscribe();
      expect(mockApiService.delete).toHaveBeenCalledWith(
        jasmine.stringContaining('/reviews/review-1/likes'),
        jasmine.any(Object)
      );
    });

    it('should use different review IDs in URLs', () => {
      mockApiService.post.and.returnValue(of(void 0));

      service.likeReview('review-999').subscribe();

      expect(mockApiService.post).toHaveBeenCalledWith(
        `${mockConfigUrl}/reviews/review-999/likes`,
        null,
        jasmine.any(Object)
      );
    });
  });

  describe('Credentials Handling', () => {
    it('should enable credentials in both methods', () => {
      mockApiService.post.and.returnValue(of(void 0));
      mockApiService.delete.and.returnValue(of(void 0));

      service.likeReview('review-1').subscribe();
      expect(mockApiService.post).toHaveBeenCalledWith(
        jasmine.any(String),
        null,
        jasmine.objectContaining({ withCredentials: true })
      );

      service.unlikeReview('review-1').subscribe();
      expect(mockApiService.delete).toHaveBeenCalledWith(
        jasmine.any(String),
        jasmine.objectContaining({ withCredentials: true })
      );
    });
  });

  describe('Observable Behavior', () => {
    it('should be subscribable multiple times', (done) => {
      mockApiService.post.and.returnValue(of(void 0));

      let callCount = 0;

      service.likeReview('review-1').subscribe(() => callCount++);
      service.likeReview('review-1').subscribe(() => callCount++);

      setTimeout(() => {
        expect(callCount).toBe(2);
        done();
      }, 10);
    });

    it('should handle concurrent like and unlike operations', (done) => {
      mockApiService.post.and.returnValue(of(void 0));
      mockApiService.delete.and.returnValue(of(void 0));

      let likeCalled = false;
      let unlikeCalled = false;

      service.likeReview('review-1').subscribe(() => {
        likeCalled = true;
      });

      service.unlikeReview('review-1').subscribe(() => {
        unlikeCalled = true;
      });

      setTimeout(() => {
        expect(likeCalled).toBe(true);
        expect(unlikeCalled).toBe(true);
        done();
      }, 10);
    });

    it('should handle multiple likes on different reviews', (done) => {
      mockApiService.post.and.returnValue(of(void 0));

      let callCount = 0;

      service.likeReview('review-1').subscribe(() => callCount++);
      service.likeReview('review-2').subscribe(() => callCount++);
      service.likeReview('review-3').subscribe(() => callCount++);

      setTimeout(() => {
        expect(callCount).toBe(3);
        expect(mockApiService.post).toHaveBeenCalledTimes(3);
        done();
      }, 10);
    });
  });

  describe('Error Scenarios', () => {
    it('should propagate network errors from like', (done) => {
      const networkError = new Error('Network error');
      mockApiService.post.and.returnValue(throwError(() => networkError));

      service.likeReview('review-1').subscribe({
        error: (err) => {
          expect(err.message).toBe('Network error');
          done();
        }
      });
    });

    it('should propagate network errors from unlike', (done) => {
      const networkError = new Error('Network error');
      mockApiService.delete.and.returnValue(throwError(() => networkError));

      service.unlikeReview('review-1').subscribe({
        error: (err) => {
          expect(err.message).toBe('Network error');
          done();
        }
      });
    });

    it('should handle server errors', (done) => {
      const serverError = { status: 500, message: 'Internal server error' };
      mockApiService.post.and.returnValue(throwError(() => serverError));

      service.likeReview('review-1').subscribe({
        error: (err) => {
          expect(err.status).toBe(500);
          done();
        }
      });
    });
  });

  describe('Edge Cases', () => {
    it('should handle empty review ID', () => {
      mockApiService.post.and.returnValue(of(void 0));

      service.likeReview('').subscribe();

      expect(mockApiService.post).toHaveBeenCalledWith(
        `${mockConfigUrl}/reviews//likes`,
        null,
        jasmine.any(Object)
      );
    });

    it('should handle special characters in review ID', () => {
      mockApiService.post.and.returnValue(of(void 0));

      const reviewIdWithSpecialChars = 'review-123-abc_def.456';
      service.likeReview(reviewIdWithSpecialChars).subscribe();

      expect(mockApiService.post).toHaveBeenCalledWith(
        `${mockConfigUrl}/reviews/${reviewIdWithSpecialChars}/likes`,
        null,
        jasmine.any(Object)
      );
    });
  });
});
