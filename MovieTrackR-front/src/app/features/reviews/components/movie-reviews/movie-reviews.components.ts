import { CommonModule } from '@angular/common';
import { Component, computed, effect, inject, Input, input, signal } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { PaginatorModule, PaginatorState } from 'primeng/paginator';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { ReviewService } from '../../services/reviews.service';
import { ReviewLikesService } from '../../services/review-likes.service';
import { AuthService } from '../../../../core/auth/auth-service';
import { NotificationService } from '../../../../core/services/notification.service';
import { DialogService, DynamicDialogRef } from 'primeng/dynamicdialog';
import { ReviewListItem } from '../../models/review.model';
import { ReviewCardComponent } from '../review-card/review-card.component';
import { ReviewFormModalComponent } from '../review-form-modal/review-form-modal.component';
import { CommentsModalComponent } from '../comments-modal/comments-modal.component';

@Component({
  selector: 'app-movie-reviews',
  standalone: true,
  imports: [CommonModule, ButtonModule, PaginatorModule, ProgressSpinnerModule, ReviewCardComponent],
  templateUrl: './movie-reviews.components.html',
  styleUrl: './movie-reviews.components.scss',
})
export class MovieReviewsComponents {
  movieId = input.required<string>();
  averageRating = input<number | null>();

  private readonly reviewService = inject(ReviewService);
  private readonly likesService = inject(ReviewLikesService);
  private readonly authService = inject(AuthService);
  private readonly notificationService = inject(NotificationService);
  private readonly dialogService = inject(DialogService);
  private reviewDialogRef: DynamicDialogRef<ReviewFormModalComponent> | null = null;
  private commentsDialogRef: DynamicDialogRef<CommentsModalComponent> | null = null;
  protected readonly isAuthenticated = this.authService.isAuthenticated;

  protected readonly reviews = signal<ReviewListItem[]>([]);
  protected readonly totalCount = signal(0);
  protected readonly currentPage = signal(1);
  protected readonly reloadKey = signal(0);
  protected readonly pageSize = signal(10);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly hasReviews = computed(() => this.reviews().length > 0);
  protected readonly totalPages = computed(() => Math.ceil(this.totalCount() / this.pageSize() || 1));

  protected login(): void {
    this.authService.login(window.location.pathname);
  }

  constructor() {
    effect(() => {
      const id = this.movieId();
      const page = this.currentPage();
      const size = this.pageSize();
      const reload = this.reloadKey();
      if (!id) return;

      this.loadReviews(id, page, size);
    })
  }

  private loadReviews(movieId: string, page: number, pageSize: number): void {
    this.loading.set(true);
    this.error.set(null);

    this.reviewService.getMovieReviews(movieId, page, pageSize).subscribe({
      next: (result) => {
        this.reviews.set(result.items);
        this.totalCount.set(result.totalCount);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.reviews.set([]);
        this.error.set('Erreur lors de la récupération des critiques');
      }
    });
  }

  protected onPageChange(event: PaginatorState): void {
    this.currentPage.set((event.page ?? 0) + 1);
    this.pageSize.set(event.rows ?? 10);
  }

  private ensureAuthenticated(forAction: string): boolean {
    if (!this.isAuthenticated()) {
      this.notificationService.warning(
        `Vous devez être connecté pour ${forAction}`
      );
      this.authService.login(window.location.pathname);
      return false;
    }
    return true;
  }

  protected onLike(review: ReviewListItem): void {
    if (!this.ensureAuthenticated('liker une critique')) return;
    
    this.updateReviewLikes(review.id);

    const action = review.hasLiked
    ? this.likesService.unlikeReview(review.id)
    : this.likesService.likeReview(review.id);
    
    action.subscribe({
      error: () => {
        this.updateReviewLikes(review.id);
        this.notificationService.error('Impossible de mettre à jour le like');
      }
    });
  }

  protected onWriteReview(): void {
    if (!this.ensureAuthenticated('rédiger une critique')) return;

    this.reviewDialogRef = this.dialogService.open(ReviewFormModalComponent, {
      header: 'Rédiger une critique',
      width: '800px',
      data: { movieId: this.movieId()}
    });

    if (this.reviewDialogRef) {
      this.reviewDialogRef.onClose.subscribe((success: boolean) => {
        if (success) {
          this.notificationService.success('Critique publiée avec succès !');
          this.currentPage.set(1);
          this.reloadKey.update(x => x + 1);
        }
      });
    }
  }

  protected onEdit(review: ReviewListItem): void {
    if (!this.ensureAuthenticated('mettre à jour une critique')) return;

    this.reviewDialogRef = this.dialogService.open(ReviewFormModalComponent, {
      header: 'Modifier ma critique',
      width: '800px',
      data: { 
        movieId: this.movieId(), 
        review: review
      }
    });

    if (this.reviewDialogRef) {
      this.reviewDialogRef.onClose.subscribe((success: boolean) => {
        if (success) {
          this.notificationService.success('Critique modifiée avec succès !');
          this.currentPage.set(1);
          this.reloadKey.update(x => x + 1);
        }
      });
    }
  }

  protected onDelete(reviewId: string): void {
    if (!this.ensureAuthenticated('supprimer une critique')) return;

    if (!confirm('Êtes-vous certain de vouloir supprimer cette critique ?')) return;

    this.reviewService.deleteReview(reviewId).subscribe({
      next: () => {
        this.notificationService.success('Critique supprimée');
        this.currentPage.set(1);
      },
      error: () => {
        this.notificationService.error('Impossible de supprimer la critique');
      }
    });
  }

  protected onComments(review: ReviewListItem): void {
    this.commentsDialogRef = this.dialogService.open(CommentsModalComponent, {
      width: '800px',
      data: { 
        reviewId: review.id,
        reviewAuthor: review.userName
      }
    });

    if (this.commentsDialogRef) {
      this.commentsDialogRef.onClose.subscribe((updatedCommentsCount?: number) => {
        if (updatedCommentsCount !== undefined) {
          this.reviews.update(reviews => 
            reviews.map(r => 
              r.id === review.id
                ? { ...r, commentsCount: updatedCommentsCount }
                : r
            )
          );
          this.reloadKey.update(x => x + 1);
        }
      });
    }
  }

  private updateReviewLikes(reviewId: string): void {
    this.reviews.update(reviews => 
      reviews.map((r) => 
        r.id === reviewId
        ? {
          ...r, 
          hasLiked: !r.hasLiked, 
          likesCount: r.hasLiked 
            ? r.likesCount - 1 
            : r.likesCount + 1
        }
        : r
      )
    );
  }
}
