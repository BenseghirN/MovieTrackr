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
import { SearchMovieParams, SearchMovieResponse, SearchMovieResult } from '../../models/movie.model';
import { InputNumberModule } from 'primeng/inputnumber';
import { MovieCardComponent } from '../../components/movie-card/movie-card.component';
import { toSignal } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-movies-page',
  standalone: true,
  imports: [
    CommonModule,
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
  readonly imageService = inject(TmdbImageService);

  readonly searchQuery = signal('');
  readonly yearFilter = signal<number | null>(null);

  readonly movies = signal<SearchMovieResult[]>([]);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  readonly currentPage = signal(1);
  readonly pageSize = signal(20);
  readonly totalResults = signal(0);

  readonly hasResults = computed(() => this.movies().length > 0);
  readonly showNoResults = computed(() => 
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

  search(): void {
    if (!this.searchQuery().trim()) return;
    this.currentPage.set(1);

    this.router.navigate([], { 
      relativeTo: this.route,
      queryParams: this.buildQueryParams(), 
      queryParamsHandling: 'merge' 
    });
  }

  onPageChange(event: PaginatorState): void {
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

  onSearchKeyup(event: KeyboardEvent): void {
    if (event.key === 'Enter') {
      this.currentPage.set(1);
      this.search();
    }
  }

  onQueryChange(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.searchQuery.set(value);
  }

  onYearChange(value: number | null): void {
    this.yearFilter.set(value ?? null);
    if (this.searchQuery().trim()) {
      this.search();
    }
  }

  onMovieClick(movie: SearchMovieResult): void {
    const id = movie.isLocal && movie.localId 
      ? movie.localId 
      : movie.tmdbId?.toString();
    
    if (id) this.router.navigate(['/movies', id]);
  }

  private buildQueryParams(page: number = 1): SearchMovieParams {
    const query = this.searchQuery().trim();
    const year = this.yearFilter();

    const params: SearchMovieParams = { 
      query: query,
      page: page,
      pageSize: this.pageSize()
    };

    if (year) {
      params.year = year;
    }
    return params;
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
        next: (result: SearchMovieResponse) => {
          this.movies.set(result.items);
          this.totalResults.set(result.meta.totalResults);
          this.loading.set(false);
        },
        error: () => {
          this.loading.set(false);
          this.movies.set([]);
          this.error.set('Impossible de charger les films');
        },
      });
  }
}
