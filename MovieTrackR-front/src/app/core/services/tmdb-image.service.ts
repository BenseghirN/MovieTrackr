import { Injectable } from '@angular/core';

@Injectable({providedIn: 'root'})
export class TmdbImageService {
  private readonly baseUrl = 'https://image.tmdb.org/t/p';

  getPosterUrl(posterPath: string | null, size: 'w92' | 'w154' | 'w185' | 'w342' | 'w500' | 'w780' | 'w1280' | 'original' = 'w342'): string {
    if (!posterPath) {
      return 'assets/images/no-poster.png';
    }
    return `${this.baseUrl}/${size}${posterPath}`;
  }

  getBackdropUrl(backdropPath: string | null, size: 'w300' | 'w780' | 'w1280' | 'original' = 'w780'): string {
    if (!backdropPath) {
      return '';
    }
    return `${this.baseUrl}/${size}${backdropPath}`;
  }
}