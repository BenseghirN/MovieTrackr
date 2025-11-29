import { DecimalPipe } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Button } from 'primeng/button';
import { Chip } from 'primeng/chip';
import { ProgressSpinner } from 'primeng/progressspinner';
import { TmdbImageService } from '../../../../core/services/tmdb-image.service';
import { MovieService } from '../../services/movie.service';
import { CarouselModule } from 'primeng/carousel';
import { toSignal } from '@angular/core/rxjs-interop';
import { map, switchMap, of, catchError, tap } from 'rxjs';
import { MovieReviewsComponents } from '../../../reviews/components/movie-reviews/movie-reviews.components.ts/movie-reviews.components';
import { CardModule } from 'primeng/card';
import { SafeUrlPipe } from '../../../../shared/pipes/safe-url.pipe';
import { DialogModule } from 'primeng/dialog';

@Component({
  selector: 'app-movie-details-page',
  imports: [
    DecimalPipe, 
    Chip, 
    Button, 
    ProgressSpinner, 
    CarouselModule, 
    MovieReviewsComponents, 
    CardModule, 
    DialogModule, 
    SafeUrlPipe
  ],
  templateUrl: './movie-details.page.html',
  styleUrl: './movie-details.page.scss',
})
export class MovieDetailsPage {
  private readonly route = inject(ActivatedRoute);
  private readonly moviesService = inject(MovieService);
  protected readonly imageService = inject(TmdbImageService);

  protected readonly movieId = toSignal(
    this.route.paramMap,
    { initialValue: null }
  );
  
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly movie = toSignal(
    this.route.paramMap.pipe(
      map(p => p.get('id')),
      switchMap(id => {
        if (!id){
          this.loading.set(false);
          this.error.set('ID du film manquant');
          return of(null);
        }  
        
        this.loading.set(true);
        this.error.set(null);

        return this.moviesService.getMovieByRouteId(id).pipe(
          tap(() => this.loading.set(false)),
          catchError(err => {
            console.error('Erreur chargement film', err);
            this.loading.set(false);
            this.error.set('Impossible de charger les informations du film');
            return of(null);
          })
        );
      })
    ),
    { initialValue: null }
  );
  protected trailerDialogVisible = signal(false);
  protected currentTrailerUrl = signal<string>('');

  protected readonly hasCast = computed(() => !!this.movie()?.cast?.length);
  protected readonly hasCrew = computed(() => !!this.movie()?.crew?.length);
  protected readonly director = computed(() => {
    const m = this.movie();
    if (!m) return null;
    return m.crew.find(c => c.job === 'Director')?.name ?? null;
  });

  protected readonly carouselResponsiveOptions = [
    { breakpoint: '1400px', numVisible: 6, numScroll: 3 },
    { breakpoint: '1200px', numVisible: 5, numScroll: 2 },
    { breakpoint: '992px', numVisible: 4, numScroll: 2 },
    { breakpoint: '768px', numVisible: 3, numScroll: 1 },
    { breakpoint: '576px', numVisible: 2, numScroll: 1 }
  ];

  protected formatDuration(minutes: number | null): string {
    if (!minutes) return 'N/A';
    const hours = Math.floor(minutes / 60);
    const mins = minutes % 60;
    return `${hours}h ${mins}min`;
  }

  protected openTrailer(rawUrl: string): void {
    this.currentTrailerUrl.set(this.buildEmbedUrl(rawUrl));
    this.trailerDialogVisible.set(true);
  }

  protected onTrailerDialogVisibleChange(visible: boolean): void {
    this.trailerDialogVisible.set(visible);
    if (!visible) {
      this.currentTrailerUrl.set('');
    }
  }

  private buildEmbedUrl(rawUrl: string): string {
    if (rawUrl.includes('/embed/')) {
      return rawUrl;
    }
    try {
      const url = new URL(rawUrl);
      const key = url.searchParams.get('v');
      if (key) {
        return `https://www.youtube.com/embed/${key}`;
      }
    } catch {
    }
    return rawUrl;
  }
}
