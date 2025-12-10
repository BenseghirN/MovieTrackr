export interface CreateMovieModelForAdministration {
  title: string;
  tmdbId: number;
  originalTitle: string;
  year: number;
  posterUrl: string;
  backdropPath: string;
  trailerUrl: string;
  tagline: string;
  duration: number;
  overview: string;
  releaseDate: string;
  voteAverage: number;
  genreIds: string[];
}

export interface UpdateMovieModelForAdministration {
  title: string;
  originalTitle: string;
  year: number;
  posterUrl: string;
  backdropPath: string;
  trailerUrl: string;
  tagline: string;
  duration: number;
  overview: string;
  voteAverage: number;
  releaseDate: string;
  genreIds: string[];
}