import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { PaginatorModule, PaginatorState } from 'primeng/paginator';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { MovieService } from '../../../../core/services/movie.service';
import { TmdbImageService } from '../../../../core/services/tmdb-image.service';
import { MovieSearchResponse, MovieSearchResult } from '../../../../core/models/movie.model';
import { DecimalPipe } from '@angular/common';

@Component({
  selector: 'app-movies-page',
  standalone: true,
  imports: [FormsModule, CardModule, ButtonModule, InputTextModule, PaginatorModule, ProgressSpinnerModule, DecimalPipe],
  templateUrl: './movies-page.html',
  styleUrl: './movies-page.scss',
})
export class MoviesPage {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly moviesService = inject(MovieService);
  protected readonly imageService = inject(TmdbImageService);

  protected readonly searchQuery = signal('');
  protected readonly movies = signal<MovieSearchResult[]>([]);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);

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

  search(): void {
    const query = this.searchQuery();
    if (!query.trim()) return;

    this.loading.set(true);
    this.error.set(null);

    this.router.navigate([], {
      queryParams: { q: query },
      queryParamsHandling: 'merge'
    });

    this.moviesService.search({
      query,
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
}
