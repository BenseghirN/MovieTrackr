import { Component, inject, OnInit, signal } from '@angular/core';
import { AdminCommentsService } from '../../services/admin-comments.service';
import { ConfirmationService } from 'primeng/api';
import { NotificationService } from '../../../core/services/notification.service';
import { ReviewComment } from '../../../features/reviews/models/review.model';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { Popover, PopoverModule } from 'primeng/popover';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TableModule } from 'primeng/table';
import { TooltipModule } from 'primeng/tooltip';

@Component({
  selector: 'app-admin-comments-table',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, ProgressSpinnerModule, TableModule, ButtonModule, TooltipModule, PopoverModule],
  templateUrl: './admin-comments-table-component.html',
  styleUrl: './admin-comments-table-component.scss',
})
export class AdminCommentsTableComponent implements OnInit {
  private readonly adminCommentsService = inject(AdminCommentsService);
  private readonly notificationService = inject(NotificationService);
  private readonly confirmationService = inject(ConfirmationService);

  readonly comments = signal<ReviewComment[]>([]);
  readonly loading = signal<boolean>(false);
  readonly error = signal<string | null>(null);

  readonly selectedComment = signal<ReviewComment | null>(null);


  ngOnInit(): void {
    this.loadComments();
  }

  onChangeVisibility(comment: ReviewComment): void {
    this.loading.set(true);
    this.adminCommentsService.changeCommentVisibility(comment.id).subscribe({
      next: () => {
        const newVisi = !(comment.publiclyVisible);
        this.comments.update((comments) => 
          comments.map((c) => c.id === comment.id ? {
            ...c,
            publiclyVisible: newVisi
          } : c)
        );
        this.loading.set(false);
        this.notificationService.success(newVisi
          ? `Commentaire rendu visible avec succès.` 
          : `Commentaire rendu invisible avec succès.`);
      },
      error: () => {
        this.loading.set(false);
        this.notificationService.error(`Impossible de changer la visibilité du commentaire.`);
      }
    });
  }

  truncateContent(content: string, maxLength: number = 60): string {
    if (!content || content.length <= maxLength) return content || '—';
    return content.substring(0, maxLength) + '...';
  }

  showCommentDetails(event: Event, comment: ReviewComment, popover: Popover): void {
    this.selectedComment.set(comment);
    popover.toggle(event);
  }

  private loadComments(): void {
    this.loading.set(true);
    this.error.set(null);

    this.adminCommentsService.getAllComments().subscribe({
      next: (result: ReviewComment[]) => {
        this.comments.set(result);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.error.set('Impossible de charger la liste des commentaires.');
        this.notificationService.error(this.error()!);
      }
    });
  }
}
