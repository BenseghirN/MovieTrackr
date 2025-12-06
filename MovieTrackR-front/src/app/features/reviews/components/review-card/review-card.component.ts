import { CommonModule, DatePipe } from '@angular/common';
import { Component, computed, inject, input, output } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { TooltipModule } from 'primeng/tooltip';
import { ReviewListItem } from '../../models/review.model';
import { AuthService } from '../../../../core/auth/auth-service';

@Component({
  selector: 'app-review-card',
  standalone: true,
  imports: [CommonModule, DatePipe, CardModule, ButtonModule, TooltipModule],
  templateUrl: './review-card.component.html',
  styleUrl: './review-card.component.scss',
})
export class ReviewCardComponent {
  readonly review = input.required<ReviewListItem>();

  readonly like = output<void>();
  readonly edit = output<void>();
  readonly delete = output<void>();
  readonly comments = output<void>();

  private readonly authService = inject(AuthService);
  
  readonly isMyReview = computed(() => {
    const currentUser = this.authService.currentUser();
    return currentUser?.id === this.review().userId;
  });

  readonly stars = computed(() => {
    const rating = this.review().rating;
    return {
      full: rating,
      half: 0,
      empty: 5 - rating
    }
  });
  
  onLike(): void {
    this.like.emit();
  }

  onEdit(): void {
    this.edit.emit();
  }

  onDelete(): void {
    this.delete.emit();
  }

  onComments(): void {
    this.comments.emit();
  }
}
