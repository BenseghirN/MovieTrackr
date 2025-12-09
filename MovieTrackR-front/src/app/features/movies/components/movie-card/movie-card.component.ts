import { Component, computed, inject, input, output } from '@angular/core';
import { TmdbImageService } from '../../../../core/services/tmdb-image.service';
import { SearchMovieResult } from '../../models/movie.model';
import { CommonModule, DecimalPipe } from '@angular/common';
import { MovieSummary } from '../../../user-lists/models/user-list.model';

@Component({
  selector: 'app-movie-card',
  imports: [CommonModule, DecimalPipe],
  templateUrl: './movie-card.component.html',
  styleUrl: './movie-card.component.scss',
})
export class MovieCardComponent {
  readonly movie = input.required<SearchMovieResult | MovieSummary>();
  readonly movieClick = output<SearchMovieResult | MovieSummary>();
  readonly imageService = inject(TmdbImageService);

  readonly posterPath = computed(() => {
    const m = this.movie();
    return 'posterPath' in m ? m.posterPath : m.posterUrl;
  });

  readonly rating = computed(() => {
    const m = this.movie();
    return 'voteAverage' in m ? m.voteAverage : null;
  });

  onClick(): void {
    this.movieClick.emit(this.movie());
  }
}
