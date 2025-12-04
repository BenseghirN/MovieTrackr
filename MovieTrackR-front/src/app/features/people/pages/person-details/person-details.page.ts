import { CommonModule, Location } from '@angular/common';
import { Component, computed, effect, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { CarouselModule } from 'primeng/carousel';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { SkeletonModule } from 'primeng/skeleton';
import { TmdbImageService } from '../../../../core/services/tmdb-image.service';
import { PeopleService } from '../../services/people.service';
import { PersonDetails, MovieCredit, PersonMovieCredits } from '../../models/person-details.model';
import { toSignal } from '@angular/core/rxjs-interop';
import { NotificationService } from '../../../../core/services/notification.service';
import { TooltipModule } from 'primeng/tooltip';

@Component({
  selector: 'app-person-details-page',
  standalone: true,
  imports: [CommonModule, ButtonModule, CardModule, CarouselModule, ProgressSpinnerModule, SkeletonModule, TooltipModule],
  templateUrl: './person-details.page.html',
  styleUrl: './person-details.page.scss',
})
export class PersonDetailsPage {
  private readonly route = inject(ActivatedRoute);
  private readonly location = inject(Location);
  private readonly router = inject(Router);
  private readonly peopleService = inject(PeopleService);
  private readonly notificationService = inject(NotificationService);
  public readonly imageService = inject(TmdbImageService);

  public readonly personDetails = signal<PersonDetails | null>(null);
  public readonly movieCredits = signal<MovieCredit[]>([]);

  public readonly loading = signal(false);
  public readonly loadingCredits = signal(false);
  public readonly error = signal<string | null>(null);

  public readonly castCredits = computed(() =>
    this.movieCredits().filter(c => c.creditType === 'cast')
  );

  public readonly crewCredits = computed(() =>
    this.movieCredits().filter(c => c.creditType === 'crew')
  );

  public readonly groupedCrewCredits = computed(() => {
    const crew = this.crewCredits();
    const grouped = new Map<string, {
      credit: MovieCredit;
      jobs: string[];
    }>();

    crew.forEach(credit => {
      const key = credit.tmdbMovieId?.toString() || `${credit.title}-${credit.year}`;
      
      if (grouped.has(key)) {
        grouped.get(key)!.jobs.push(credit.job || 'Unknown');
      } else {
        grouped.set(key, {
          credit: credit,
          jobs: [credit.job || 'Unknown']
        });
      }
    });

    return Array.from(grouped.values())
      .sort((a, b) => (b.credit.year || 0) - (a.credit.year || 0));
  });

  public readonly carouselResponsiveOptions = [
    { breakpoint: '1400px', numVisible: 5, numScroll: 5 },
    { breakpoint: '1200px', numVisible: 4, numScroll: 4 },
    { breakpoint: '992px', numVisible: 3, numScroll: 3 },
    { breakpoint: '768px', numVisible: 2, numScroll: 2 },
    { breakpoint: '576px', numVisible: 1, numScroll: 1 }
  ];

  private readonly routeParamsSignal = toSignal(
    this.route.paramMap,
    { initialValue: this.route.snapshot.paramMap }
  );

  constructor() {
    effect(() => {
      const params = this.routeParamsSignal();
      const personId = params?.get('id');

      if (personId) {
        this.loadPersonDetails(personId);
      }
    });
  
  }
  
  private loadPersonDetails(personId: string): void {
    this.loading.set(true);
    this.error.set(null);

    this.peopleService.getPersonByRouteId(personId).subscribe({
      next: (person: PersonDetails) => {
        this.personDetails.set(person);
        this.loading.set(false);

        this.loadMovieCredits(person.id);
      },
      error: () => {
        this.loading.set(false);
        this.error.set('Impossible de charger les détails de la personne.');
      }
    });
  }

  public goBack(): void {
    this.location.back()
  }
  
  public onMovieClick(credit: MovieCredit): void {
    console.log(credit);
    const isValidGuid = credit.movieId && 
      credit.movieId !== '00000000-0000-0000-0000-000000000000';
    const id = isValidGuid ? credit.movieId : credit.tmdbMovieId;
    console.log(id);

    if (id) {
      this.router.navigate(['/movies', id]);
    }
  }
  
  public formatDate(dateString?: string | null): string {
    if (!dateString) return '';
    const date = new Date(dateString);
    return date.toLocaleDateString('fr-FR', {
      day: 'numeric',
      month: 'long',
      year: 'numeric'
    });
  }

  public calculateAge(birthDate?: string | null, deathDate?: string | null): string {
    if (!birthDate) return '';

    const birth = new Date(birthDate);
    const end = deathDate ? new Date(deathDate) : new Date();
    const age = end.getFullYear() - birth.getFullYear();

    return deathDate
      ? `(décédé à ${age} ans)`
      : `(${age} ans)`;
  }
  
  private loadMovieCredits(personId: string): void {
    this.loadingCredits.set(true);
  
    this.peopleService.getPersonMovieCredits(personId).subscribe({
      next: (credits: PersonMovieCredits) => {
        this.movieCredits.set(credits);
        this.loadingCredits.set(false);
      },
      error: (err) => {
        this.loadingCredits.set(false);
        this.notificationService.error(`Erreur lors du chargement des crédits: ${err}`);
      }
    });
  }
}
