import { CommonModule } from '@angular/common';
import { Component, computed, inject, input, output, signal } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { OrderListModule } from 'primeng/orderlist';
import { Popover, PopoverModule } from 'primeng/popover';
import { UserListMovie } from '../../models/user-list.model';
import { UserListService } from '../../services/user-list.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { TmdbImageService } from '../../../../core/services/tmdb-image.service';

@Component({
  selector: 'app-reorder-movies-popover',
  standalone: true,
  imports: [CommonModule, ButtonModule, PopoverModule, OrderListModule],
  templateUrl: './reorder-movies-popover.component.html',
  styleUrl: './reorder-movies-popover.component.scss',
})
export class ReorderMoviesPopoverComponent {
  readonly listId = input.required<string>();
  readonly movies = input.required<UserListMovie[]>();
  readonly reordered = output<void>();

  private readonly listService = inject(UserListService);
  private readonly notificationService = inject(NotificationService);
  readonly imageService = inject(TmdbImageService);

  readonly localMovies = signal<UserListMovie[]>([]);
  readonly saving = signal(false);

  readonly hasChanges = computed(() => {
    const original = this.movies()
    const current = this.localMovies();
    const changed = current.some((movie, index) => movie.movieId !== original[index]?.movieId);
    return changed;
  });

  toggle(event: Event, popover: Popover): void {
    this.localMovies.set([...this.movies()]);
    popover.toggle(event);
  }

  onReorder(event: any): void {
    const current = this.localMovies();
    this.localMovies.set([...current]);
  }

  onSave(popover: Popover): void {
    if (!this.hasChanges()) {
      popover.hide();
      return;
    }

    this.saving.set(true);
    const reorderedList = this.localMovies();

    const updates = reorderedList.map((movie, index) => ({
      movieId: movie.movieId,
      newPosition: (index + 1) * 10
    }));

    this.updatePosition(updates, 0, popover);
  }

  onCancel(popover: Popover): void {
    popover.hide();
  }
  
  private updatePosition(
    updates: Array<{ movieId: string; newPosition: number; }>, index: number, popover: Popover): void {
    if (index >= updates.length) {
      this.saving.set(false);
      this.notificationService.success('Ordre mis à jour.');
      this.reordered.emit();
      popover.hide();
      return;
    }

    const update = updates[index];

    this.listService.reorderMovieInList(this.listId(), update).subscribe({
      next: () => this.updatePosition(updates, index + 1, popover),
      error: () => {
        this.saving.set(false);
        this.notificationService.error('Erreur lors de la mise à jour');
      }
    });
  }
}
