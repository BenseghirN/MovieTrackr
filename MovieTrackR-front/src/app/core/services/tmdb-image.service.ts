import { Injectable } from '@angular/core';

type PosterSize = 'w92' | 'w154' | 'w185' | 'w342' | 'w500' | 'w780' | 'w1280' | 'original';
type BackdropSize = 'w300' | 'w780' | 'w1280' | 'original';
type ProfileSize = 'w45' | 'w185' | 'w342' | 'w500' | 'h632' | 'original';

@Injectable({ providedIn: 'root' })
export class TmdbImageService {
  private readonly baseUrl = 'https://image.tmdb.org/t/p';
  private readonly placeholderPoster = 'assets/images/no-poster.png';
  private readonly placeholderBackdrop = 'assets/images/no-backdrop.png';
  private readonly placeholderProfile = 'assets/images/no-profile.png';

  getPosterUrl(posterPath: string | null | undefined, size: PosterSize = 'w500'): string {
    if (!posterPath) {
      return this.placeholderPoster;
    }
    return `${this.baseUrl}/${size}${posterPath}`;
  }

  getBackdropUrl(backdropPath: string | null | undefined, size: BackdropSize = 'w1280'): string {
    if (!backdropPath) {
      return this.placeholderBackdrop;
    }
    return `${this.baseUrl}/${size}${backdropPath}`;
  }

  getProfileUrl(profilePath: string | null | undefined, size: ProfileSize = 'w185'): string {
    if (!profilePath) {
      return this.placeholderProfile;
    }
    return `${this.baseUrl}/${size}${profilePath}`;
  }

  getPlaceholderPoster(): string {
    return this.placeholderPoster;
  }

  getPlaceholderBackdrop(): string {
    return this.placeholderBackdrop;
  }

  getPlaceholderProfile(): string {
    return this.placeholderProfile;
  }
}