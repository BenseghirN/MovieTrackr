import { Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { MovieService } from '../../../movies/services/movie.service';
import { TmdbImageService } from '../../../../core/services/tmdb-image.service';
import { PageMeta, SearchMovieResponse, SearchMovieResult } from '../../../movies/models/movie.model';
import { NotificationService } from '../../../../core/services/notification.service';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { CarouselModule } from 'primeng/carousel';
import { SkeletonModule } from 'primeng/skeleton';

@Component({
  selector: 'app-home-page',
  imports: [ButtonModule, CardModule, InputTextModule, ProgressSpinnerModule, CarouselModule, SkeletonModule],
  templateUrl: './home.page.html',
  styleUrl: './home.page.scss',
})
export class HomePageComponent {
  private readonly router = inject(Router);
  private readonly movieService = inject(MovieService);
  private readonly notificationService = inject(NotificationService);
  readonly imageService = inject(TmdbImageService);
  
  private readonly searchQuery = signal('');
  readonly popularMovies = signal<SearchMovieResult[]>([]);
  readonly popularMoviesMeta = signal<PageMeta | null>(null);
  readonly popularCarouselPage = signal(0);

  readonly trendingMovies = signal<SearchMovieResult[]>([]);
 
  readonly loadingPopular = signal(false);
  readonly loadingTrending = signal(false);
  readonly error = signal<string | null>(null);

  readonly carouselResponsiveOptions = [
    { breakpoint: '1400px', numVisible: 5, numScroll: 5 },
    { breakpoint: '1200px', numVisible: 4, numScroll: 4 },
    { breakpoint: '992px', numVisible: 3, numScroll: 3 },
    { breakpoint: '768px', numVisible: 2, numScroll: 2 },
    { breakpoint: '576px', numVisible: 1, numScroll: 1 }
  ];

  constructor() {
    this.loadPopularMovies();
    this.loadTrendingMovies();
  }

  onQueryChange(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.searchQuery.set(value);
  }

  onSearchKeyup(event: KeyboardEvent): void {
    if (event.key === 'Enter' &&  this.searchQuery().trim() !== '') {
      this.redirectToSearch();
    }
  }
  
  onSearch(): void {
    if (this.searchQuery().trim() !== '') {
      this.redirectToSearch();
    }
  }

  goToMovies(): void {
    this.router.navigate(['/movies']);
  }

  onMovieClick(movie: SearchMovieResult): void{
    const id = movie.localId ? movie.localId : movie.tmdbId;

    if (id) {
      this.router.navigate(['/movies', id]);
    }
  }

  scrollToCarousels(): void {
    const carouselsSection = document.getElementById('carousels');
    carouselsSection?.scrollIntoView({ behavior: 'smooth' });
  }

  scrollToFeatures(): void {
    const featuresSection = document.querySelector('.features');
    featuresSection?.scrollIntoView({ behavior: 'smooth' });
  }

  onLoadMore(): void{
    const meta = this.popularMoviesMeta();
    if (!meta || !meta.hasMore || this.loadingPopular()) return;
    const nextPage = meta.page + 1;
    this.loadPopularMovies(nextPage, true);
  }

  onPopularCarouselPageEvent(event: any): void {
    this.popularCarouselPage.set(event.page);
  }

  private redirectToSearch() {
    const query = this.searchQuery().trim();
    this.searchQuery.set('');
    
    this.router.navigate(['/movies'], {
      queryParams: {
        query: query,
        page: 1
      }
    });
  }

  private loadPopularMovies(pageNumber: number = 1, append: boolean = false): void {
    this.loadingPopular.set(true);
    this.error.set(null);

    this.movieService.getPopularMovies({page: pageNumber}).subscribe({
      next: (result: SearchMovieResponse) => {
        if (append) {
          this.popularMovies.update((movies) => [...movies, ...result.items])
          setTimeout(() => {
            this.popularCarouselPage.update(page => page);
          });
        } else 
          this.popularMovies.set(result.items);
          
        this.popularMoviesMeta.set(result.meta);
        this.loadingPopular.set(false);
      },
      error: () => {
        this.loadingPopular.set(false);
        if (!append) this.popularMovies.set([]);
        this.error.set('Impossible de charger les films populaires');
        this.notificationService.error(this.error()!);
      }
    });
  }

  private loadTrendingMovies(): void {
    this.loadingTrending.set(true);
    this.error.set(null);

    this.movieService.getTrendingMovies().subscribe({
      next: (result: SearchMovieResult[]) => {
        this.trendingMovies.set(result);
        this.loadingTrending.set(false);
      },
      error: () => {
        this.loadingTrending.set(false);
        this.trendingMovies.set([]);
        this.error.set('Impossible de charger les films tendance');
        this.notificationService.error(this.error()!);
      }
    });    
  }
}
