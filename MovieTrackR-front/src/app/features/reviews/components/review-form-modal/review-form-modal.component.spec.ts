import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { DynamicDialogRef, DynamicDialogConfig } from 'primeng/dynamicdialog';
import { ReviewFormModalComponent } from './review-form-modal.component';
import { ReviewService } from '../../services/reviews.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { of, throwError } from 'rxjs';
import { ReviewListItem, CreateReviewModel, UpdateReviewModel } from '../../models/review.model';
import { DebugElement } from '@angular/core';

describe('ReviewFormModalComponent', () => {
  let component: ReviewFormModalComponent;
  let fixture: ComponentFixture<ReviewFormModalComponent>;
  let mockDialogRef: jasmine.SpyObj<DynamicDialogRef>;
  let mockDialogConfig: DynamicDialogConfig<{ movieId: string; review?: ReviewListItem }>;
  let mockReviewService: jasmine.SpyObj<ReviewService>;
  let mockNotificationService: jasmine.SpyObj<NotificationService>;

  const mockReview: ReviewListItem = {
    id: '1',
    movieId: 'movie-123',
    movieTitle: 'Test Movie',
    posterUrl: 'https://example.com/poster.jpg',
    userId: 'user-1',
    userName: 'testuser',
    avatarUrl: 'https://example.com/avatar.jpg',
    rating: 4,
    content: 'Excellent film avec une très bonne histoire',
    likesCount: 5,
    commentsCount: 2,
    createdAt: '2024-01-01T00:00:00Z',
    hasLiked: false,
    publiclyVisible: true
  };

  beforeEach(async () => {
    mockDialogRef = jasmine.createSpyObj('DynamicDialogRef', ['close']);
    mockReviewService = jasmine.createSpyObj('ReviewService', ['createReview', 'updateReview']);
    mockNotificationService = jasmine.createSpyObj('NotificationService', ['error', 'success']);

    mockDialogConfig = {
      data: {
        movieId: 'movie-123',
        review: undefined,
      },
    };

    await TestBed.configureTestingModule({
      imports: [ReviewFormModalComponent, ReactiveFormsModule],
      providers: [
        { provide: DynamicDialogRef, useValue: mockDialogRef },
        { provide: DynamicDialogConfig, useValue: mockDialogConfig },
        { provide: ReviewService, useValue: mockReviewService },
        { provide: NotificationService, useValue: mockNotificationService },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ReviewFormModalComponent);
    component = fixture.componentInstance;
  });

  describe('Component Initialization', () => {
    it('should create', () => {
      expect(component).toBeTruthy();
    });

    it('should initialize with empty form for new review', () => {
      fixture.detectChanges();

      expect(component.movieId).toBe('movie-123');
      expect(component.existingReview()).toBeNull();
      expect(component.reviewForm.get('rating')?.value).toBe(0);
      expect(component.reviewForm.get('content')?.value).toBe('');
    });

    it('should initialize with existing review data in edit mode', () => {
      mockDialogConfig.data!.review = mockReview;
      fixture.detectChanges();

      expect(component.existingReview()).toEqual(mockReview);
      expect(component.isEditMode()).toBe(true);
      expect(component.reviewForm.get('rating')?.value).toBe(4);
      expect(component.reviewForm.get('content')?.value).toBe(
        'Excellent film avec une très bonne histoire'
      );
    });

    it('should close dialog when movieId is missing', () => {
      mockDialogConfig.data!.movieId = '';
      fixture.detectChanges();

      expect(mockDialogRef.close).toHaveBeenCalled();
    });
  });

  describe('Form Validation', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should have invalid form when rating is 0', () => {
      component.reviewForm.patchValue({
        rating: 0,
        content: 'This is a valid review with enough characters',
      });

      expect(component.reviewForm.get('rating')?.invalid).toBe(true);
    });

    it('should have invalid form when content is empty', () => {
      component.reviewForm.patchValue({
        rating: 5,
        content: '',
      });

      expect(component.reviewForm.get('content')?.invalid).toBe(true);
    });

    it('should have invalid form when content is less than 10 characters', () => {
      component.reviewForm.patchValue({
        rating: 5,
        content: 'Short',
      });
      component.contentLength.set(5);

      expect(component.canSubmit()).toBe(false);
    });

    it('should have invalid form when content exceeds 2000 characters', () => {
      const longContent = 'a'.repeat(2001);
      component.reviewForm.patchValue({
        rating: 5,
        content: longContent,
      });
      component.contentLength.set(2001);

      expect(component.canSubmit()).toBe(false);
    });

    it('should allow submit with valid rating and content', () => {
      component.reviewForm.patchValue({
        rating: 5,
        content: 'This is a valid review with enough characters',
      });
      component.contentLength.set(46);

      expect(component.canSubmit()).toBe(true);
    });

    it('should mark all fields as touched on invalid submit', () => {
      spyOn(component.reviewForm, 'markAllAsTouched');
      component.reviewForm.patchValue({
        rating: 0,
        content: '',
      });

      component.onSubmit();

      expect(component.reviewForm.markAllAsTouched).toHaveBeenCalled();
    });
  });

  describe('Form Interaction', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should update rating value when changed', () => {
      component.reviewForm.patchValue({ rating: 3 });

      expect(component.rating()).toBe(3);
    });

    it('should update content value when changed', () => {
      const testContent = 'This is test content for a review';
      component.reviewForm.patchValue({ content: testContent });

      expect(component.content()).toBe(testContent);
    });

    it('should update contentLength on editor text change', () => {
      const event = {
        textValue: 'This is a test review with enough characters',
      };

      component.onEditorTextChange(event as any);

      expect(component.contentLength()).toBe(
        'This is a test review with enough characters'.length
      );
    });

    it('should handle editor text change with trimming', () => {
      const event = {
        textValue: '  spaced content  ',
      };

      component.onEditorTextChange(event as any);

      expect(component.contentLength()).toBe('spaced content'.length);
    });

    it('should set contentLength to 0 when text is empty', () => {
      const event = {
        textValue: '',
      };

      component.onEditorTextChange(event as any);

      expect(component.contentLength()).toBe(0);
    });
  });

  describe('Review Creation', () => {
    beforeEach(() => {
      fixture.detectChanges();
      mockReviewService.createReview.and.returnValue(of(void 0));
    });

    it('should create a new review when form is valid', () => {
      component.reviewForm.patchValue({
        rating: 4,
        content: 'This is a valid review with enough characters',
      });
      component.contentLength.set(46);

      component.onSubmit();

      expect(mockReviewService.createReview).toHaveBeenCalledWith(
        jasmine.objectContaining({
          movieId: 'movie-123',
          rating: 4,
          content: 'This is a valid review with enough characters',
        })
      );
    });

    it('should trim content before sending to API', () => {
      component.reviewForm.patchValue({
        rating: 5,
        content: '  Content with spaces  ',
      });
      component.contentLength.set(19);

      component.onSubmit();

      expect(mockReviewService.createReview).toHaveBeenCalledWith(
        jasmine.objectContaining({
          content: 'Content with spaces',
        })
      );
    });

    it('should close dialog with success after creating review', (done) => {
      component.reviewForm.patchValue({
        rating: 5,
        content: 'This is a valid review with enough characters',
      });
      component.contentLength.set(46);

      component.onSubmit();

      setTimeout(() => {
        expect(mockDialogRef.close).toHaveBeenCalledWith(true);
        done();
      });
    });

    it('should set loading to false after successful creation', () => {
      component.reviewForm.patchValue({
        rating: 5,
        content: 'This is a valid review with enough characters',
      });
      component.contentLength.set(46);

      expect(component.loading()).toBe(false);
      component.onSubmit();

      expect(mockReviewService.createReview).toHaveBeenCalled();
      expect(component.loading()).toBe(false);
    });

    it('should clear validation errors before submitting', () => {
      component.validationErrors.set({ Content: ['Error 1'] });
      component.reviewForm.patchValue({
        rating: 5,
        content: 'This is a valid review with enough characters',
      });
      component.contentLength.set(46);

      component.onSubmit();

      expect(component.validationErrors()).toEqual({});
    });
  });

  describe('Review Update', () => {
    beforeEach(() => {
      mockDialogConfig.data!.review = mockReview;
      fixture.detectChanges();
      mockReviewService.updateReview.and.returnValue(of(void 0));
    });

    it('should update existing review when in edit mode', () => {
      component.reviewForm.patchValue({
        rating: 5,
        content: 'Updated review content with more characters',
      });
      component.contentLength.set(43);

      component.onSubmit();

      expect(mockReviewService.updateReview).toHaveBeenCalledWith(
        '1',
        jasmine.objectContaining({
          rating: 5,
          content: 'Updated review content with more characters',
        })
      );
    });

    it('should not send movieId when updating', () => {
      component.reviewForm.patchValue({
        rating: 5,
        content: 'Updated review content with more characters',
      });
      component.contentLength.set(43);

      component.onSubmit();

      expect(mockReviewService.updateReview).toHaveBeenCalled();
      expect(mockReviewService.createReview).not.toHaveBeenCalled();
    });

    it('should close dialog with success after updating review', (done) => {
      component.reviewForm.patchValue({
        rating: 5,
        content: 'Updated review content with more characters',
      });
      component.contentLength.set(43);

      component.onSubmit();

      setTimeout(() => {
        expect(mockDialogRef.close).toHaveBeenCalledWith(true);
        done();
      });
    });
  });

  describe('Error Handling', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should handle validation errors from API (400 with errors)', () => {
      const validationError = {
        status: 400,
        errors: {
          Content: ['Content must be unique', 'Invalid format'],
        },
      };
      mockReviewService.createReview.and.returnValue(throwError(() => validationError));

      component.reviewForm.patchValue({
        rating: 5,
        content: 'This is a valid review with enough characters',
      });
      component.contentLength.set(46);

      component.onSubmit();

      expect(component.validationErrors()).toEqual({
        Content: ['Content must be unique', 'Invalid format'],
      });
      expect(component.loading()).toBe(false);
    });

    it('should handle generic errors (400 without errors)', () => {
      const genericError = {
        status: 400,
        message: 'Bad request error',
      };
      mockReviewService.createReview.and.returnValue(throwError(() => genericError));

      component.reviewForm.patchValue({
        rating: 5,
        content: 'This is a valid review with enough characters',
      });
      component.contentLength.set(46);

      component.onSubmit();

      expect(mockNotificationService.error).toHaveBeenCalledWith('Bad request error');
      expect(component.loading()).toBe(false);
    });

    it('should show default error message when no message provided', () => {
      const errorWithoutMessage = {
        status: 400,
      };
      mockReviewService.createReview.and.returnValue(
        throwError(() => errorWithoutMessage)
      );

      component.reviewForm.patchValue({
        rating: 5,
        content: 'This is a valid review with enough characters',
      });
      component.contentLength.set(46);

      component.onSubmit();

      expect(mockNotificationService.error).toHaveBeenCalledWith(
        `Une erreur est survenue lors de l'envoi de la critique.`
      );
    });

    it('should set loading to false on error', () => {
      mockReviewService.createReview.and.returnValue(
        throwError(() => ({ status: 500 }))
      );

      component.reviewForm.patchValue({
        rating: 5,
        content: 'This is a valid review with enough characters',
      });
      component.contentLength.set(46);

      component.onSubmit();

      expect(component.loading()).toBe(false);
    });
  });

  describe('Field Validation States', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should return true when field is invalid and touched', () => {
      const ratingControl = component.reviewForm.get('rating');
      ratingControl?.markAsTouched();
      ratingControl?.patchValue(0);

      expect(component.isInvalid('rating')).toBe(true);
    });

    it('should return false when field is valid', () => {
      const ratingControl = component.reviewForm.get('rating');
      ratingControl?.patchValue(5);

      expect(component.isInvalid('rating')).toBe(false);
    });

    it('should return false when field is invalid but not touched', () => {
      const ratingControl = component.reviewForm.get('rating');
      ratingControl?.patchValue(0);

      expect(component.isInvalid('rating')).toBe(false);
    });
  });

  describe('Dialog Actions', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should close dialog with false on cancel', () => {
      component.onCancel();

      expect(mockDialogRef.close).toHaveBeenCalledWith(false);
    });

    it('should not disable submit button when form is valid and not loading', () => {
      component.reviewForm.patchValue({
        rating: 5,
        content: 'This is a valid review with enough characters',
      });
      component.contentLength.set(46);

      expect(component.canSubmit()).toBe(true);
    });

    it('should disable submit button when loading', () => {
      component.loading.set(true);

      expect(component.canSubmit()).toBe(false);
    });
  });

  describe('Edit Mode Indicator', () => {
    it('should show edit mode as false for new review', () => {
      fixture.detectChanges();

      expect(component.isEditMode()).toBe(false);
    });

    it('should show edit mode as true when review exists', () => {
      mockDialogConfig.data!.review = mockReview;
      fixture.detectChanges();

      expect(component.isEditMode()).toBe(true);
    });
  });

  describe('Editor Initialization', () => {
    beforeEach(() => {
      mockDialogConfig.data!.review = mockReview;
      fixture.detectChanges();
    });

    it('should sync content length from editor on init when in edit mode', () => {
      spyOn<any>(component, 'syncContentLengthFromEditor');
      component.onEditorInit({});

      expect((component as any).syncContentLengthFromEditor).toHaveBeenCalled();
    });

    it('should not sync content length from editor on init when creating new review', () => {
      mockDialogConfig.data!.review = undefined;
      const newFixture = TestBed.createComponent(ReviewFormModalComponent);
      const newComponent = newFixture.componentInstance;
      newFixture.detectChanges();

      spyOn<any>(newComponent, 'syncContentLengthFromEditor');
      newComponent.onEditorInit({});

      expect((newComponent as any).syncContentLengthFromEditor).not.toHaveBeenCalled();
    });
  });
});
