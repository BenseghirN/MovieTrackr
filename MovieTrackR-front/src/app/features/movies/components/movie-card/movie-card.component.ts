import { Component, EventEmitter, inject, Input, Output } from '@angular/core';
import { TmdbImageService } from '../../../../core/services/tmdb-image.service';
import { MovieSearchResult } from '../../models/movie.model';
import { CommonModule, DecimalPipe } from '@angular/common';
import { CardModule } from 'primeng/card';

@Component({
  selector: 'app-movie-card',
  imports: [CommonModule, CardModule, DecimalPipe],
  templateUrl: './movie-card.component.html',
  styleUrl: './movie-card.component.scss',
})
export class MovieCardComponent {
  @Input({ required: true }) movie!: MovieSearchResult;
  @Output() movieClick = new EventEmitter<MovieSearchResult>();

  protected readonly imageService = inject(TmdbImageService);

  onClick(): void {
    this.movieClick.emit(this.movie);
  }
}
