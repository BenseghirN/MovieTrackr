import { DecimalPipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Button } from 'primeng/button';
import { Chip } from 'primeng/chip';
import { ProgressSpinner } from 'primeng/progressspinner';
import { MovieDetails } from '../../models/movie-details.model';
import { TmdbImageService } from '../../../../core/services/tmdb-image.service';
import { MovieService } from '../../services/movie.service';
import { CarouselModule } from 'primeng/carousel';

@Component({
  selector: 'app-movie-details-page',
  standalone: true,
  imports: [DecimalPipe, Chip, Button, ProgressSpinner, CarouselModule],
  templateUrl: './movie-details.page.html',
  styleUrl: './movie-details.page.scss',
})
export class MovieDetailsPage implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly moviesService = inject(MovieService);
  protected readonly imageService = inject(TmdbImageService);

  protected readonly movie = signal<MovieDetails | null>(null);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);

  protected readonly carouselResponsiveOptions = [
    {
      breakpoint: '1400px',
      numVisible: 6,
      numScroll: 3
    },
    {
      breakpoint: '1200px',
      numVisible: 5,
      numScroll: 2
    },
    {
      breakpoint: '992px',
      numVisible: 4,
      numScroll: 2
    },
    {
      breakpoint: '768px',
      numVisible: 3,
      numScroll: 1
    },
    {
      breakpoint: '576px',
      numVisible: 2,
      numScroll: 1
    }
  ];

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.error.set('ID du film manquant');
      this.loading.set(false);
      return;
    }

    this.loadMovieDetails(id);
  }

  private loadMovieDetails(id: string): void {
    this.loading.set(true);
    this.error.set(null);
    const isGuid = id.includes('-');
    
    if (isGuid) {
      this.moviesService['getLocalMovie'](id).subscribe({
        next: (movie) => {
          this.movie.set(movie);
          this.loading.set(false);
        },
        error: (err) => {
          this.error.set(err.message || 'Erreur lors du chargement');
          this.loading.set(false);
        }
      });
    } else {
      this.moviesService['getTmdbMovie'](+id).subscribe({
        next: (movie) => {
          this.movie.set(movie);
          this.loading.set(false);
        },
        error: (err) => {
          this.error.set(err.message || 'Erreur lors du chargement');
          this.loading.set(false);
        }
      });
    }
  }

  protected formatDuration(minutes: number | null): string {
    if (!minutes) return 'N/A';
    const hours = Math.floor(minutes / 60);
    const mins = minutes % 60;
    return `${hours}h ${mins}min`;
  }

  protected getDirector(): string | null {
    const movie = this.movie();
    if (!movie) return null;
    const director = movie.crew.find(c => c.job === 'Director');
    return director?.name ?? null;
  }

}
