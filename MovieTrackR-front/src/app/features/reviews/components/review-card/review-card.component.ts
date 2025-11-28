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
  review = input.required<ReviewListItem>();

  like = output<void>();
  edit = output<void>();
  delete = output<void>();
  comments = output<void>();

  private readonly authService = inject(AuthService);
  
  protected readonly isMyReview = computed(() => {
    const currentUser = this.authService.currentUser();
    return currentUser?.id === this.review().userId;
  });
  protected readonly stars = computed(() => {
    const rating = this.review().rating;
    return {
      full: Math.floor(rating / 2),
      half: rating % 2 >= 1,
      empty: 5 - Math.ceil(rating / 2)
    }
  });
  
  protected onLike(): void {
    this.like.emit();
  }

  protected onEdit(): void {
    this.edit.emit();
  }

  protected onDelete(): void {
    this.delete.emit();
  }

  protected onComments(): void {
    this.comments.emit();
  }
}
