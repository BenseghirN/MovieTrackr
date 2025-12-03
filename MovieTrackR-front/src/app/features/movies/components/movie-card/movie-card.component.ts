import { Component, computed, EventEmitter, inject, input, Input, output, Output } from '@angular/core';
import { TmdbImageService } from '../../../../core/services/tmdb-image.service';
import { MovieSearchResult } from '../../models/movie.model';
import { CommonModule, DecimalPipe } from '@angular/common';
import { CardModule } from 'primeng/card';
import { MovieSummary } from '../../../user-lists/models/user-list.model';

@Component({
  selector: 'app-movie-card',
  imports: [CommonModule, CardModule, DecimalPipe],
  templateUrl: './movie-card.component.html',
  styleUrl: './movie-card.component.scss',
})
export class MovieCardComponent {
  public movie = input.required<MovieSearchResult | MovieSummary>();
  public movieClick = output<MovieSearchResult | MovieSummary>();
  public readonly imageService = inject(TmdbImageService);

  public readonly posterPath = computed(() => {
    const m = this.movie();
    return 'posterPath' in m ? m.posterPath : m.posterUrl;
  });

  public readonly rating = computed(() => {
    const m = this.movie();
    return 'voteAverage' in m ? m.voteAverage : null;
  });

  public onClick(): void {
    this.movieClick.emit(this.movie());
  }
}
