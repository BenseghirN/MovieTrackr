import { CommonModule } from '@angular/common';
import { Component, computed, inject, OnInit, signal, viewChild } from '@angular/core';
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
import { NotificationService } from '../../../../core/services/notification.service';
import { Editor, EditorModule, EditorTextChangeEvent } from 'primeng/editor';

interface ReviewFormDialogData {
  movieId: string;
  review?: ReviewListItem;
}

@Component({
  selector: 'app-review-form-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, ButtonModule, TextareaModule, FloatLabelModule, RatingModule, MessageModule, EditorModule],
  templateUrl: './review-form-modal.component.html',
  styleUrl: './review-form-modal.component.scss',
})
export class ReviewFormModalComponent implements OnInit {
  private readonly dialogRef = inject(DynamicDialogRef);
  private readonly dialogConfig = inject(DynamicDialogConfig<ReviewFormDialogData>);
  private readonly reviewService = inject(ReviewService);
  private readonly notificationService = inject(NotificationService);
  
  movieId!: string;
  readonly existingReview = signal<ReviewListItem | null>(null);
  readonly reviewEditor = viewChild<Editor>('reviewEditor');
  
  private readonly formBuilder = inject(FormBuilder);
  readonly reviewForm = this.formBuilder.nonNullable.group({
    rating: this.formBuilder.nonNullable.control<number>(0, {
      validators: [Validators.required, Validators.min(1), Validators.max(5)],
    }),
    content: this.formBuilder.nonNullable.control<string>('', {
      validators: [Validators.required],
    }),
  });
  
  readonly loading = signal(false);
  readonly validationErrors = signal<Partial<Record<string, string[]>>>({});
  readonly rating = toSignal(
    this.reviewForm.controls.rating.valueChanges,
    { initialValue: this.reviewForm.controls.rating.value }
  );

  readonly content = toSignal(
    this.reviewForm.controls.content.valueChanges,
    { initialValue: this.reviewForm.controls.content.value }
  );

  readonly isEditMode = computed(() => !!this.existingReview());
  readonly contentLength = signal(0);
  readonly contentHTMLLength = signal(0);
  readonly canSubmit = computed(() => {
    const r = this.rating() ?? 0;
    const c = (this.content() ?? '').trim();
    return r > 0 && c.length >= 10 && c.length <= 2000 && !this.loading();
  });
  
  ngOnInit(): void {
    this.movieId = this.dialogConfig.data?.movieId!;
    const review = this.dialogConfig.data?.review ?? null;
    this.existingReview.set(review);
    
    if (!this.movieId) {
      this.dialogRef.close();
      return;
    }
    
    if (review) {
      this.reviewForm.patchValue({
        rating: review.rating,
        content: review.content,
      })} else {
      this.reviewForm.reset({ rating: 0, content: '' });
      this.contentLength.set(0);
    }
  }

  onEditorTextChange(event: EditorTextChangeEvent): void {
    const textLength = (event.textValue ?? '').trim().length;
    this.contentLength.set(textLength);

    if (textLength > 2000) {
      this.isInvalid('content');
    }
  }

  onEditorInit(event: unknown): void {
    // Quill est maintenant initialisé et le contenu patché
    if (this.existingReview()) {
      this.syncContentLengthFromEditor();
    }
  }

  onSubmit(): void {
    if (!this.canSubmit() 
        || this.reviewForm.invalid 
        || this.contentLength() < 10 
        || this.contentLength() > 2000
    ) {
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
      ? this.reviewService.updateReview(this.existingReview()!.id, payload)
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

  isInvalid(fieldName: 'rating' | 'content'): boolean {
    const field = this.reviewForm.controls[fieldName];
    return field.invalid && field.touched;
  }
  
  private handleError(err: any): void {
    this.loading.set(false);
    if (err.status === 400 && err.errors) {
      this.validationErrors.set(err.errors);
      return;
    }

    if (err.status === 400 && !err.errors) {
      this.notificationService.error(
        err.message || `Une erreur est survenue lors de l'envoi de la critique.`
      );
      return;
    }
  }

  private syncContentLengthFromEditor(): void {
    const editor = this.reviewEditor();
    if (!editor) return;

    const quill = editor.getQuill();
    if (!quill) return;

    const text = (quill.getText() ?? '').trim();
    this.contentLength.set(text.length);
  }
}
