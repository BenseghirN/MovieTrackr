import { CommonModule } from '@angular/common';
import { Component, computed, effect, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextModule } from 'primeng/inputtext';
import { PaginatorModule, PaginatorState } from 'primeng/paginator';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { PeopleService } from '../../services/people.service';
import { TmdbImageService } from '../../../../core/services/tmdb-image.service';
import { SearchPeopleParams, SearchPeopleResponse, SearchPeopleResult } from '../../models/people.model';
import { toSignal } from '@angular/core/rxjs-interop';
import { PersonCardComponent } from '../../components/person-card/person-card.component';
import { Title } from '@angular/platform-browser';

@Component({
  selector: 'app-people-page',
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
    PersonCardComponent
  ],
  templateUrl: './people.page.html',
  styleUrl: './people.page.scss',
})
export class PeoplePage {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly title = inject(Title);
  private readonly peopleService = inject(PeopleService);
  private readonly imageService = inject(TmdbImageService);

  readonly searchQuery = signal('');

  readonly people = signal<SearchPeopleResult[]>([]);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  readonly currentPage = signal(1);
  readonly pageSize = signal(20);
  readonly totalResults = signal(0);

  readonly hasResults = computed(() => this.people().length > 0);
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
      const pageParam = params.get('page');
      const page = pageParam ? +pageParam : 1;

      this.searchQuery.set(queryParam);
      this.currentPage.set(Number.isNaN(page) || page < 1 ? 1 : page);

      if (!queryParam.trim()) {
        this.people.set([]);
        this.totalResults.set(0);
        return
      }
      
      this.title.setTitle(`${queryParam} | MovieTrackR`);
      this.fetchPeople(queryParam, page);
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

  onSearchKeyUp(event: KeyboardEvent): void {
    if (event.key === 'Enter') {
      this.currentPage.set(1);
      this.search();
    }
  }

  onQueryChange(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.searchQuery.set(value);
  }

  onPersonClick(person: SearchPeopleResult): void {
    const id = person.isLocal && person.id
      ? person.id
      : person.tmdbId?.toString();

    if (id) this.router.navigate(['people', id]);
  }

  private fetchPeople(query: string, page: number) {
    this.loading.set(true);
    this.error.set(null);

    this.peopleService.search({
      query: query,
      page: page,
      pageSize: this.pageSize()
    } as SearchPeopleParams).subscribe({
      next: (result : SearchPeopleResponse) => {
        this.people.set(result.items);
        this.totalResults.set(result.meta.totalResults);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.people.set([]);
        this.error.set('Impossible de charger les personnes');
      }
    });
  }

  private buildQueryParams(page: number = 1): SearchPeopleParams {
    const query = this.searchQuery().trim();

    const params: SearchPeopleParams = { 
      query: query,
      page: page,
      pageSize: this.pageSize()
    };
    return params;
  }
}
