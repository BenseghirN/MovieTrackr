import { Component, computed, inject, OnInit, signal } from '@angular/core';
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
import { DecimalPipe } from '@angular/common';
import { InputNumberModule } from 'primeng/inputnumber';

interface SortOption {
  label: string;
  value: string;
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
    DecimalPipe
  ],
  templateUrl: './movies.page.html',
  styleUrl: './movies.page.scss',
})
export class MoviesPage implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly moviesService = inject(MovieService);
  protected readonly imageService = inject(TmdbImageService);

  protected readonly searchQuery = signal('');
  protected readonly yearFilter = signal<number | null>(null);

  protected readonly movies = signal<MovieSearchResult[]>([]);
  protected readonly loading = signal(false);
  protected readonly loadingGenres = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly sortOptions: SortOption[] = [
    { label: 'Année', value: 'year' }
  ];

  protected readonly currentPage = signal(1);
  protected readonly pageSize = signal(20);
  protected readonly totalResults = signal(0);

  protected readonly hasResults = computed(() => this.movies().length > 0);
  protected readonly showNoResults = computed(() => 
    !this.loading() && this.searchQuery() && !this.hasResults()
  );

  constructor() {
    const initialQuery = this.route.snapshot.queryParams['q'] || '';
    if (initialQuery) {
      this.searchQuery.set(initialQuery);
      this.search();
    }
  }

  ngOnInit(): void {
    const params = this.route.snapshot.queryParams;
    if (params['q']) {
      this.searchQuery.set(params['q']);
      if (params['year']) this.yearFilter.set(+params['year']);
      this.search();
    }
  }

  search(): void {
    const query = this.searchQuery();
    if (!query.trim()) return;

    this.loading.set(true);
    this.error.set(null);

    const queryParams: Record<string, string | number | null> = { query: query };
    if (this.yearFilter()) queryParams['year'] = this.yearFilter();

    this.router.navigate([], { 
      queryParams, 
      queryParamsHandling: 'merge' 
    });

    this.moviesService.search({
      query,
      year: this.yearFilter() ?? undefined,
      page: this.currentPage(),
      pageSize: this.pageSize()
    }).subscribe({
      next: (response: MovieSearchResponse) => {
        this.movies.set(response.items);
        this.totalResults.set(response.meta.totalResults);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(err.message || 'Une erreur est survenue');
        this.loading.set(false);
      }
    });
  }

  onPageChange(event: PaginatorState): void {
    this.currentPage.set((event.page ?? 0) + 1);
    this.pageSize.set(event.rows ?? 20);
    this.search();
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
    this.yearFilter.set(value);
  }

    onMovieClick(movie: MovieSearchResult): void {
    const id = movie.isLocal && movie.localId 
      ? movie.localId 
      : movie.tmdbId?.toString();
    
    if (id) {
      this.router.navigate(['/movies', id]);
    }
  }
}
