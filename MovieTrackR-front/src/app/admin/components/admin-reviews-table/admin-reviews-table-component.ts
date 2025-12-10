import { CommonModule } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TableModule } from 'primeng/table';
import { ToggleSwitchModule } from 'primeng/toggleswitch';
import { TooltipModule } from 'primeng/tooltip';
import { AdminReviewsService } from '../../services/admin-reviews.service';
import { NotificationService } from '../../../core/services/notification.service';
import { ReviewListItem } from '../../../features/reviews/models/review.model';
import { TmdbImageService } from '../../../core/services/tmdb-image.service';
import { RouterModule } from '@angular/router';
import { Popover, PopoverModule } from 'primeng/popover';
import { UserForAdministration } from '../../models/user.model';

@Component({
  selector: 'app-admin-reviews-table',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, ProgressSpinnerModule, TableModule, ButtonModule, ConfirmDialogModule, ToggleSwitchModule, TooltipModule, PopoverModule],
  templateUrl: './admin-reviews-table-component.html',
  styleUrl: './admin-reviews-table-component.scss',
})
export class AdminReviewsTableComponent implements OnInit {
  private readonly adminReviewService = inject(AdminReviewsService);
  private readonly notificationService = inject(NotificationService);
  readonly imageService = inject(TmdbImageService);
  
  readonly reviews = signal<ReviewListItem[]>([]);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  readonly selectedReview = signal<ReviewListItem | null>(null);
  
  ngOnInit(): void {
    this.loadReviews();
  }
  
  private loadReviews(): void {
    this.loading.set(true);
    this.error.set(null);

    this.adminReviewService.getAllReviews().subscribe({
      next: (result: ReviewListItem[]) => {
        this.reviews.set(result);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.error.set('Impossible de charger la liste des reviews.');
        this.notificationService.error(this.error()!);
      }
    });
  }

  showReviewDetails(event: Event, review: ReviewListItem, popover: Popover): void {
    this.selectedReview.set(review);
    popover.toggle(event);
  }

  onChangeVisibility(review: ReviewListItem): void {
    this.loading.set(true);
    this.adminReviewService.changeReviewVisibility(review.id).subscribe({
      next: (updatedReview: ReviewListItem) => {
        this.loading.set(false);
        this.reviews.update((reviews) => 
          reviews.map((r) => r.id === updatedReview.id ? {
            ...r,
            publiclyVisible: updatedReview.publiclyVisible
          } : r)
        );
        this.notificationService.success(updatedReview.publiclyVisible 
          ? `Review rendue visible avec succès.` 
          : `Review rendue invisible avec succès.`);
      },
      error: () => {
        this.loading.set(false);
        this.notificationService.error(`Impossible de changer la visibilité de la review.`);
      }
    });
  }
}
