import { CommonModule } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TableModule } from 'primeng/table';
import { TooltipModule } from 'primeng/tooltip';
import { NotificationService } from '../../../core/services/notification.service';
import { AdminMoviesService } from '../../services/admin-movies.service';
import { ConfirmationService } from 'primeng/api';
import { MovieForAdministration } from '../../models/movie.model';
import { TmdbImageService } from '../../../core/services/tmdb-image.service';

@Component({
  selector: 'app-admin-movies-table',
  standalone: true,
  imports: [CommonModule, ProgressSpinnerModule, TableModule, ButtonModule, ConfirmDialogModule, FormsModule, TooltipModule, RouterModule],
  templateUrl: './admin-movies-table-component.html',
  styleUrl: './admin-movies-table-component.scss',
})
export class AdminMoviesTableComponent implements OnInit{
  private readonly adminMoviesService = inject(AdminMoviesService);
  private readonly notificationService = inject(NotificationService);
  private readonly confirmationService = inject(ConfirmationService);
  readonly imageService = inject(TmdbImageService);

  readonly movies = signal<MovieForAdministration[]>([]);
  readonly loading = signal<boolean>(false);
  readonly error = signal<string | null>(null);


  ngOnInit(): void {
    this.loadMovies();
  }

  getGenreNames(movie: MovieForAdministration): string {
    return movie.genres?.map(g => g.name).join(', ') || '—';
  }

  onDeleteMovie(event: Event, movie: MovieForAdministration): void {
    this.loading.set(true);
    this.error.set(null);

    this.confirmationService.confirm({
      target: event.target as EventTarget,
      header: 'Confirmation',
      message: `Êtes-vous sûr de vouloir supprimer "${movie.title}" ?`,
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Supprimer',
      rejectLabel: 'Annuler',
      closeOnEscape: true,
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.adminMoviesService.deleteMovie(movie.id).subscribe({
          next: () => {
            this.movies.update(list => list.filter(m => m.id !== movie.id));
            this.loading.set(false);
            this.notificationService.success(`"${movie.title}" supprimé avec succès.`);
          },
          error: () => {
            this.loading.set(false);
            this.error.set(`Impossible de supprimer le film "${movie.title}".`)
            this.notificationService.error(this.error()!);
          }
        });
      },
      reject: () => this.loading.set(false)
    });
  }

  private loadMovies(): void {
    this.loading.set(true);
    this.adminMoviesService.getAllMovies().subscribe({
      next: (movies) => {
        this.movies.set(movies);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.notificationService.error('Impossible de charger la liste des films.');
      }
    });
  }
}
