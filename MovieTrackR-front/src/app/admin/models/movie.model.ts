import { Genre } from "../../core/models/genre.model"

export interface MovieForAdministration {
  id: string
  tmdbId: number
  title: string
  originalTitle: string
  directorId: string
  directorName: string
  year: number
  posterUrl: string
  releaseDate: string
  createdAt: string
  genres: Genre[]
}

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