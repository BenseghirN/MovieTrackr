import { CommonModule, DatePipe } from '@angular/common';
import { Component, computed, effect, inject, input, output, signal } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { TooltipModule } from 'primeng/tooltip';
import { ReviewListItem } from '../../models/review.model';
import { AuthService } from '../../../../core/auth/auth-service';
import { Router } from '@angular/router';
import { TmdbImageService } from '../../../../core/services/tmdb-image.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { DialogService, DynamicDialogRef } from 'primeng/dynamicdialog';
import { CommentsModalComponent } from '../comments-modal/comments-modal.component';
import { ReviewFormModalComponent } from '../review-form-modal/review-form-modal.component';
import { ReviewService } from '../../services/reviews.service';
import { ReviewLikesService } from '../../services/review-likes.service';

@Component({
  selector: 'app-review-card',
  standalone: true,
  imports: [CommonModule, DatePipe, ButtonModule, TooltipModule],
  templateUrl: './review-card.component.html',
  styleUrl: './review-card.component.scss',
})
export class ReviewCardComponent {
  readonly review = input.required<ReviewListItem>();
  readonly reviewState = signal<ReviewListItem | null>(null);
  readonly edit = output<void>();
  readonly delete = output<void>();
  
  private readonly authService = inject(AuthService);
  private readonly notificationService = inject(NotificationService);
  private readonly dialogService = inject(DialogService);
  private readonly reviewService = inject(ReviewService);
  private readonly likesService = inject(ReviewLikesService);

  
  readonly router = inject(Router);
  readonly imageService = inject(TmdbImageService);
  
  private reviewDialogRef: DynamicDialogRef<ReviewFormModalComponent> | null = null;
  private commentsDialogRef: DynamicDialogRef<CommentsModalComponent> | null = null;
  readonly isAuthenticated = this.authService.isAuthenticated;



  readonly isMyReview = computed(() => {
    const currentUser = this.authService.currentUser();
    const r = this.reviewState();
    return !!r && currentUser?.id === r.userId;
  });

  readonly stars = computed(() => {
    const r = this.reviewState();
    const rating = r?.rating ?? 0;
    return {
      full: rating,
      half: 0,
      empty: 5 - rating
    }
  });

  constructor() {
    effect(() => {
      this.reviewState.set(this.review());
    });
    
  }
  
  onLike(): void {
    if (!this.ensureAuthenticated('liker une critique')) return;

    const current = this.reviewState();
    if (!current) return;
    
    this.toggleLikeState();

    const action = this.review().hasLiked
    ? this.likesService.unlikeReview(current.id)
    : this.likesService.likeReview(current.id);
    
    action.subscribe({
      error: () => {
        this.toggleLikeState();
        this.notificationService.error('Impossible de mettre à jour le like');
      }
    });
  }

  onEdit(event: Event): void {
    event.stopPropagation();
    if (!this.ensureAuthenticated('mettre à jour une critique')) return;

    const current = this.reviewState();
    if (!current) return;

    this.reviewDialogRef = this.dialogService.open(ReviewFormModalComponent, {
      header: 'Modifier ma critique',
      width: '800px',
      data: { 
        movieId: current.movieId, 
        review: current
      }
    });

    if (this.reviewDialogRef) {
      this.reviewDialogRef.onClose.subscribe((success: boolean) => {
        if (success) {
          this.notificationService.success('Critique modifiée avec succès !');
          this.edit.emit();
        }
      });
    }
  }

  onDelete(event: Event): void {
    event.stopPropagation();
    if (!this.ensureAuthenticated('supprimer une critique')) return;

    const current = this.reviewState();
    if (!current) return;

    if (!confirm('Êtes-vous certain de vouloir supprimer cette critique ?')) return;

    this.reviewService.deleteReview(current.id).subscribe({
      next: () => {
        this.notificationService.success('Critique supprimée');
        this.delete.emit();      
      },
      error: () => {
        this.notificationService.error('Impossible de supprimer la critique');
      }
    });    
  }

  onComments(event: Event): void {
    event.stopPropagation();

    const current = this.reviewState();
    if (!current) return;

    this.commentsDialogRef = this.dialogService.open(CommentsModalComponent, {
      width: '800px',
      data: { 
        reviewId: current.id,
        reviewAuthor: current.userName
      }
    });

    if (this.commentsDialogRef) {
      this.commentsDialogRef.onClose.subscribe((updatedCommentsCount?: number) => {
        if (updatedCommentsCount === undefined) return;
        const updated: ReviewListItem = {
          ...current,
          commentsCount: updatedCommentsCount
        };
        
        this.reviewState.set(updated);
      });
    }    
  }
    
  onAuthorClick(): void {
    const id = this.review().userId;
    this.router.navigate(['profiles', id]);
  }

  onMovieTitleClick(): void {
    const movieId = this.review().movieId;
    this.router.navigate(['movies/', movieId]);
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

  private toggleLikeState(): void {
    const current = this.reviewState();
    if (!current) return;

    this.reviewState.set({
      ...current,
      hasLiked: !current.hasLiked, 
      likesCount: current.hasLiked 
            ? current.likesCount - 1 
            : current.likesCount + 1
    });
  }
}
