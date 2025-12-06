import { CommonModule } from '@angular/common';
import { Component, computed, inject, input, output } from '@angular/core';
import { CardModule } from 'primeng/card';
import { TmdbImageService } from '../../../../core/services/tmdb-image.service';
import { SearchPeopleResult } from '../../models/people.model';

@Component({
  selector: 'app-person-card',
  standalone: true,
  imports: [ CommonModule, CardModule],
  templateUrl: './person-card.component.html',
  styleUrl: './person-card.component.scss',
})
export class PersonCardComponent {
  private readonly imageService = inject(TmdbImageService);

  readonly person = input.required<SearchPeopleResult>();
  readonly personClick = output<SearchPeopleResult>();

  readonly profileUrl = computed(() => {
    const profilePath = this.person().profilePath;
    return profilePath
      ? this.imageService.getProfileUrl(profilePath, 'w342')
      : null;
  });

  onClick(): void {
    this.personClick.emit(this.person());
  }
}
