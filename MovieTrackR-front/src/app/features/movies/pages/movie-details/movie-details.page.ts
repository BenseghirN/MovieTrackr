import { DecimalPipe, Location } from '@angular/common';
import { Component, computed, effect, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Button } from 'primeng/button';
import { Chip } from 'primeng/chip';
import { ProgressSpinner } from 'primeng/progressspinner';
import { TmdbImageService } from '../../../../core/services/tmdb-image.service';
import { MovieService } from '../../services/movie.service';
import { CarouselModule } from 'primeng/carousel';
import { toSignal } from '@angular/core/rxjs-interop';
import { map, switchMap, of, catchError, tap, finalize } from 'rxjs';
import { MovieReviewsComponents } from '../../../reviews/components/movie-reviews/movie-reviews.components';
import { CardModule } from 'primeng/card';
import { SafeUrlPipe } from '../../../../shared/pipes/safe-url.pipe';
import { DialogModule } from 'primeng/dialog';
import { MovieStreamingOffers } from '../../models/streaming-offers.model';
import { AddToListPopoverComponent } from '../../../user-lists/components/add-to-list-popover/add-to-list-popover.component';
import { TooltipModule } from 'primeng/tooltip';
import { Title } from '@angular/platform-browser';

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
    SafeUrlPipe,
    AddToListPopoverComponent,
    TooltipModule
  ],
  templateUrl: './movie-details.page.html',
  styleUrl: './movie-details.page.scss',
})
export class MovieDetailsPage {
  private readonly route = inject(ActivatedRoute);
  private readonly location = inject(Location);
  private readonly router = inject(Router);
  private readonly title = inject(Title);

  private readonly moviesService = inject(MovieService);
  readonly imageService = inject(TmdbImageService);

  readonly movieId = toSignal(
    this.route.paramMap,
    { initialValue: null }
  );
  
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly movie = toSignal(
    this.route.paramMap.pipe(
      map(p => p.get('id')),
      tap(() => {
        this.loading.set(true);  // Mettre ici au début du pipe
        this.error.set(null);
      }),
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
          }),
          finalize(() => this.loading.set(false))
        );
      })
    ),
    { initialValue: null }
  );
  readonly streamingOffers = signal<MovieStreamingOffers | null>(null);
  readonly streamingLoading = signal(false);
  readonly streamingError = signal<string | null>(null);

  readonly posterFlipped = signal(false);

  readonly trailerDialogVisible = signal(false);
  readonly currentTrailerUrl = signal<string>('');

  readonly hasCast = computed(() => !!this.movie()?.cast?.length);
  readonly hasCrew = computed(() => !!this.movie()?.crew?.length);
  readonly director = computed(() => {
    const m = this.movie();
    if (!m) return null;
    return m.crew.find(c => c.job === 'Director') ?? null;
  });
  
  readonly crewByDepartment = computed(() => {
    const movie = this.movie();
    if (!movie?.crew) return {};

    const deptMap: Record<string, string> = {
      'Directing': 'Réalisation',
      'Writing': 'Scénario',
      'Production': 'Production',
      'Camera': 'Photographie',
      'Editing': 'Montage',
      'Sound': 'Musique & Son',
      'Art': 'Direction artistique',
      'Costume & Make-Up': 'Costumes & Maquillage'
    };

    const grouped: Record<string, Array<{ personId: string; name: string; job: string }>> = {};

    movie.crew.forEach(member => {
      const frenchDept = deptMap[member.department!];
      if (!frenchDept) return;

      if (!grouped[frenchDept]) {
        grouped[frenchDept] = [];
      }

      // Éviter les doublons (même personne avec plusieurs jobs dans le même département)
      const exists = grouped[frenchDept].some(m => m.personId === member.personId);
      if (!exists) {
        grouped[frenchDept].push({
          personId: member.personId,
          name: member.name,
          job: member.job
        });
      }
    });

    return grouped;
  });

  getDepartmentKeys(): string[] {
    return Object.keys(this.crewByDepartment());
  }

  readonly carouselResponsiveOptions = [
    { breakpoint: '1400px', numVisible: 6, numScroll: 6 },
    { breakpoint: '1200px', numVisible: 5, numScroll: 5 },
    { breakpoint: '992px', numVisible: 4, numScroll: 4 },
    { breakpoint: '768px', numVisible: 3, numScroll: 3 },
    { breakpoint: '576px', numVisible: 2, numScroll: 2 }
  ];

  constructor() {
    effect(() => {
      const m = this.movie();
      if (!m?.tmdbId){
        this.streamingOffers.set(null);
        this.posterFlipped.set(false);
        return;
      }
      this.title.setTitle(`${m.title} (${m.year}) | MovieTrackR`);
      this.loadStreamingOffers(m.tmdbId);
    });
  }

  formatDuration(minutes: number | null): string {
    if (!minutes) return 'N/A';
    const hours = Math.floor(minutes / 60);
    const mins = minutes % 60;
    return `${hours}h ${mins}min`;
  }

  openTrailer(rawUrl: string): void {
    this.currentTrailerUrl.set(this.buildEmbedUrl(rawUrl));
    this.trailerDialogVisible.set(true);
  }

  onTrailerDialogVisibleChange(visible: boolean): void {
    this.trailerDialogVisible.set(visible);
    if (!visible) {
      this.currentTrailerUrl.set('');
    }
  }

  togglePosterFlip(): void {
    this.posterFlipped.update(v => !v);
  }

  openStreamingLink(url: string | null, event: MouseEvent): void {
    event.stopPropagation();
    if (!url) return;
    window.open(url, '_blank');
  }

  onPersonClick(personId: string): void {
    if (personId) {
      this.router.navigate(['/people', personId]);
    }
  }

  goBack(): void {
    this.location.back()
  }

  formatDate(dateString?: string | null): string {
    if (!dateString) return '';
    const date = new Date(dateString);
    return date.toLocaleDateString('fr-FR', {
      day: 'numeric',
      month: 'long',
      year: 'numeric'
    });
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

  private loadStreamingOffers(tmdbId: number): void {
    this.streamingLoading.set(true);
    this.streamingError.set(null);

    this.moviesService.getStreamingOffers(tmdbId, 'BE').subscribe({
      next: result => {
        this.streamingOffers.set(result);
        this.streamingLoading.set(false);
      },
      error: () => {
        this.streamingError.set('Impossible de charger les offres de streaming.');
        this.streamingLoading.set(false);
        this.streamingOffers.set(null);
      }
    });
  }
}
