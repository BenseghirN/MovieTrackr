import { TestBed } from '@angular/core/testing';
import { ReviewCommentsService } from './review-comments.service';
import { ApiService } from '../../../core/services/api.service';
import { ConfigService } from '../../../core/services/config.service';
import { of, throwError } from 'rxjs';
import { PagedComments, ReviewComment } from '../models/review.model';

describe('ReviewCommentsService', () => {
  let service: ReviewCommentsService;
  let mockApiService: jasmine.SpyObj<ApiService>;
  let mockConfigService: jasmine.SpyObj<ConfigService>;

  const mockConfigUrl = 'https://api.example.com';

  const mockReviewComment: ReviewComment = {
    id: 'comment-1',
    reviewId: 'review-1',
    userId: 'user-1',
    userName: 'John Doe',
    content: 'Great review!',
    createdAt: '2024-01-01T00:00:00Z'
  };

  const mockPagedComments: PagedComments = {
    items: [mockReviewComment],
    totalCount: 5,
    page: 1,
    pageSize: 20,
    totalPages: 1
  };

  beforeEach(() => {
    mockApiService = jasmine.createSpyObj('ApiService', ['get', 'post', 'put', 'delete']);
    mockConfigService = jasmine.createSpyObj('ConfigService', [], {
      apiUrl: mockConfigUrl
    });

    TestBed.configureTestingModule({
      providers: [
        ReviewCommentsService,
        { provide: ApiService, useValue: mockApiService },
        { provide: ConfigService, useValue: mockConfigService }
      ]
    });

    service = TestBed.inject(ReviewCommentsService);
  });

  describe('Service Initialization', () => {
    it('should be created', () => {
      expect(service).toBeTruthy();
    });
  });

  describe('getComments', () => {
    it('should call api.get with correct endpoint', () => {
      mockApiService.get.and.returnValue(of(mockPagedComments));

      service.getComments('review-1').subscribe();

      expect(mockApiService.get).toHaveBeenCalledWith(
        `${mockConfigUrl}/reviews/review-1/comments`,
        { params: { page: 1, pageSize: 20 }, withCredentials: true }
      );
    });

    it('should use default page and pageSize when not provided', () => {
      mockApiService.get.and.returnValue(of(mockPagedComments));

      service.getComments('review-1').subscribe();

      expect(mockApiService.get).toHaveBeenCalledWith(
        jasmine.any(String),
        jasmine.objectContaining({
          params: { page: 1, pageSize: 20 }
        })
      );
    });

    it('should accept custom page and pageSize', () => {
      mockApiService.get.and.returnValue(of(mockPagedComments));

      service.getComments('review-1', 2, 50).subscribe();

      expect(mockApiService.get).toHaveBeenCalledWith(
        jasmine.any(String),
        jasmine.objectContaining({
          params: { page: 2, pageSize: 50 }
        })
      );
    });

    it('should enable credentials in request', () => {
      mockApiService.get.and.returnValue(of(mockPagedComments));

      service.getComments('review-1').subscribe();

      expect(mockApiService.get).toHaveBeenCalledWith(
        jasmine.any(String),
        jasmine.objectContaining({ withCredentials: true })
      );
    });

    it('should return paged comments', (done) => {
      mockApiService.get.and.returnValue(of(mockPagedComments));

      service.getComments('review-1').subscribe(result => {
        expect(result).toEqual(mockPagedComments);
        expect(result.items.length).toBe(1);
        done();
      });
    });

    it('should handle API error', (done) => {
      const error = new Error('Failed to fetch comments');
      mockApiService.get.and.returnValue(throwError(() => error));

      service.getComments('review-1').subscribe({
        error: (err) => {
          expect(err).toEqual(error);
          done();
        }
      });
    });

    it('should construct correct URL with review ID', () => {
      mockApiService.get.and.returnValue(of(mockPagedComments));

      service.getComments('review-123').subscribe();

      expect(mockApiService.get).toHaveBeenCalledWith(
        `${mockConfigUrl}/reviews/review-123/comments`,
        jasmine.any(Object)
      );
    });
  });

  describe('createComment', () => {
    it('should call api.post with correct endpoint and payload', () => {
      mockApiService.post.and.returnValue(of(void 0));

      service.createComment('review-1', 'Great movie!').subscribe();

      expect(mockApiService.post).toHaveBeenCalledWith(
        `${mockConfigUrl}/reviews/review-1/comments`,
        { content: 'Great movie!' },
        { withCredentials: true }
      );
    });

    it('should enable credentials in request', () => {
      mockApiService.post.and.returnValue(of(void 0));

      service.createComment('review-1', 'Test').subscribe();

      expect(mockApiService.post).toHaveBeenCalledWith(
        jasmine.any(String),
        jasmine.any(Object),
        jasmine.objectContaining({ withCredentials: true })
      );
    });

    it('should handle empty content', () => {
      mockApiService.post.and.returnValue(of(void 0));

      service.createComment('review-1', '').subscribe();

      expect(mockApiService.post).toHaveBeenCalledWith(
        jasmine.any(String),
        { content: '' },
        jasmine.any(Object)
      );
    });

    it('should return void on success', (done) => {
      mockApiService.post.and.returnValue(of(void 0));

      service.createComment('review-1', 'Test comment').subscribe(result => {
        expect(result).toBeUndefined();
        done();
      });
    });

    it('should handle API error', (done) => {
      const error = new Error('Failed to create comment');
      mockApiService.post.and.returnValue(throwError(() => error));

      service.createComment('review-1', 'Test').subscribe({
        error: (err) => {
          expect(err).toEqual(error);
          done();
        }
      });
    });

    it('should construct correct URL with review ID', () => {
      mockApiService.post.and.returnValue(of(void 0));

      service.createComment('review-456', 'Content').subscribe();

      expect(mockApiService.post).toHaveBeenCalledWith(
        `${mockConfigUrl}/reviews/review-456/comments`,
        jasmine.any(Object),
        jasmine.any(Object)
      );
    });
  });

  describe('updateComment', () => {
    it('should call api.put with correct endpoint and payload', () => {
      mockApiService.put.and.returnValue(of(void 0));

      service.updateComment('review-1', 'comment-1', 'Updated content').subscribe();

      expect(mockApiService.put).toHaveBeenCalledWith(
        `${mockConfigUrl}/reviews/review-1/comments/comment-1`,
        { content: 'Updated content' },
        { withCredentials: true }
      );
    });

    it('should enable credentials in request', () => {
      mockApiService.put.and.returnValue(of(void 0));

      service.updateComment('review-1', 'comment-1', 'Content').subscribe();

      expect(mockApiService.put).toHaveBeenCalledWith(
        jasmine.any(String),
        jasmine.any(Object),
        jasmine.objectContaining({ withCredentials: true })
      );
    });

    it('should include review ID and comment ID in URL', () => {
      mockApiService.put.and.returnValue(of(void 0));

      service.updateComment('review-123', 'comment-456', 'Updated').subscribe();

      expect(mockApiService.put).toHaveBeenCalledWith(
        `${mockConfigUrl}/reviews/review-123/comments/comment-456`,
        jasmine.any(Object),
        jasmine.any(Object)
      );
    });

    it('should return void on success', (done) => {
      mockApiService.put.and.returnValue(of(void 0));

      service.updateComment('review-1', 'comment-1', 'New content').subscribe(result => {
        expect(result).toBeUndefined();
        done();
      });
    });

    it('should handle API error', (done) => {
      const error = new Error('Failed to update comment');
      mockApiService.put.and.returnValue(throwError(() => error));

      service.updateComment('review-1', 'comment-1', 'Content').subscribe({
        error: (err) => {
          expect(err).toEqual(error);
          done();
        }
      });
    });
  });

  describe('deleteComment', () => {
    it('should call api.delete with correct endpoint', () => {
      mockApiService.delete.and.returnValue(of(void 0));

      service.deleteComment('review-1', 'comment-1').subscribe();

      expect(mockApiService.delete).toHaveBeenCalledWith(
        `${mockConfigUrl}/reviews/review-1/comments/comment-1`,
        { withCredentials: true }
      );
    });

    it('should enable credentials in request', () => {
      mockApiService.delete.and.returnValue(of(void 0));

      service.deleteComment('review-1', 'comment-1').subscribe();

      expect(mockApiService.delete).toHaveBeenCalledWith(
        jasmine.any(String),
        jasmine.objectContaining({ withCredentials: true })
      );
    });

    it('should include review ID and comment ID in URL', () => {
      mockApiService.delete.and.returnValue(of(void 0));

      service.deleteComment('review-789', 'comment-012').subscribe();

      expect(mockApiService.delete).toHaveBeenCalledWith(
        `${mockConfigUrl}/reviews/review-789/comments/comment-012`,
        jasmine.any(Object)
      );
    });

    it('should return void on success', (done) => {
      mockApiService.delete.and.returnValue(of(void 0));

      service.deleteComment('review-1', 'comment-1').subscribe(result => {
        expect(result).toBeUndefined();
        done();
      });
    });

    it('should handle API error', (done) => {
      const error = new Error('Failed to delete comment');
      mockApiService.delete.and.returnValue(throwError(() => error));

      service.deleteComment('review-1', 'comment-1').subscribe({
        error: (err) => {
          expect(err).toEqual(error);
          done();
        }
      });
    });

    it('should handle 404 error when comment not found', (done) => {
      const error = { status: 404, message: 'Comment not found' };
      mockApiService.delete.and.returnValue(throwError(() => error));

      service.deleteComment('review-1', 'non-existent').subscribe({
        error: (err) => {
          expect(err.status).toBe(404);
          done();
        }
      });
    });
  });

  describe('API URL Construction', () => {
    it('should use config.apiUrl from ConfigService', () => {
      mockApiService.get.and.returnValue(of(mockPagedComments));

      service.getComments('review-1').subscribe();

      const callArgs = mockApiService.get.calls.mostRecent().args;
      expect(callArgs[0]).toContain(mockConfigUrl);
    });

    it('should construct correct URLs for all methods', () => {
      mockApiService.get.and.returnValue(of(mockPagedComments));
      mockApiService.post.and.returnValue(of(void 0));
      mockApiService.put.and.returnValue(of(void 0));
      mockApiService.delete.and.returnValue(of(void 0));

      service.getComments('review-1').subscribe();
      expect(mockApiService.get).toHaveBeenCalledWith(
        jasmine.stringContaining('/reviews/review-1/comments'),
        jasmine.any(Object)
      );

      service.createComment('review-1', 'test').subscribe();
      expect(mockApiService.post).toHaveBeenCalledWith(
        jasmine.stringContaining('/reviews/review-1/comments'),
        jasmine.any(Object),
        jasmine.any(Object)
      );

      service.updateComment('review-1', 'comment-1', 'test').subscribe();
      expect(mockApiService.put).toHaveBeenCalledWith(
        jasmine.stringContaining('/reviews/review-1/comments/comment-1'),
        jasmine.any(Object),
        jasmine.any(Object)
      );

      service.deleteComment('review-1', 'comment-1').subscribe();
      expect(mockApiService.delete).toHaveBeenCalledWith(
        jasmine.stringContaining('/reviews/review-1/comments/comment-1'),
        jasmine.any(Object)
      );
    });
  });

  describe('Credentials Handling', () => {
    it('should enable credentials in all methods', () => {
      mockApiService.get.and.returnValue(of(mockPagedComments));
      mockApiService.post.and.returnValue(of(void 0));
      mockApiService.put.and.returnValue(of(void 0));
      mockApiService.delete.and.returnValue(of(void 0));

      service.getComments('review-1').subscribe();
      expect(mockApiService.get).toHaveBeenCalledWith(
        jasmine.any(String),
        jasmine.objectContaining({ withCredentials: true })
      );

      service.createComment('review-1', 'test').subscribe();
      expect(mockApiService.post).toHaveBeenCalledWith(
        jasmine.any(String),
        jasmine.any(Object),
        jasmine.objectContaining({ withCredentials: true })
      );

      service.updateComment('review-1', 'comment-1', 'test').subscribe();
      expect(mockApiService.put).toHaveBeenCalledWith(
        jasmine.any(String),
        jasmine.any(Object),
        jasmine.objectContaining({ withCredentials: true })
      );

      service.deleteComment('review-1', 'comment-1').subscribe();
      expect(mockApiService.delete).toHaveBeenCalledWith(
        jasmine.any(String),
        jasmine.objectContaining({ withCredentials: true })
      );
    });
  });

  describe('Pagination', () => {
    it('should handle page 1 with default pageSize', () => {
      mockApiService.get.and.returnValue(of(mockPagedComments));

      service.getComments('review-1', 1, 20).subscribe();

      expect(mockApiService.get).toHaveBeenCalledWith(
        jasmine.any(String),
        jasmine.objectContaining({
          params: { page: 1, pageSize: 20 }
        })
      );
    });

    it('should handle higher page numbers', () => {
      mockApiService.get.and.returnValue(of(mockPagedComments));

      service.getComments('review-1', 5, 20).subscribe();

      expect(mockApiService.get).toHaveBeenCalledWith(
        jasmine.any(String),
        jasmine.objectContaining({
          params: { page: 5, pageSize: 20 }
        })
      );
    });

    it('should handle custom page sizes', () => {
      mockApiService.get.and.returnValue(of(mockPagedComments));

      service.getComments('review-1', 1, 100).subscribe();

      expect(mockApiService.get).toHaveBeenCalledWith(
        jasmine.any(String),
        jasmine.objectContaining({
          params: { page: 1, pageSize: 100 }
        })
      );
    });
  });

  describe('Observable Behavior', () => {
    it('should be subscribable multiple times', (done) => {
      mockApiService.get.and.returnValue(of(mockPagedComments));

      let callCount = 0;

      service.getComments('review-1').subscribe(() => callCount++);
      service.getComments('review-1').subscribe(() => callCount++);

      setTimeout(() => {
        expect(callCount).toBe(2);
        done();
      }, 10);
    });

    it('should handle concurrent requests', (done) => {
      mockApiService.post.and.returnValue(of(void 0));
      mockApiService.put.and.returnValue(of(void 0));

      let postCalled = false;
      let putCalled = false;

      service.createComment('review-1', 'test').subscribe(() => {
        postCalled = true;
      });

      service.updateComment('review-1', 'comment-1', 'test').subscribe(() => {
        putCalled = true;
      });

      setTimeout(() => {
        expect(postCalled).toBe(true);
        expect(putCalled).toBe(true);
        done();
      }, 10);
    });
  });
});
