import { CommonModule } from '@angular/common';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { DynamicDialogRef, DynamicDialogConfig } from 'primeng/dynamicdialog';
import { TextareaModule } from 'primeng/textarea';
import { ReviewService } from '../../services/reviews.service';
import { CreateReviewModel, ReviewListItem, UpdateReviewModel } from '../../models/review.model';
import { FloatLabelModule } from 'primeng/floatlabel';
import { RatingModule } from 'primeng/rating';
import { MessageModule } from 'primeng/message';
import { toSignal } from '@angular/core/rxjs-interop';
import { map, startWith } from 'rxjs/operators';
import { NotificationService } from '../../../../core/services/notification.service';

interface ReviewFormDialogData {
  movieId: string;
  review?: ReviewListItem;
}

@Component({
  selector: 'app-review-form-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, ButtonModule, TextareaModule, FloatLabelModule, RatingModule, MessageModule],
  templateUrl: './review-form-modal.component.html',
  styleUrl: './review-form-modal.component.scss',
})
export class ReviewFormModalComponent implements OnInit {
  private readonly dialogRef = inject(DynamicDialogRef);
  private readonly dialogConfig = inject(DynamicDialogConfig<ReviewFormDialogData>);
  private readonly reviewService = inject(ReviewService);
  private readonly notificationService = inject(NotificationService);
  
  protected movieId!: string;
  protected existingReview?: ReviewListItem;
  
  private readonly formBuilder = inject(FormBuilder);
  protected readonly reviewForm = this.formBuilder.nonNullable.group({
    rating: this.formBuilder.nonNullable.control<number>(0, {
      validators: [Validators.required, Validators.min(1), Validators.max(5)],
    }),
    content: this.formBuilder.nonNullable.control<string>('', {
      validators: [Validators.required, Validators.minLength(10), Validators.maxLength(2000)],
    }),
  });
  
  protected readonly loading = signal(false);
  protected readonly validationErrors = signal<Partial<Record<string, string[]>>>({});
  protected readonly rating = toSignal(
    this.reviewForm.controls.rating.valueChanges,
    { initialValue: this.reviewForm.controls.rating.value }
  );

  protected readonly content = toSignal(
    this.reviewForm.controls.content.valueChanges,
    { initialValue: this.reviewForm.controls.content.value }
  );

  protected readonly isEditMode = computed(() => !!this.existingReview);
  protected readonly contentLength = computed(() => (this.content() ?? '').length);
  protected readonly canSubmit = computed(() => {
    const r = this.rating() ?? 0;
    const c = (this.content() ?? '').trim();
    return r > 0 && c.length >= 10 && !this.loading();
  });
  
  ngOnInit(): void {
    this.movieId = this.dialogConfig.data?.movieId!;
    this.existingReview = this.dialogConfig.data?.review;
    
    if (!this.movieId) {
      this.dialogRef.close();
      return;
    }
    
    if (this.existingReview) {
      this.reviewForm.patchValue({
        rating: this.existingReview.rating,
        content: this.existingReview.content,
      });
    }
  }

  onSubmit(): void {
    if (!this.canSubmit()) {
      this.reviewForm.markAllAsTouched();
      return;
    }
    this.loading.set(true);
    this.validationErrors.set({});

    const values = this.reviewForm.getRawValue();
    const content = values.content.trim();
    const rating = values.rating;

    const payload = this.isEditMode()
      ? { rating: rating, content: content } as UpdateReviewModel
      : { movieId: this.movieId, rating: rating, content: content } as CreateReviewModel;

    const action = this.isEditMode()
      ? this.reviewService.updateReview(this.existingReview!.id, payload)
      : this.reviewService.createReview(payload as CreateReviewModel);
      
    action.subscribe({
      next: () => {
        this.loading.set(false);       
        this.dialogRef.close(true);
      },
      error: (err) => this.handleError(err)
    });
  }

  onCancel(): void {
    this.dialogRef.close(false);
  }
  
  private handleError(err: any): void {
    this.loading.set(false);
    if (err.status === 400 && err.errors) {
      this.validationErrors.set(err.errors);
      return;
    }

    if (err.status === 400 && !err.errors) {
      this.notificationService.error(
        err.message || `Une erreur est survenue lors de l’envoi de la critique.`
      );
      return;
    }
  }

  protected isInvalid(fieldName: 'rating' | 'content'): boolean {
    const field = this.reviewForm.controls[fieldName];
    return field.invalid && field.touched;
  }
}
