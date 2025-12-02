import { CommonModule } from '@angular/common';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { DynamicDialogConfig, DynamicDialogRef } from 'primeng/dynamicdialog';
import { PaginatorModule, PaginatorState } from 'primeng/paginator';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TextareaModule } from 'primeng/textarea';
import { ReviewCommentsService } from '../../services/review-comments.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { ReviewComment } from '../../models/review.model';
import { AuthService } from '../../../../core/auth/auth-service';
import { toSignal } from '@angular/core/rxjs-interop';
import { ScrollPanelModule } from 'primeng/scrollpanel';


interface CommentsDialogData {
  reviewId: string;
  reviewAuthor: string;
}

@Component({
  selector: 'app-comments-modal',
  standalone: true,
  imports: [
    CommonModule, 
    ReactiveFormsModule, 
    ButtonModule, 
    TextareaModule, 
    ProgressSpinnerModule,
    PaginatorModule,
    ScrollPanelModule
  ],
  templateUrl: './comments-modal.component.html',
  styleUrl: './comments-modal.component.scss',
})
export class CommentsModalComponent implements OnInit {
  private readonly dialogRef = inject(DynamicDialogRef);
  private readonly dialogConfig = inject(DynamicDialogConfig<CommentsDialogData>);
  private readonly commentService = inject(ReviewCommentsService);
  private readonly notificationService = inject(NotificationService);
  private readonly authService = inject(AuthService);

  protected reviewId!: string;
  protected reviewAuthor!: string;

  private readonly formBuilder = inject(FormBuilder);
  protected readonly commentForm = this.formBuilder.nonNullable.group({
    content: this.formBuilder.nonNullable.control<string>('', {
      validators: [Validators.required, Validators.minLength(5), Validators.maxLength(500)]
    })
  });
  protected readonly content = toSignal(
    this.commentForm.controls.content.valueChanges,
    { initialValue: this.commentForm.controls.content.value }
  );

  protected readonly comments = signal<ReviewComment[]>([]);
  protected readonly totalCount = signal(0);
  protected readonly currentPage = signal(1);
  protected readonly pageSize = signal(20);
  protected readonly loading = signal(false);
  protected readonly submitting = signal(false);
  protected readonly editingCommentId = signal<string | null>(null);

  protected readonly currentUser = this.authService.currentUser;
  protected readonly isAuthenticated = this.authService.isAuthenticated;

  protected readonly hasComments = computed(() => this.comments().length > 0);
  protected readonly totalPages = computed(() => Math.ceil(this.totalCount() / this.pageSize() || 1));
  protected readonly contentLength = computed(() => (this.content() ?? '').length);
  
  protected readonly canSubmit = computed(() => {
    const content = (this.content() ?? '').trim();
    return content.length >= 5 && content.length <= 500 && !this.submitting();
  });

  protected login(): void {
    this.authService.login(window.location.pathname);
  }

  ngOnInit(): void {
    this.reviewId = this.dialogConfig.data?.reviewId!;
    this.reviewAuthor = this.dialogConfig.data?.reviewAuthor || 'Unknown';

    if (!this.reviewId) {
      this.dialogRef.close();
      return;
    }
    this.loadComments();
  }

  private loadComments(): void {
    this.loading.set(true);

    this.commentService.getComments(this.reviewId, this.currentPage(), this.pageSize())
      .subscribe({
        next: (result) => {
          this.comments.set(result.items);
          this.totalCount.set(result.totalCount);
          this.loading.set(false);
        },
        error: () => {
          this.loading.set(false);
          this.comments.set([]);
          this.notificationService.error('Impossible de charger les commentaires.');
        }
      });
  }

  protected onPageChange(event: PaginatorState): void {
    this.currentPage.set((event.page ?? 0) + 1);
    this.pageSize.set(event.rows ?? 20);
    this.loadComments();
  }

  protected onSubmit(): void {
    if (!this.canSubmit()) {
      this.commentForm.markAllAsTouched();
      return;
    }

    if (!this.isAuthenticated()) {
      this.notificationService.warning('Vous devez être connecté pour commenter.');
      this.authService.login(window.location.href);
      return;
    }

    this.submitting.set(true);
    const content = this.commentForm.controls.content.value.trim();
    const editingId = this.editingCommentId();

    const action = editingId
      ? this.commentService.updateComment(this.reviewId, editingId, content)
      : this.commentService.createComment(this.reviewId, content);

    action.subscribe({
      next: () => {
        this.submitting.set(false);
        this.commentForm.reset();
        this.editingCommentId.set(null);
        this.notificationService.success(editingId ? 'Commentaire modifié avec succés.' : 'Commentaire ajouté avec succès.');
        this.currentPage.set(1);
        this.loadComments();
      },
      error: () => {
        this.submitting.set(false);
        this.notificationService.error('Impossible de publier le commentaire.');
      }
    });
  }

  protected onEdit(comment: ReviewComment): void {
    this.editingCommentId.set(comment.id);
    this.commentForm.patchValue({ content: comment.content});
  }

  protected onCancelEdit(): void {
    this.editingCommentId.set(null);
    this.commentForm.reset();
  }

  protected onDelete(commentId: string): void {
    if (!confirm('Êtes-vous sûr de vouloir supprimer ce commentaire ?')) return;

    this.commentService.deleteComment(this.reviewId, commentId).subscribe({
      next: () => {
        this.notificationService.success('Commentaire supprimé avec succès.');
        this.loadComments();
      },
      error: () => this.notificationService.error('Impossible de supprimer le commentaire.')
    });
  }

  protected isMyComment(comment: ReviewComment): boolean {
    return this.currentUser()?.id === comment.userId;
  }

  protected onClose(): void {
    this.dialogRef.close(this.totalCount());
  }
}
