import { CommonModule } from '@angular/common';
import { Component, computed, effect, inject, input, signal } from '@angular/core';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { ReviewService } from '../../../reviews/services/reviews.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { PaginatorModule, PaginatorState } from 'primeng/paginator';
import { ReviewCardComponent } from '../../../reviews/components/review-card/review-card.component';
import { AuthService } from '../../../../core/auth/auth-service';
import { ReviewListItem, UserReviewSortOption, UserReviewsQueryParams } from '../../../reviews/models/review.model';
import { SelectModule } from 'primeng/select';
import { FormsModule } from '@angular/forms';
import { FloatLabelModule } from 'primeng/floatlabel';

@Component({
  selector: 'app-user-profile-reviews-section',
  standalone: true,
  imports: [CommonModule, ProgressSpinnerModule, PaginatorModule, SelectModule, ReviewCardComponent, FormsModule, FloatLabelModule],
  templateUrl: './user-profile-reviews-section.component.html',
  styleUrl: './user-profile-reviews-section.component.scss',
})
export class UserProfileReviewsSectionComponent {
  readonly userId = input.required<string | undefined>();

  private readonly reviewService = inject(ReviewService);
  private readonly notificationService = inject(NotificationService);

  
  readonly reviews = signal<ReviewListItem[]>([]);
  readonly totalCount = signal(0);
  readonly currentPage = signal(1);
  readonly pageSize = signal(10);
  readonly sort = signal<UserReviewSortOption>('Newest');
  readonly ratingFilter = signal<number | null>(null);
  readonly reloadKey = signal(0);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  
  readonly hasReviews = computed(() => this.reviews().length > 0);
  readonly totalPages = computed(() => Math.ceil(this.totalCount() / this.pageSize() || 1));
  
  readonly sortOptions = [
    { label: 'Plus récents', value: 'Newest' as UserReviewSortOption },
    { label: 'Plus anciens', value: 'Oldest' as UserReviewSortOption },
    { label: 'Mieux notés', value: 'HighestRating' as UserReviewSortOption },
    { label: 'Moins bien notés', value: 'LowestRating' as UserReviewSortOption },
  ];

  readonly ratingOptions = [
    { label: 'Toutes les notes', value: null },
    { label: '5 ★', value: 5 },
    { label: '4 ★', value: 4 },
    { label: '3 ★', value: 3 },
    { label: '2 ★', value: 2 },
    { label: '1 ★', value: 1 },
    { label: '0 ★', value: 0 },
  ];

  constructor() {
    effect(() => {
      const id = this.userId();
      const reload = this.reloadKey();
      if (!id) return;

      const params: UserReviewsQueryParams = {
        page: this.currentPage(),
        pageSize: this.pageSize(),
        sort: this.sort(),
        ratingFilter: this.ratingFilter()
      };

      this.loadReviews(id, params);
    });
  }

  onPageChange(event: PaginatorState): void {
    this.currentPage.set((event.page ?? 0) + 1);
    this.pageSize.set(event.rows ?? 10);
  }

  onSortChange(sort: UserReviewSortOption): void {
    this.currentPage.set(1);
    this.sort.set(sort);
  }

  onRatingFilterChange(rating: number | null): void {
    this.currentPage.set(1);
    this.ratingFilter.set(rating);
  }

  onEdit(): void {
    this.currentPage.set(1);
    this.reloadKey.update(x => x + 1);
  }

  onDelete(): void {
    this.currentPage.set(1);
    this.reloadKey.update(x => x + 1);
  }  

  private loadReviews(userId: string, params: UserReviewsQueryParams): void {
    this.loading.set(true);
    this.error.set(null);

    this.reviewService.getUserReviews(userId, params).subscribe({
      next: (result) => {
        this.reviews.set(result.items);
        this.totalCount.set(result.totalCount);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.reviews.set([]);
        this.error.set('Erreur lors de la récupération des critiques');
        this.notificationService.error("Impossible de charger les critiques de l'utilisateur.");
      }
    });
  }
}
