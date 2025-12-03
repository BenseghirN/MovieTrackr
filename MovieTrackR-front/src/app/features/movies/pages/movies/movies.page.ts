import { Component, computed, effect, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { PaginatorModule, PaginatorState } from 'primeng/paginator';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { MovieService } from '../../services/movie.service';
import { TmdbImageService } from '../../../../core/services/tmdb-image.service';
import { MovieSearchResponse, MovieSearchResult } from '../../models/movie.model';
import { InputNumberModule } from 'primeng/inputnumber';
import { MovieCardComponent } from '../../components/movie-card/movie-card.component';
import { toSignal } from '@angular/core/rxjs-interop';

interface MovieSearchParams {
  query: string;
  page: number;
  year?: number | null;
}

@Component({
  selector: 'app-movies-page',
  standalone: true,
  imports: [
    FormsModule, 
    CardModule, 
    ButtonModule, 
    InputTextModule, 
    InputNumberModule, 
    PaginatorModule, 
    ProgressSpinnerModule,
    MovieCardComponent
  ],
  templateUrl: './movies.page.html',
  styleUrl: './movies.page.scss',
})
export class MoviesPage {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly moviesService = inject(MovieService);
  protected readonly imageService = inject(TmdbImageService);

  protected readonly searchQuery = signal('');
  protected readonly yearFilter = signal<number | null>(null);

  protected readonly movies = signal<MovieSearchResult[]>([]);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly currentPage = signal(1);
  protected readonly pageSize = signal(20);
  protected readonly totalResults = signal(0);

  public readonly hasResults = computed(() => this.movies().length > 0);
  public readonly showNoResults = computed(() => 
    !this.loading() && this.searchQuery() && !this.hasResults()
  );

  private readonly queryParamsSignal = toSignal(
    this.route.queryParamMap,
    { initialValue: this.route.snapshot.queryParamMap },
  );


  constructor() {
    effect(() => {
      const params = this.queryParamsSignal();
      if (!params) return;

      const queryParam = params.get('query') ?? '';
      const yearParam = params.get('year');
      const pageParam = params.get('page');

      const page = pageParam ? +pageParam : 1;
      const year = yearParam ? +yearParam : null;

      this.searchQuery.set(queryParam);
      this.yearFilter.set(Number.isNaN(year!) ? null : year);
      this.currentPage.set(Number.isNaN(page) || page < 1 ? 1 : page);

      if (!queryParam.trim()) {
        this.movies.set([]);
        this.totalResults.set(0);
        return;
      }

      this.fetchMovies(queryParam, page);
    });
  }

  public search(): void {
    if (!this.searchQuery().trim()) return;
    this.currentPage.set(1);

    this.router.navigate([], { 
      relativeTo: this.route,
      queryParams: this.buildQueryParams(), 
      queryParamsHandling: 'merge' 
    });
  }

  public onPageChange(event: PaginatorState): void {
    const newPage = (event.page ?? 0) + 1;
    this.currentPage.set(newPage);
    this.pageSize.set(event.rows ?? 20);
    if (!this.searchQuery().trim()) return;

    this.router.navigate([], { 
      relativeTo: this.route,
      queryParams: this.buildQueryParams(newPage), 
      queryParamsHandling: 'merge' 
    });
  }

  private fetchMovies(query: string, page: number): void {
    this.loading.set(true);
    this.error.set(null);

    this.moviesService
      .search({
        query,
        year: this.yearFilter() ?? undefined,
        page,
        pageSize: this.pageSize(),
      })
      .subscribe({
        next: (result: MovieSearchResponse) => {
          this.movies.set(result.items);
          this.totalResults.set(result.meta.totalResults);
          this.loading.set(false);
        },
        error: (err) => {
          this.loading.set(false);
          this.movies.set([]);
          this.error.set('Impossible de charger les films');
          console.log(err);
        },
      });
  }

  public onSearchKeyup(event: KeyboardEvent): void {
    if (event.key === 'Enter') {
      this.currentPage.set(1);
      this.search();
    }
  }

  public onQueryChange(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.searchQuery.set(value);
  }

  public onYearChange(value: number | null): void {
    this.yearFilter.set(value ?? null);
    if (this.searchQuery().trim()) {
      this.search();
    }
  }

  public onMovieClick(movie: MovieSearchResult): void {
    const id = movie.isLocal && movie.localId 
      ? movie.localId 
      : movie.tmdbId?.toString();
    
    if (id) {
      this.router.navigate(['/movies', id]);
    }
  }

  private buildQueryParams(page: number = 1): MovieSearchParams {
    const query = this.searchQuery().trim();
    const year = this.yearFilter();

    const params: MovieSearchParams = { 
      query,
      page
    };

    if (year) {
      params.year = year;
    }
    return params;
  }
}
